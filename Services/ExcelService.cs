using OfficeOpenXml;
using WpfApp.Models;

namespace WpfApp.Services;

/// <summary>
/// Excel服务类，提供Excel文件的导入导出功能
/// </summary>
public class ExcelService
{
    public ExcelService()
    {
        // 设置非商业许可
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// 从Excel文件导入工作参数
    /// </summary>
    public async Task<List<WorkParameter>> ImportFromExcelAsync(string filePath)
    {
        var parameters = new List<WorkParameter>();
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        
        if (worksheet == null || worksheet.Dimension == null)
            return parameters;

        // 读取表头（第一行）
        var headers = Enumerable.Range(1, worksheet.Dimension.Columns)
            .Select(col => worksheet.Cells[1, col].Text)
            .ToList();

        // 读取数据（从第二行开始）
        for (int row = 2; row <= worksheet.Dimension.Rows; row++)
        {
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var cell = worksheet.Cells[row, col];
                if (cell.Value != null && double.TryParse(cell.Value.ToString(), out double value))
                {
                    parameters.Add(new WorkParameter
                    {
                        StepNumber = row - 1,
                        Category = headers[col - 1],
                        Value = value,
                        UpdateTime = DateTime.Now
                    });
                }
            }
        }

        return parameters;
    }

    /// <summary>
    /// 导出工作参数到Excel文件
    /// </summary>
    public async Task ExportToExcelAsync(string filePath, List<WorkParameter> parameters)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("参数设置");

        // 获取所有类别并写入表头
        var categories = parameters.Select(p => p.Category).Distinct().ToList();
        for (int col = 0; col < categories.Count; col++)
        {
            worksheet.Cells[1, col + 1].Value = categories[col];
            worksheet.Cells[1, col + 1].Style.Font.Bold = true;
        }

        // 按步骤号分组写入数据
        var groupedParams = parameters.GroupBy(p => p.StepNumber)
                                    .OrderBy(g => g.Key)
                                    .ToList();

        for (int row = 0; row < groupedParams.Count; row++)
        {
            var rowData = groupedParams[row];
            for (int col = 0; col < categories.Count; col++)
            {
                var param = rowData.FirstOrDefault(p => p.Category == categories[col]);
                if (param != null)
                {
                    worksheet.Cells[row + 2, col + 1].Value = param.Value;
                }
            }
        }

        // 设置样式
        using (var range = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column])
        {
            range.AutoFitColumns();
            range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }

        // 保存文件
        await package.SaveAsAsync(new FileInfo(filePath));
    }
} 