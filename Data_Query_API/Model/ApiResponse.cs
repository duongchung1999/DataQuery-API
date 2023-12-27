namespace Data_Query_API.Model
{
    /// <summary>
    /// API 返回值格式化
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 返回状态
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 返回数据
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 对外格式化方法
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="isSuccess"></param>
        /// <param name="Message"></param>
        /// <param name="Data"></param>
        public ApiResponse(int Status,bool isSuccess, string Message, object Data)
        {
            status = Status;
            message = Message;
            IsSuccess = isSuccess;
            data = Data;
        }

        /// <summary>
        /// 请求成功返回
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static ApiResponse OK(object Data)
        {
            return new ApiResponse(200,true, "请求成功", Data);
        }

        /// <summary>
        /// 请求失败返回
        /// </summary>
        /// <param name="code">传入失败代码</param>
        /// <param name="str">传入失败信息</param>
        /// <returns></returns>
        public static ApiResponse Error(int code ,string str)
        {
            return new ApiResponse(code, false, str, new List<string>());
        }
    }
}
