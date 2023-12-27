using Data_Query_API.DataProcessing.BackgroundData;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// 后台数据获取接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundDataController : ControllerBase
    {
        /// <summary>
        /// 获取后台所有有上传测试数据的机型和站别
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUploadDataModelStation")]
        public async Task<ApiResponse> GetUploadDataModelStation()
        {
            return await Background.GetUploadDataModelStation();
        }

        /// <summary>
        /// 获取后台所有有上传LG测试数据的机型和站别
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetUploadLGDataModelStation")]
        public async Task<ApiResponse> GetUploadLGDataModelStation()
        {
            return await Background.GetUploadLGDataModelStation();
        }
        /// <summary>
        /// 根据用户ID获取用户所有机型
        /// </summary>
        /// <param name="UserID">传入用户Id</param>
        /// <returns></returns>
        [HttpPost("GetUserModel")]
        public async Task<ApiResponse> GetUserModel([FromForm]string UserID)
        {
            return await Background.GetUserModel(UserID);
        }

        /// <summary>
        /// 根据用户ID获取用户所有有上传声学数据的机型
        /// </summary>
        /// <param name="UserID">传入用户Id</param>
        /// <returns></returns>
        [HttpPost("GetUploadAEDataUserModel")]
        public async Task<ApiResponse> GetUploadAEDataUserModel([FromForm] string UserID)
        {
            return await Background.GetUploadAEDataUserModel(UserID);
        }

        /// <summary>
        /// 根据机型名称获取后台有上传测试数据的站别
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <returns></returns>
        [HttpPost("GetStation")]
        public async Task<ApiResponse> GetStation([FromForm] string ModelName)
        {
            return await Background.GetStation(ModelName);
        }

        /// <summary>
        /// 根据机型名称获取后台有上传Logi测试数据的站别
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <returns></returns>
        [HttpPost("GetLGStation")]
        public async Task<ApiResponse> GetLGStation([FromForm] string ModelName)
        {
            return await Background.GetLGStation(ModelName);
        }

        /// <summary>
        /// 根据机型和站别名称获取后台站别测试项目
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <returns></returns>
        [HttpPost("GetStationTestItem")]
        public async Task<ApiResponse> GetStationTestItem([FromForm] string ModelName, [FromForm] string StationName)
        {
            return await Background.GetStationTestItem(ModelName, StationName);
        }

        /// <summary>
        /// 根据机型和站别名称获取后台站别Config
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <returns></returns>
        [HttpPost("GetStationConfig")]
        public async Task<ApiResponse> GetStationConfig([FromForm] string ModelName, [FromForm] string StationName)
        {
            return await Background.GetStationConfig(ModelName, StationName);
        }

        /// <summary>
        /// 根据料号获取对应信息
        /// </summary>
        /// <param name="ItemNumber">传入料号</param>
        /// <returns></returns>
        [HttpPost("GetItemNumberInfo")]
        public async Task<ApiResponse> GetItemNumberInfo([FromForm] string ItemNumber)
        {
            return await Background.GetItemNumberInfo(ItemNumber);
        }
    }
}
