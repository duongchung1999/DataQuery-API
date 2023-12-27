using Data_Query_API.DataProcessing.DataDailyData;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// 数据日报查询接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataDailyQueryController : ControllerBase
    {
        /// <summary>
        /// 获取日报表数据数据库所有表名
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDailyTableName")]
        public async Task<ApiResponse> GetDailyTableName()
        {
            return await DataDailyData.GetDailyTableName();
        }

        /// <summary>
        /// 根据年获取当年所有有数据的机型
        /// </summary>
        /// <param name="Year">传入年份</param>
        /// <returns></returns>
        [HttpPost("GetModelFromYear")]
        public async Task<ApiResponse> GetModelFromYear([FromForm]string Year)
        {
            return await DataDailyData.GetModelFromYear(Year);
        }

        /// <summary>
        ///  查询指定机型指定时间段每日生产数据统计
        /// </summary>
        /// <param name="Year">传入年份</param>
        /// <param name="Model">传入机型名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <returns></returns>
        [HttpPost("GetDailyData")]
        public async Task<ApiResponse> GetDailyData([FromForm] string Year, [FromForm] string Model, [FromForm] string StartTime, [FromForm] string EndTime)
        {
            return await DataDailyData.GetDailyData(Year, Model, StartTime, EndTime);
        }

        /// <summary>
        ///  查询指定机型指定时间段单日生产数据统计
        /// </summary>
        /// <param name="Year">传入年份</param>
        /// <param name="Model">传入机型名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <returns></returns>
        [HttpPost("GetDailyReportData")]
        public async Task<ApiResponse> GetDailyReportData([FromForm] string Year, [FromForm] string Model, [FromForm] string StartTime, [FromForm] string EndTime)
        {
            return await DataDailyData.GetDailyReportData(Year, Model, StartTime, EndTime);
        }

        /// <summary>
        ///  查询指定机型指定时间段单日生产数据统计
        /// </summary>
        /// <param name="ModelName">传入机型数据</param>
        /// <param name="DailyReportData">传入日报数据</param>
        /// <param name="SaveType">存储数据类型</param>
        /// <returns></returns>
        [HttpPost("SaveDailyReport")]
        public async Task<IActionResult> SaveDailyReport([FromForm] string ModelName,[FromForm] string DailyReportData, [FromForm] string SaveType)
        {
           return await Task.Run(() =>
            {
                string path = DataDailyData.SaveDailyReport(ModelName, DailyReportData, SaveType);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(path);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });

        }
    }
}
