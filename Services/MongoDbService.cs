using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq.Expressions;
using WpfApp.Models;

namespace WpfApp.Services;

/// <summary>
/// MongoDB数据库服务类，提供数据存储和查询功能
/// </summary>
public class MongoDbService
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<PlcData> _plcDataCollection;
    private readonly IMongoCollection<WorkParameterFile> _workParameterFileCollection;

    /// <summary>
    /// 初始化MongoDB服务
    /// </summary>
    /// <param name="connectionString">MongoDB连接字符串</param>
    /// <param name="databaseName">数据库名称，默认为"PlcDatabase"</param>
    /// <param name="collectionName">集合名称，默认为"PlcData"</param>
    public MongoDbService(string connectionString, string databaseName = "PlcDatabase", string collectionName = "PlcData")
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
        _plcDataCollection = _database.GetCollection<PlcData>(collectionName);
        _workParameterFileCollection = _database.GetCollection<WorkParameterFile>("WorkParameterFiles");
        
        // 创建索引
        CreateIndexes();
    }

    /// <summary>
    /// 创建必要的索引
    /// </summary>
    private async void CreateIndexes()
    {
        var indexKeysDefinition = Builders<PlcData>.IndexKeys.Descending(x => x.Timestamp);
        await _plcDataCollection.Indexes.CreateOneAsync(new CreateIndexModel<PlcData>(indexKeysDefinition));
    }

    /// <summary>
    /// 保存单条PLC数据
    /// </summary>
    /// <param name="data">要保存的数据</param>
    public async Task SaveDataAsync(PlcData data)
    {
        try
        {
            await _plcDataCollection.InsertOneAsync(data);
        }
        catch (Exception ex)
        {
            // 这里可以添加日志记录
            throw new MongoDbServiceException("保存数据失败", ex);
        }
    }

    /// <summary>
    /// 批量保存PLC数据
    /// </summary>
    /// <param name="dataList">要保存的数据列表</param>
    public async Task SaveManyAsync(IEnumerable<PlcData> dataList)
    {
        try
        {
            await _plcDataCollection.InsertManyAsync(dataList);
        }
        catch (Exception ex)
        {
            throw new MongoDbServiceException("批量保存数据失败", ex);
        }
    }

    /// <summary>
    /// 获取最新的数据记录
    /// </summary>
    /// <param name="limit">返回的记录数量</param>
    /// <returns>按时间戳降序排列的数据列表</returns>
    public async Task<List<PlcData>> GetLatestDataAsync(int limit = 100)
    {
        try
        {
            return await _plcDataCollection.Find(_ => true)
                .Sort(Builders<PlcData>.Sort.Descending(x => x.Timestamp))
                .Limit(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new MongoDbServiceException("获取最新数据失败", ex);
        }
    }

    /// <summary>
    /// 按时间范围查询数据
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>指定时间范围内的数据列表</returns>
    public async Task<List<PlcData>> GetDataByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var filter = Builders<PlcData>.Filter.And(
                Builders<PlcData>.Filter.Gte(x => x.Timestamp, startTime),
                Builders<PlcData>.Filter.Lte(x => x.Timestamp, endTime)
            );

            return await _plcDataCollection.Find(filter)
                .Sort(Builders<PlcData>.Sort.Ascending(x => x.Timestamp))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new MongoDbServiceException("按时间范围查询数据失败", ex);
        }
    }

    /// <summary>
    /// 获取指定条件的数据
    /// </summary>
    /// <param name="filter">查询条件表达式</param>
    /// <returns>符合条件的数据列表</returns>
    public async Task<List<PlcData>> GetDataAsync(Expression<Func<PlcData, bool>> filter)
    {
        try
        {
            return await _plcDataCollection.Find(filter).ToListAsync();
        }
        catch (Exception ex)
        {
            throw new MongoDbServiceException("查询数据失败", ex);
        }
    }

    /// <summary>
    /// 删除指定时间之前的数据
    /// </summary>
    /// <param name="beforeTime">指定时间点</param>
    /// <returns>删除的记录数</returns>
    public async Task<long> DeleteDataBeforeAsync(DateTime beforeTime)
    {
        try
        {
            var filter = Builders<PlcData>.Filter.Lt(x => x.Timestamp, beforeTime);
            var result = await _plcDataCollection.DeleteManyAsync(filter);
            return result.DeletedCount;
        }
        catch (Exception ex)
        {
            throw new MongoDbServiceException("删除历史数据失败", ex);
        }
    }

    /// <summary>
    /// 获取数据统计信息
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>统计信息</returns>
    public async Task<PlcDataStatistics> GetStatisticsAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            var filter = Builders<PlcData>.Filter.And(
                Builders<PlcData>.Filter.Gte(x => x.Timestamp, startTime),
                Builders<PlcData>.Filter.Lte(x => x.Timestamp, endTime)
            );

            var data = await _plcDataCollection.Find(filter).ToListAsync();

            return new PlcDataStatistics
            {
                Count = data.Count,
                AverageTemperature = data.Average(x => x.Temperature),
                MaxTemperature = data.Max(x => x.Temperature),
                MinTemperature = data.Min(x => x.Temperature),
                AveragePressure = data.Average(x => x.Pressure),
                MaxPressure = data.Max(x => x.Pressure),
                MinPressure = data.Min(x => x.Pressure)
            };
        }
        catch (Exception ex)
        {
            throw new MongoDbServiceException("获取统计信息失败", ex);
        }
    }

    public async Task SaveWorkParameterFileAsync(WorkParameterFile file)
    {
        await _workParameterFileCollection.InsertOneAsync(file);
    }

    public async Task UpdateWorkParametersAsync(List<WorkParameter> parameters)
    {
        var bulkOps = parameters.Select(p =>
            new ReplaceOneModel<WorkParameter>(
                Builders<WorkParameter>.Filter.Eq(x => x.Id, p.Id),
                p) { IsUpsert = true }
        );
        
        await _workParameterFileCollection.BulkWriteAsync(bulkOps);
    }

    public async Task DeleteWorkParameterAsync(ObjectId parameterId)
    {
        await _workParameterFileCollection.DeleteOneAsync(
            Builders<WorkParameterFile>.Filter.ElemMatch(
                x => x.Parameters,
                p => p.Id == parameterId
            )
        );
    }
}

/// <summary>
/// MongoDB服务异常类
/// </summary>
public class MongoDbServiceException : Exception
{
    public MongoDbServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// PLC数据统计信息类
/// </summary>
public class PlcDataStatistics
{
    /// <summary>
    /// 数据记录数量
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 平均温度
    /// </summary>
    public double AverageTemperature { get; set; }

    /// <summary>
    /// 最高温度
    /// </summary>
    public double MaxTemperature { get; set; }

    /// <summary>
    /// 最低温度
    /// </summary>
    public double MinTemperature { get; set; }

    /// <summary>
    /// 平均压力
    /// </summary>
    public double AveragePressure { get; set; }

    /// <summary>
    /// 最高压力
    /// </summary>
    public double MaxPressure { get; set; }

    /// <summary>
    /// 最低压力
    /// </summary>
    public double MinPressure { get; set; }
} 