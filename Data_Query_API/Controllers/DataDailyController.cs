using Data_Query_API.DataProcessing.LGTestData;
using Data_Query_API.DataProcessing.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// 生成日报数据接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataDailyController : ControllerBase
    {
        /// <summary>
        /// 根据传入日期生成日报表文件
        /// </summary>
        /// <param name="DateTimeValue">传入生成报表的日期</param>
        /// <returns></returns>
        [HttpPost("GenerateDailyReportData")]
        public async Task<IActionResult> GenerateDailyReportData([FromForm] string DateTimeValue)
        {
            return await Task.Run(async () =>
            {
                string path = await TestDataDaily.GenerateDailyReportData(DateTimeValue);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(path);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }

        /// <summary>
        /// 根据传入日期生成日LG测试数据和AE数据
        /// </summary>
        /// <param name="DateTimeValue">传入生成的日期</param>
        /// <returns></returns>
        [HttpPost("GenerateLGDailyData")]
        public async Task<IActionResult> GenerateLGDailyData([FromForm] string DateTimeValue)
        {
            return await Task.Run(async () =>
            {
                string path = await LGDailyData.GenerateLGDailyData(DateTimeValue);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(path);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }
    }
}
