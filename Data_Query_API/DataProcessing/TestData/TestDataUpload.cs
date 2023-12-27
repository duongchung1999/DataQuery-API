using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;

namespace Data_Query_API.DataProcessing.DataQuery
{
    /// <summary>
    /// 测试数据上传类
    /// </summary>
    public class TestDataUpload
    {
        #region 上传测试数据
        /// <summary>
        /// 上传测试数据
        /// </summary>
        /// <param name="dataValue"></param>
        /// <returns></returns>
        public static async Task<ApiResponse> UploadTestData(TsetDataUploadValue dataValue)
        {
            return await Task.Run(async() =>
            {
                try
                {
                    Mysql context = new(GetConfiguration.testLogMysql);
                    string DateTimeValue = DateTime.Now.ToString("yy-MM-dd");
                    string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTimeValue.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
                    string TableName = $"{dataValue.ModelValue.Replace('-', '_')}_{DateTimeValue.Split('-')[0]}_{Quarter}".ToLower();
                    var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = '{TableName}' and TABLE_SCHEMA = 'testlog'; ");
                    if (TestDataQuery.Rows.Count == 0)
                    {
                        ApiResponse rs = await DataTableCreation.CreateTestLogTable(dataValue.ModelValue);
                        if (rs.status != 200) return rs;
                    }
                    string Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string sql = $"INSERT into {TableName}(SN,Time,Result,Station,workorders,MachineName,TestTime,WireNumber,TestLog) VALUES" +
                        $"('{dataValue.SN}','{Time}','{dataValue.Result}','{dataValue.Station}','{dataValue.workorders}','{dataValue.MachineName}'," +
                        $"'{dataValue.TestTime}','{dataValue.WireNumber}','{dataValue.TestLog}')";

                    ApiResponse result = await context.commonExecute(sql);
                    if (result.message.Contains("Unknown column 'WireNumber' in 'field list'"))
                    {
                        sql = $"alter table {TableName} Add column WireNumber VARCHAR(10) DEFAULT NULL COMMENT '线材使用次数';{sql}";
                        Log.LogWrite($"创建字段 电脑编号:{dataValue.MachineName} 机型：{dataValue.ModelValue} 站别：{dataValue.Station}", "data_upload");
                        result = await context.commonExecute(sql);
                    }
                    else if (result.status != 200)
                    {
                        result = await context.commonExecute(sql);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"上传测试数据(Test_Data_Upload)异常，异常信息{ex.Message}", "data_upload");
                    return ApiResponse.Error(500, $"{ex.Message}");
                }
            });
        }
        #endregion

        #region 获取服务器当前时间
        /// <summary>
        /// 获取服务器当前时间
        /// </summary>
        /// <returns></returns>
        public static string Get_date_time()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion
    }
}
