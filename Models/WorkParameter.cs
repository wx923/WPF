using MongoDB.Bson;

namespace WpfApp.Models;

public class WorkParameter
{
    public ObjectId Id { get; set; }
    public int StepNumber { get; set; }           // 步骤号
    public string Category { get; set; } = "";    // 类别
    public double Value { get; set; }             // 值
    public string Description { get; set; } = ""; // 描述
    public DateTime UpdateTime { get; set; }      // 更新时间
}

public class WorkParameterFile
{
    public ObjectId Id { get; set; }
    public string FileName { get; set; } = "";
    public DateTime ImportTime { get; set; }
    public List<WorkParameter> Parameters { get; set; } = new();
} 