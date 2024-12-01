using MongoDB.Bson;

namespace WpfApp.Models;

public class PlcData
{
    public ObjectId Id { get; set; }
    public DateTime Timestamp { get; set; }

    // 门锁状态
    public bool Door1Lock { get; set; }
    public bool Door2Lock { get; set; }

    // 炉门气缸状态
    public bool FurnaceVerticalCylinder { get; set; }
    public bool FurnaceHorizontalCylinder { get; set; }

    // 区域料位状态
    public bool SmallCarHasMaterial { get; set; }
    public bool Buffer1HasMaterial { get; set; }
    public bool Buffer2HasMaterial { get; set; }
    public bool ClampHasMaterial { get; set; }

    // 机械手水平一轴位置状态
    public bool Arm1HorizontalForwardLimit { get; set; }
    public bool Arm1HorizontalBackwardLimit { get; set; }
    public bool Arm1HorizontalOriginLimit { get; set; }
    public int Arm1HorizontalUpperLimit { get; set; }
    public int Arm1HorizontalLowerLimit { get; set; }
    public int Arm1HorizontalOriginPosition { get; set; }
    public int Arm1HorizontalCurrentPosition { get; set; }
    public int Arm1HorizontalCurrentSpeed { get; set; }

    // 机械手水平二轴位置状态
    public bool Arm2HorizontalForwardLimit { get; set; }
    public bool Arm2HorizontalBackwardLimit { get; set; }
    public bool Arm2HorizontalOriginLimit { get; set; }
    public int Arm2HorizontalUpperLimit { get; set; }
    public int Arm2HorizontalLowerLimit { get; set; }
    public int Arm2HorizontalOriginPosition { get; set; }
    public int Arm2HorizontalCurrentPosition { get; set; }
    public int Arm2HorizontalCurrentSpeed { get; set; }

    // 机械手垂直轴位置状态
    public bool ArmVerticalUpperLimit { get; set; }
    public bool ArmVerticalLowerLimit { get; set; }
    public bool ArmVerticalOriginLimit { get; set; }
    public int ArmVerticalUpperLimitPosition { get; set; }
    public int ArmVerticalLowerLimitPosition { get; set; }
    public int ArmVerticalOriginPosition { get; set; }
    public int ArmVerticalCurrentPosition { get; set; }
    public int ArmVerticalCurrentSpeed { get; set; }

    // 夹具位置状态
    public bool ClampHorizontalForwardLimit { get; set; }
    public bool ClampHorizontalBackwardLimit { get; set; }
    public bool ClampHorizontalOriginLimit { get; set; }
    public bool ClampVerticalUpperLimit { get; set; }
    public bool ClampVerticalLowerLimit { get; set; }
    public bool ClampVerticalOriginLimit { get; set; }

    // 夹具位置数值
    public int ClampHorizontalUpperLimit { get; set; }
    public int ClampHorizontalLowerLimit { get; set; }
    public int ClampHorizontalOriginPosition { get; set; }
    public int ClampVerticalUpperLimit { get; set; }
    public int ClampVerticalLowerLimit { get; set; }
    public int ClampVerticalOriginPosition { get; set; }
    public int ClampHorizontalCurrentPosition { get; set; }
    public int ClampVerticalCurrentPosition { get; set; }
    public int ClampHorizontalCurrentSpeed { get; set; }
    public int ClampVerticalCurrentSpeed { get; set; }

    // 炉内状态
    public bool FurnaceStatus { get; set; }
} 