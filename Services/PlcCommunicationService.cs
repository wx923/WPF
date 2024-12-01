using HslCommunication;
using HslCommunication.ModBus;
using System.Collections.Concurrent;
using WpfApp.Models;

namespace WpfApp.Services;

/// <summary>
/// PLC通信服务类，提供与PLC的通信功能
/// </summary>
public class PlcCommunicationService : IDisposable
{
    private readonly ModbusTcpNet _modbusTcp;
    private readonly ConcurrentDictionary<string, Timer> _cyclicReadTimers;
    private bool _isConnected;
    private readonly object _lock = new();

    /// <summary>
    /// 数据变化事件
    /// </summary>
    public event EventHandler<PlcDataChangedEventArgs>? DataChanged;

    /// <summary>
    /// 连接状态变化事件
    /// </summary>
    public event EventHandler<PlcConnectionEventArgs>? ConnectionStateChanged;

    /// <summary>
    /// 错误发生事件
    /// </summary>
    public event EventHandler<PlcErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// 初始化PLC通信服务
    /// </summary>
    /// <param name="ipAddress">PLC的IP地址</param>
    /// <param name="port">PLC的端口号</param>
    public PlcCommunicationService(string ipAddress, int port)
    {
        _modbusTcp = new ModbusTcpNet(ipAddress, port)
        {
            ReceiveTimeOut = 1000,  // 接收超时时间（毫秒）
            ConnectTimeOut = 2000   // 连接超时时间（毫秒）
        };
        _cyclicReadTimers = new ConcurrentDictionary<string, Timer>();
    }

    /// <summary>
    /// 异步连接到PLC
    /// </summary>
    /// <returns>连接是否成功</returns>
    public async Task<bool> ConnectAsync()
    {
        try
        {
            lock (_lock)
            {
                if (_isConnected) return true;
            }

            var result = await _modbusTcp.ConnectServerAsync();
            _isConnected = result.IsSuccess;
            
            OnConnectionStateChanged(new PlcConnectionEventArgs(_isConnected, result.Message));
            return _isConnected;
        }
        catch (Exception ex)
        {
            OnErrorOccurred(new PlcErrorEventArgs("连接失败", ex));
            return false;
        }
    }

    /// <summary>
    /// 断开与PLC的连接
    /// </summary>
    public void Disconnect()
    {
        try
        {
            lock (_lock)
            {
                if (!_isConnected) return;
                
                _modbusTcp.ConnectClose();
                _isConnected = false;
                OnConnectionStateChanged(new PlcConnectionEventArgs(false, "已断开连接"));
            }
        }
        catch (Exception ex)
        {
            OnErrorOccurred(new PlcErrorEventArgs("断开连接失败", ex));
        }
    }

    /// <summary>
    /// 异步读取16位整数
    /// </summary>
    /// <param name="address">PLC地址（如"D0"）</param>
    /// <returns>读取的值，失败返回null</returns>
    public async Task<short?> ReadInt16Async(string address)
    {
        try
        {
            var result = await _modbusTcp.ReadInt16Async(address);
            if (result.IsSuccess)
            {
                OnDataChanged(new PlcDataChangedEventArgs(address, result.Content));
                return result.Content;
            }
            return null;
        }
        catch (Exception ex)
        {
            OnErrorOccurred(new PlcErrorEventArgs($"读取地址 {address} 失败", ex));
            return null;
        }
    }

    /// <summary>
    /// 启动周期性读取
    /// </summary>
    /// <param name="address">PLC地址</param>
    /// <param name="interval">读取间隔时间</param>
    /// <param name="callback">数据回调处理函数</param>
    public void StartCyclicRead(string address, TimeSpan interval, Action<short?> callback)
    {
        var timer = new Timer(async _ =>
        {
            var value = await ReadInt16Async(address);
            callback(value);
        }, null, TimeSpan.Zero, interval);

        _cyclicReadTimers.TryAdd(address, timer);
    }

    /// <summary>
    /// 停止指定地址的周期性读取
    /// </summary>
    /// <param name="address">要停止的PLC地址</param>
    public void StopCyclicRead(string address)
    {
        if (_cyclicReadTimers.TryRemove(address, out var timer))
        {
            timer.Dispose();
        }
    }

    protected virtual void OnDataChanged(PlcDataChangedEventArgs e)
    {
        DataChanged?.Invoke(this, e);
    }

    protected virtual void OnConnectionStateChanged(PlcConnectionEventArgs e)
    {
        ConnectionStateChanged?.Invoke(this, e);
    }

    protected virtual void OnErrorOccurred(PlcErrorEventArgs e)
    {
        ErrorOccurred?.Invoke(this, e);
    }

    public void Dispose()
    {
        foreach (var timer in _cyclicReadTimers.Values)
        {
            timer.Dispose();
        }
        _cyclicReadTimers.Clear();
        
        Disconnect();
        _modbusTcp.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// PLC数据变化事件参数
/// </summary>
public class PlcDataChangedEventArgs : EventArgs
{
    public string Address { get; }
    public object Value { get; }
    public DateTime Timestamp { get; }

    public PlcDataChangedEventArgs(string address, object value)
    {
        Address = address;
        Value = value;
        Timestamp = DateTime.Now;
    }
}

/// <summary>
/// PLC连接事件参数
/// </summary>
public class PlcConnectionEventArgs : EventArgs
{
    public bool IsConnected { get; }
    public string Message { get; }

    public PlcConnectionEventArgs(bool isConnected, string message)
    {
        IsConnected = isConnected;
        Message = message;
    }
}

/// <summary>
/// PLC错误事件参数
/// </summary>
public class PlcErrorEventArgs : EventArgs
{
    public string Message { get; }
    public Exception Exception { get; }

    public PlcErrorEventArgs(string message, Exception exception)
    {
        Message = message;
        Exception = exception;
    }
} 