using Data_Query_API.DataProcessing.DataQuery;
using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.DataProcessing.TestData;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// 测试数据查询接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestDataQueryController : ControllerBase
    {
        /// <summary>
        /// 获取测试数据数据库所有表名
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetDataTableName")]
        public async Task<ApiResponse> GetDataTableName()
        {
            return await TestData.GetDataTableName();
        }

        /// <summary>
        /// 根据表名获取表中所有站别
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <returns></returns>
        [HttpPost("GetDataTableStation")]
        public async Task<ApiResponse> GetDataTableStation([FromForm] string TableName)
        {

                return await TestData.GetDataTableStation(TableName);
        }

        /// <summary>
        /// 根据表名获取表中指定站别所有测试电脑编号
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <param name="Station">传入站别名称</param>
        [HttpPost("GetComputerName")]
        public async Task<ApiResponse> GetComputerName([FromForm] string TableName, [FromForm] string Station)
        {

                return await TestData.GetComputerName(TableName, Station);
        }

        /// <summary>
        /// 根据表名获取表中指定站别所有工单信息
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <param name="Station">传入站别名称</param>
        /// <returns></returns>
        [HttpPost("GetDataTableWorkOrder")]
        public async Task<ApiResponse> GetDataTableWorkOrder([FromForm] string TableName, [FromForm] string Station)
        {

                return await TestData.GetDataTableWorkOrder(TableName, Station);
        }

        /// <summary>
        /// 获取指定条件测试数据
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="Station">站别名称</param>
        /// <param name="MachineName">电脑编号</param>
        /// <param name="Result">测试结果</param>
        /// <param name="DuplicateRemoval">是否去重(默认传False)</param>
        /// <param name="Workorders">工单号</param>
        /// <param name="SN">SN 号</param>
        /// <param name="IncludeValue">测试数据包含内容</param>
        /// <param name="StartTime">查询开始时间</param>
        /// <param name="EndTime">查询结束时间</param>
        /// <returns></returns>
        [HttpPost("GetTestData")]
        public async Task<ApiResponse> GetTestData([FromForm] string TableName, [FromForm] string Station, [FromForm] string MachineName,
            [FromForm] string Result, [FromForm] string DuplicateRemoval, [FromForm] string Workorders, [FromForm] string SN,
            [FromForm] string IncludeValue, [FromForm] string StartTime, [FromForm] string EndTime)
        {
            return await Task.Run(async () =>
            {
                return await TestData.GetTestData(TableName, Station, MachineName, Result, DuplicateRemoval, Workorders, SN, IncludeValue, StartTime, EndTime);
            });
        }

        /// <summary>
        /// 自定义查询测试数据
        /// </summary>
        /// <param name="Sql">传入自定义查询语句</param>
        /// <returns></returns>
        [HttpPost("CustomQuery")]
        public async Task<ApiResponse> CustomQuery([FromForm] string Sql)
        {
            return await Task.Run(async () =>
            {
                return await TestData.CustomQuery(Sql);
            });
        }

        /// <summary>
        /// 保存页面测试数据
        /// </summary>
        /// <param name="saveTestData">传入对应数据</param>
        /// <returns></returns>
        [HttpPost("SaveTestData")]
        public async Task<IActionResult> SaveTestData(SaveTestDataValue saveTestData)
        {
            return await Task.Run(() =>
            {
                 string path = TestData.SaveData(saveTestData);
                 var provider = new FileExtensionContentTypeProvider();
                 FileInfo fileInfo = new(path);
                 var ext = fileInfo.Extension;
                 new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                 return File(System.IO.File.ReadAllBytes(path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }

        /// <summary>
        /// 保存页面模糊查询测试数据
        /// </summary>
        /// <param name="saveTestData">传入保存的测试数据</param>
        /// <returns></returns>
        [HttpPost("SaveFuzzyQueryTestData")]
        public async Task<IActionResult> SaveFuzzyQueryTestData(SaveTestDataValue saveTestData)
        {
            return await Task.Run(() =>
            {
                string path = TestData.SaveFuzzyQueryTestData(saveTestData);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(path);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }

        /// <summary>
        /// 创建所有log表数据分析触发器
        /// </summary>
        /// <returns></returns>
        [HttpPost("GenerateTestDdataTrigger")]
        public async Task<ApiResponse> GenerateTestDdataTrigger()
        {
            return await DataTableCreation.GenerateTestDdataTrigger();
        }

        /// <summary>
        /// 删除所有log表数据分析触发器
        /// </summary>
        /// <returns></returns>
        [HttpPost("DeleteTestDdataTrigger")]
        public async Task<ApiResponse> DeleteTestDdataTrigger()
        {
            return await DataTableCreation.DeleteTestDdataTrigger();
        }
    }
}
