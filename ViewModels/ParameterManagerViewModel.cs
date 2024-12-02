using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels;

public partial class ParameterManagerViewModel : ObservableObject
{
    private readonly ExcelService _excelService;
    private readonly MongoDbService _mongoDbService;

    [ObservableProperty]
    private ObservableCollection<WorkParameter> _parameters = new();

    [ObservableProperty]
    private WorkParameter? _selectedParameter;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private string _selectedFilePath = "未选择文件";

    public ParameterManagerViewModel(ExcelService excelService, MongoDbService mongoDbService)
    {
        _excelService = excelService;
        _mongoDbService = mongoDbService;
    }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Excel Files|*.xlsx;*.xls",
            Title = "选择Excel参数文件",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                SelectedFilePath = dialog.FileName;
                StatusMessage = "正在导入...";
                
                var importedParams = await _excelService.ImportFromExcelAsync(dialog.FileName);
                if (!importedParams.Any())
                {
                    StatusMessage = "文件中没有找到有效数据";
                    return;
                }

                // 保存到数据库
                var parameterFile = new WorkParameterFile
                {
                    FileName = Path.GetFileName(dialog.FileName),
                    ImportTime = DateTime.Now,
                    Parameters = importedParams
                };
                
                await _mongoDbService.SaveWorkParameterFileAsync(parameterFile);

                // 更新UI
                Parameters.Clear();
                foreach (var param in importedParams)
                {
                    Parameters.Add(param);
                }

                StatusMessage = $"成功导入 {importedParams.Count} 条参数";
            }
            catch (Exception ex)
            {
                StatusMessage = $"导入失败: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        if (!Parameters.Any())
        {
            StatusMessage = "没有数据可供导出";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Excel Files|*.xlsx",
            Title = "保存Excel文件",
            DefaultExt = ".xlsx",
            AddExtension = true
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                StatusMessage = "正在导出...";
                await _excelService.ExportToExcelAsync(dialog.FileName, Parameters.ToList());
                StatusMessage = "导出成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出失败: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void AddParameter()
    {
        var newParam = new WorkParameter
        {
            StepNumber = Parameters.Count + 1,
            UpdateTime = DateTime.Now
        };
        Parameters.Add(newParam);
        SelectedParameter = newParam;
    }

    [RelayCommand]
    private async Task DeleteParameterAsync()
    {
        if (SelectedParameter == null) return;

        try
        {
            await _mongoDbService.DeleteWorkParameterAsync(SelectedParameter.Id);
            Parameters.Remove(SelectedParameter);
            StatusMessage = "删除成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        try
        {
            StatusMessage = "正在保存...";
            await _mongoDbService.UpdateWorkParametersAsync(Parameters.ToList());
            StatusMessage = "保存成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存失败: {ex.Message}";
        }
    }
} 