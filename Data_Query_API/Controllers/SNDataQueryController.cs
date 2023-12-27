using Data_Query_API.DataProcessing.SNDataQuery;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// SN 查询测试数据接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SNDataQueryController : ControllerBase
    {
        /// <summary>
        /// 获取指定SN测试数据
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="YearFalgValue">是否查整年</param>
        /// <param name="SN">SN 号</param>
        /// <returns></returns>
        [HttpPost("GetTestDataFromSN")]
        public async Task<ApiResponse> GetTestDataFromSN([FromForm] string TableName, [FromForm] string YearFalgValue, [FromForm] string SN)
        {
            return await Task.Run(async () =>
            {
                return await SNDataQuery.GetTestDataFromSN(TableName, YearFalgValue, SN);
            });
        }

        /// <summary>
        /// 保存SN查询所有测试数据
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="SNTestData">传入保存的测试数据</param>
        /// <returns></returns>
        [HttpPost("SaveSNTestData")]
        public async Task<IActionResult> SaveFuzzyQueryTestData([FromForm] string Model, [FromForm] string SNTestData)
        {
            return await Task.Run(() =>
            {
                string path = SNDataQuery.SaveSNTestData(Model, SNTestData);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(path);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }
        /// <summary>
        /// 获取指定站别、SN和测试项的测试值
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <param name="SN">传入产品SN</param>
        /// <param name="TestItemName">传入测试项目名称</param>
        /// <param name="QuarterValue">传入查询几个季度</param>
        /// <returns></returns>
        [HttpPost("GetTestItemDataFromSN")]
        public async Task<ApiResponse> GetTestItemDataFromSN([FromForm] string ModelName, [FromForm] string StationName, [FromForm] string SN, [FromForm] string TestItemName, [FromForm] int QuarterValue)
        {
            return await Task.Run(async () =>
            {
                return await SNDataQuery.GetTestItemDataFromSN(ModelName, StationName, SN, TestItemName, QuarterValue);
            });
        }
    }
}
