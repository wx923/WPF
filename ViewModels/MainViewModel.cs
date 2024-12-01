using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

/// <summary>
/// 主窗口的ViewModel，负责处理UI的业务逻辑和数据状态管理
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    // 服务依赖
    private readonly PlcCommunicationService _plcService;        // PLC通信服务
    private readonly MongoDbService _mongoDbService;             // MongoDB数据服务
    private readonly DispatcherTimer _databaseSaveTimer;         // 数据库保存定时器
    private PlcData _currentData = new();                       // 当前PLC数据缓存
    private bool _isDisposed;                                   // 资源释放标志

    // 可观察属性，用于UI绑定
    [ObservableProperty]
    private double _temperature;    // 温度值

    [ObservableProperty]
    private double _pressure;       // 压力值

    [ObservableProperty]
    private bool _status;          // 状态值

    [ObservableProperty]
    private bool _isConnected;     // PLC连接状态

    [ObservableProperty]
    private string _connectionMessage = "未连接";    // 连接状态消息

    [ObservableProperty]
    private ObservableCollection<PlcData> _historicalData = new();    // 历史数据集合

    #region PLC数据属性
    // 门锁状态
    [ObservableProperty]
    private bool _door1Lock;

    [ObservableProperty]
    private bool _door2Lock;

    // 炉门气缸状态
    [ObservableProperty]
    private bool _furnaceVerticalCylinder;

    [ObservableProperty]
    private bool _furnaceHorizontalCylinder;

    // 区域料位状态
    [ObservableProperty]
    private bool _smallCarHasMaterial;

    [ObservableProperty]
    private bool _buffer1HasMaterial;

    [ObservableProperty]
    private bool _buffer2HasMaterial;

    [ObservableProperty]
    private bool _clampHasMaterial;

    // 机械手水平一轴位置状态
    [ObservableProperty]
    private bool _arm1HorizontalForwardLimit;

    [ObservableProperty]
    private bool _arm1HorizontalBackwardLimit;

    [ObservableProperty]
    private bool _arm1HorizontalOriginLimit;

    [ObservableProperty]
    private int _arm1HorizontalUpperLimit;

    [ObservableProperty]
    private int _arm1HorizontalLowerLimit;

    [ObservableProperty]
    private int _arm1HorizontalOriginPosition;

    [ObservableProperty]
    private int _arm1HorizontalCurrentPosition;

    [ObservableProperty]
    private int _arm1HorizontalCurrentSpeed;

    // 机械手水平二轴位置状态
    [ObservableProperty]
    private bool _arm2HorizontalForwardLimit;

    [ObservableProperty]
    private bool _arm2HorizontalBackwardLimit;

    [ObservableProperty]
    private bool _arm2HorizontalOriginLimit;

    [ObservableProperty]
    private int _arm2HorizontalUpperLimit;

    [ObservableProperty]
    private int _arm2HorizontalLowerLimit;

    [ObservableProperty]
    private int _arm2HorizontalOriginPosition;

    [ObservableProperty]
    private int _arm2HorizontalCurrentPosition;

    [ObservableProperty]
    private int _arm2HorizontalCurrentSpeed;

    // 机械手垂直轴位置状态
    [ObservableProperty]
    private bool _armVerticalUpperLimit;

    [ObservableProperty]
    private bool _armVerticalLowerLimit;

    [ObservableProperty]
    private bool _armVerticalOriginLimit;

    [ObservableProperty]
    private int _armVerticalUpperLimitPosition;

    [ObservableProperty]
    private int _armVerticalLowerLimitPosition;

    [ObservableProperty]
    private int _armVerticalOriginPosition;

    [ObservableProperty]
    private int _armVerticalCurrentPosition;

    [ObservableProperty]
    private int _armVerticalCurrentSpeed;

    // 夹具位置状态
    [ObservableProperty]
    private bool _clampHorizontalForwardLimit;

    [ObservableProperty]
    private bool _clampHorizontalBackwardLimit;

    [ObservableProperty]
    private bool _clampHorizontalOriginLimit;

    [ObservableProperty]
    private bool _clampVerticalUpperLimit;

    [ObservableProperty]
    private bool _clampVerticalLowerLimit;

    [ObservableProperty]
    private bool _clampVerticalOriginLimit;

    // 夹具位置数值
    [ObservableProperty]
    private int _clampHorizontalUpperLimit;

    [ObservableProperty]
    private int _clampHorizontalLowerLimit;

    [ObservableProperty]
    private int _clampHorizontalOriginPosition;

    [ObservableProperty]
    private int _clampVerticalUpperLimit;

    [ObservableProperty]
    private int _clampVerticalLowerLimit;

    [ObservableProperty]
    private int _clampVerticalOriginPosition;

    [ObservableProperty]
    private int _clampHorizontalCurrentPosition;

    [ObservableProperty]
    private int _clampVerticalCurrentPosition;

    [ObservableProperty]
    private int _clampHorizontalCurrentSpeed;

    [ObservableProperty]
    private int _clampVerticalCurrentSpeed;

    // 炉内状态
    [ObservableProperty]
    private bool _furnaceStatus;
    #endregion

    /// <summary>
    /// 构造函数，初始化ViewModel
    /// </summary>
    /// <param name="plcService">PLC通信服务</param>
    /// <param name="mongoDbService">MongoDB数据服务</param>
    public MainViewModel(PlcCommunicationService plcService, MongoDbService mongoDbService)
    {
        _plcService = plcService;
        _mongoDbService = mongoDbService;

        // 注册PLC服务事件处理器
        _plcService.ConnectionStateChanged += OnConnectionStateChanged;
        _plcService.ErrorOccurred += OnErrorOccurred;

        // 初始化数据库保存定时器（每秒保存一次）
        _databaseSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _databaseSaveTimer.Tick += DatabaseSaveTimer_Tick;
    }

    /// <summary>
    /// 初始化周期性数据读取
    /// </summary>
    private void InitializeCyclicReading()
    {
        // 读取门锁状态
        _plcService.StartCyclicRead("M100", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Door1Lock = value.Value > 0;
                _currentData.Door1Lock = Door1Lock;
            }
        });

        _plcService.StartCyclicRead("M101", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Door2Lock = value.Value > 0;
                _currentData.Door2Lock = Door2Lock;
            }
        });

        // 读取炉门气缸状态
        _plcService.StartCyclicRead("M102", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                FurnaceVerticalCylinder = value.Value > 0;
                _currentData.FurnaceVerticalCylinder = FurnaceVerticalCylinder;
            }
        });

        _plcService.StartCyclicRead("M103", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                FurnaceHorizontalCylinder = value.Value > 0;
                _currentData.FurnaceHorizontalCylinder = FurnaceHorizontalCylinder;
            }
        });

        // 读取区域料位状态
        _plcService.StartCyclicRead("M104", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                SmallCarHasMaterial = value.Value > 0;
                _currentData.SmallCarHasMaterial = SmallCarHasMaterial;
            }
        });

        _plcService.StartCyclicRead("M105", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Buffer1HasMaterial = value.Value > 0;
                _currentData.Buffer1HasMaterial = Buffer1HasMaterial;
            }
        });

        _plcService.StartCyclicRead("M106", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Buffer2HasMaterial = value.Value > 0;
                _currentData.Buffer2HasMaterial = Buffer2HasMaterial;
            }
        });

        _plcService.StartCyclicRead("M107", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                ClampHasMaterial = value.Value > 0;
                _currentData.ClampHasMaterial = ClampHasMaterial;
            }
        });

        // 读取机械手水平一轴位置状态
        _plcService.StartCyclicRead("M108", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Arm1HorizontalForwardLimit = value.Value > 0;
                _currentData.Arm1HorizontalForwardLimit = Arm1HorizontalForwardLimit;
            }
        });

        // 读取位置数值
        _plcService.StartCyclicRead("D100", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Arm1HorizontalUpperLimit = value.Value;
                _currentData.Arm1HorizontalUpperLimit = value.Value;
            }
        });

        _plcService.StartCyclicRead("D101", TimeSpan.FromMilliseconds(300), value =>
        {
            if (value.HasValue)
            {
                Arm1HorizontalLowerLimit = value.Value;
                _currentData.Arm1HorizontalLowerLimit = value.Value;
            }
        });
    }

    /// <summary>
    /// 数据库保存定时器回调，定期保存数据到MongoDB
    /// </summary>
    private async void DatabaseSaveTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isConnected) return;

        try
        {
            var dataToSave = new PlcData
            {
                Timestamp = DateTime.Now,
                
                // 门锁状态
                Door1Lock = _currentData.Door1Lock,
                Door2Lock = _currentData.Door2Lock,

                // 炉门气缸状态
                FurnaceVerticalCylinder = _currentData.FurnaceVerticalCylinder,
                FurnaceHorizontalCylinder = _currentData.FurnaceHorizontalCylinder,

                // 炉内状态
                FurnaceStatus = _currentData.FurnaceStatus
            };

            await _mongoDbService.SaveDataAsync(dataToSave);
            await LoadHistoricalDataAsync();
        }
        catch (Exception ex)
        {
            ConnectionMessage = $"数据保存失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 连接命令处理方法
    /// </summary>
    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            ConnectionMessage = "正在连接...";
            IsConnected = await _plcService.ConnectAsync();
            if (IsConnected)
            {
                InitializeCyclicReading();
                _databaseSaveTimer.Start();
                await LoadHistoricalDataAsync();
                ConnectionMessage = "已连接";
            }
            else
            {
                ConnectionMessage = "连接失败，请检查PLC地址和端口是否正确";
            }
        }
        catch (Exception ex)
        {
            ConnectionMessage = $"连接错误: {ex.Message}";
            IsConnected = false;
        }
    }

    /// <summary>
    /// 断开连接命令处理方法
    /// </summary>
    [RelayCommand]
    private void Disconnect()
    {
        if (!IsConnected) return;

        _databaseSaveTimer.Stop();
        _plcService.Disconnect();
        IsConnected = false;
        ConnectionMessage = "已断开连接";
    }

    /// <summary>
    /// 加载历史数据
    /// </summary>
    private async Task LoadHistoricalDataAsync()
    {
        try
        {
            var data = await _mongoDbService.GetLatestDataAsync(10);
            Application.Current.Dispatcher.Invoke(() =>
            {
                HistoricalData.Clear();
                foreach (var item in data)
                {
                    HistoricalData.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            ConnectionMessage = $"加载历史数据失败: {ex.Message}";
        }
    }

    /// <summary>
    /// PLC连接状态变化事件处理
    /// </summary>
    private void OnConnectionStateChanged(object? sender, PlcConnectionEventArgs e)
    {
        IsConnected = e.IsConnected;
        ConnectionMessage = e.Message;
    }

    /// <summary>
    /// PLC错误事件处理
    /// </summary>
    private void OnErrorOccurred(object? sender, PlcErrorEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            ConnectionMessage = $"错误: {e.Message}";
            if (e.Message.Contains("连接失败") || e.Message.Contains("读取地址"))
            {
                IsConnected = false;
                _databaseSaveTimer.Stop();
            }
        });
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        // 停止所有定时器和数据读取
        _databaseSaveTimer.Stop();
        _plcService.StopCyclicRead("D0");
        _plcService.StopCyclicRead("D2");
        _plcService.StopCyclicRead("M0");
        
        // 取消事件订阅
        _plcService.ConnectionStateChanged -= OnConnectionStateChanged;
        _plcService.ErrorOccurred -= OnErrorOccurred;

        _isDisposed = true;
    }
} 