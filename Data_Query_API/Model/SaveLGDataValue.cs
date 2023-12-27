namespace Data_Query_API.Model
{
    /// <summary>
    /// 保存 LG 测试数据内容
    /// </summary>
    public class SaveLGDataValue
    {
        /// <summary>
        /// 机型名称
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// 查询开始时间
        /// </summary>
        public string? StartTime { get; set; }

        /// <summary>
        /// 查询结束时间
        /// </summary>
        public string? EndTime { get; set; }

        /// <summary>
        /// 测试数据
        /// </summary>
        public List<Dictionary<string,object>>? TestData { get; set; }

        /// <summary>
        /// 测试统计数据
        /// </summary>
        public List<Dictionary<string, object>>? TestDataStatistics { get; set; }

        /// <summary>
        /// 测试项失败数据
        /// </summary>
        public List<Dictionary<string, object>>? TestFailStatistics { get; set; }
    }
}
