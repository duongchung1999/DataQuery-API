namespace Data_Query_API.Model
{
    /// <summary>
    /// 数据上传参数类
    /// </summary>
    public class TsetDataUploadValue
    {
        /// <summary>
        /// 机型名称
        /// </summary>
        public string? ModelValue { get; set; }

        /// <summary>
        /// SN
        /// </summary>
        public string? SN { get; set; }
        /// <summary>
        /// 测试结果
        /// </summary>
        public string? Result { get; set; }
        /// <summary>
        /// 测试站别
        /// </summary>
        public string? Station { get; set; }

        /// <summary>
        /// 工单
        /// </summary>
        public string? workorders { get; set; }

        /// <summary>
        /// 电脑编号
        /// </summary>
        public string? MachineName { get; set; }

        /// <summary>
        /// 测试时间
        /// </summary>
        public string? TestTime { get; set; }

        /// <summary>
        /// 线材使用次数
        /// </summary>
        public string? WireNumber { get; set; }

        /// <summary>
        /// 测试数据
        /// </summary>
        public string? TestLog { get; set; }
    }
}
