using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;

namespace Data_Query_API.DataProcessing.RealTimeData
{
    /// <summary>
    /// 实时数据处理类
    /// </summary>
    public class RealTimeDataProcessing
    {
        #region ClearStatistics
        /// <summary>
        /// 清空所有实时统计信息
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> ClearStatistics()
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                string sql = "truncate Test_success_table;truncate Test_failure_table;truncate Test_statistics;truncate Computer_statistics";
                ApiResponse result = await context.commonExecute(sql);
                return result;
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(500,ex.Message);
            }
        }
        #endregion
    }
}
