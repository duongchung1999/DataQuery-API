using Data_Query_API.DataProcessing.DataQuery;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Data_Query_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DyTestPerformency : ControllerBase
    {
        /// <summary>
        /// 获取指定条件测试效率 Performency
        /// </summary>
        /// <param name="model">传入机型名称</param>
        /// <returns></returns>
        [HttpPost("GetPerformency")]
        public async Task<ApiResponse> GetPerformency([FromForm] string model)
        {
            return await Task.Run(async () =>
            {
                return await TestData.GetPerformencyData(model);
            });
        }
    }
}
