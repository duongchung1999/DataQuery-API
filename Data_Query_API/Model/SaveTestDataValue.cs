namespace Data_Query_API.Model
{
    /// <summary>
    /// 保存测试数据内容
    /// </summary>
    public class SaveTestDataValue
    {
        /// <summary>
        /// 机型名称
        /// </summary>
        public string? Model { get; set; }
        /// <summary>
        /// 测试数据
        /// </summary>
        public List<Dictionary<string,object>>? TestData { get; set; }

        /// <summary>
        /// 测试标题数据
        /// </summary>
        public List<Dictionary<string, string>>? TestDataTitle { get; set; }

        /// <summary>
        /// 测试Limit数据
        /// </summary>
        public List<Dictionary<string, string>>? TestDataLimit { get; set; }

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
