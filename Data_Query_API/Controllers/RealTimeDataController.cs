using Data_Query_API.DataProcessing.RealTimeData;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// 实时数据接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RealTimeDataController : ControllerBase
    {
        /// <summary>
        /// 清空所有实时统计信息
        /// </summary>
        /// <returns></returns>
        [HttpPost("ClearStatistics")]
        public async Task<ApiResponse> ClearStatistics()
        {
            return await RealTimeDataProcessing.ClearStatistics();
        }
    }
}
