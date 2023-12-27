using Data_Query_API.DataProcessing.LGTestData;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// Logi 测试数据查询接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LGTestDataController : ControllerBase
    {
        /// <summary>
        /// 获取LG测试数据数据库所有表名
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLGDataTableName")]
        public async Task<ApiResponse> GetLGDataTableName()
        {
            return await LGTestData.GetLGDataTableName();
        }

        /// <summary>
        /// 根据表名获取LG测试数据表中所有站别
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <returns></returns>
        [HttpPost("GetDataTableLGStation")]
        public async Task<ApiResponse> GetDataTableLGStation([FromForm] string TableName)
        {
            return await LGTestData.GetDataTableLGStation(TableName);
        }

        /// <summary>
        /// 根据表名获取LG测试数据表中指定站别所有测试电脑名称
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="Station">传入站别名称</param>
        /// <returns></returns>
        [HttpPost("GetLGComputerName")]
        public async Task<ApiResponse> GetLGComputerName([FromForm] string TableName, [FromForm] string Station)
        {
            return await LGTestData.GetLGComputerName(TableName, Station);

        }

        /// <summary>
        /// 获取指定条件LG测试数据
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="Station">站别名称</param>
        /// <param name="MachineName">电脑编号</param>
        /// <param name="MesStatus">MES 转态</param>
        /// <param name="StartTime">查询开始时间</param>
        /// <param name="EndTime">查询结束时间</param>
        /// <returns></returns>
        [HttpPost("GetLGTestData")]
        public async Task<ApiResponse> GetLGTestData([FromForm] string TableName, [FromForm] string Station, [FromForm] string MachineName,
            [FromForm] string MesStatus, [FromForm] string StartTime, [FromForm] string EndTime)
        {
            return await Task.Run(async () =>
            {
                return await LGTestData.GetLGTestData(TableName, Station, MachineName, MesStatus, StartTime, EndTime);
            });
        }

        /// <summary>
        /// 根据指定条件获取生成 Json File 文件原始数据 
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="Station">站别名称</param>
        /// <param name="MachineName">电脑编号</param>
        /// <param name="MesStatus">MES 转态</param>
        /// <param name="StartTime">查询开始时间</param>
        /// <param name="EndTime">查询结束时间</param>
        /// <returns></returns>
        [HttpPost("GetJsonFileData")]
        public async Task<ApiResponse> GetJsonFileData([FromForm] string TableName, [FromForm] string Station, [FromForm] string MachineName,
            [FromForm] string MesStatus, [FromForm] string StartTime, [FromForm] string EndTime)
        {
            return await Task.Run(async () =>
            {
                return await LGTestData.GetJsonFileData(TableName, Station, MachineName, MesStatus, StartTime, EndTime);
            });
        }

        /// <summary>
        /// 保存页面Logi测试数据
        /// </summary>
        /// <param name="saveLGDataValue">传入保存的LG数据</param>
        /// <returns></returns>
        /// //dynamic DataQuery
        [HttpPost("SaveLGTestData")]
        public async Task<IActionResult> SaveLGTestData(SaveLGDataValue saveLGDataValue)
        {
            return await Task.Run(() =>
            {
                string Path = LGTestData.SaveLGTestData(saveLGDataValue);
                FileInfo fileInfo = new(Path);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(Path), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }
    }
}
