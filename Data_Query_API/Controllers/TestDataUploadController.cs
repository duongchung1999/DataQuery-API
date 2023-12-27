using Data_Query_API.DataProcessing.DataQuery;
using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.DataProcessing.LGTestData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// 测试数据上传接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestDataUploadController : ControllerBase
    {
        /// <summary>
        /// 判断log表是否存在，不存在则创建
        /// </summary>
        /// <param name="ModelValue">传入机型名称</param>
        /// <returns></returns>
        [HttpPost("CreateTestLogTable")]
        public async Task<ApiResponse> CreateTestLogTable([FromForm] string ModelValue)
        {
            return await DataTableCreation.CreateTestLogTable(ModelValue);
        }

        /// <summary>
        /// 上传测试数据
        /// </summary>
        /// <param name="dataValue">传入对应参数(ModelValue=机型名称,SN=SN,Result=测试结果,Station=站别名称,
        /// workorders=工单,MachineName=电脑编号,TestTime=测试时长,WireNumber=线材使用次数,TestLog=测试数据)</param>
        /// <returns></returns>
        [HttpPost("UploadTestData")]
        public async Task<ApiResponse> UploadTestData(TsetDataUploadValue dataValue)
        {
            return await TestDataUpload.UploadTestData(dataValue);
        }

        /// <summary>
        /// 获取服务器当前时间
        /// </summary>
        /// <returns></returns>
        [HttpPost("Get_date_time")]
        public async Task<string> Get_date_time()
        {
            return await Task.Run(() =>
            {
                return TestDataUpload.Get_date_time();
            });
        }

        /// <summary>
        /// 判断Logi log表是否存在，不存在则创建
        /// </summary>
        /// <param name="ModelValue">传入机型名</param>
        /// <returns></returns>
        [HttpPost("CreateLogiTestLogTable")]
        public async Task<ApiResponse> CreateLogiTestLogTable([FromForm] string ModelValue)
        {
            return await DataTableCreation.CreateLogiTestLogTable(ModelValue);
        }

        /// <summary>
        /// 上传Logi测试数据
        /// </summary>
        /// <param name="logiDataUploadValue">传入对应参数(Status=测试结果,Comment=失败项,Test_Duration=测试时长,Failed_Tests,BU,Project,Station=站别
        /// ,Stage=生产状态,MAC_Addr,IP_Addr,OemSource,DLLName=Dll名称,DLLTime=DLL生成时间,MesName=MES名称,workorders=工单,MachineName=电脑编号,
        /// TestLog=测试数据)</param>
        /// <returns></returns>
        [HttpPost("LogiTestDataUpload")]
        public async Task<ApiResponse> LogiTestDataUpload(LGDataUploadValue logiDataUploadValue)
        {
            return await LGTestDataUpload.LogiTestDataUpload(logiDataUploadValue);
        }

        /// <summary>
        /// 创建Logi声学数据上传机型站别表
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateLogiAcousticData")]
        public async Task<ApiResponse> CreateLogiAcousticData()
        {
            return await DataTableCreation.CreateLogiAcousticData();
        }

        /// <summary>
        /// 上传LG测试声学数据
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <param name="LGInfo">传入LG文件名称前缀信息（LogiProject_LogiStage）</param>
        /// <param name="MachineName">传入电脑编号</param>
        /// <param name="LGAcousticData">传入文件信息</param>
        /// <returns></returns>
        [HttpPost("UploadLGAcousticData")]
        public async Task<ApiResponse> UploadLGAcousticData([FromForm] string Model, [FromForm] string StationName, [FromForm] string LGInfo, [FromForm] string MachineName, IFormFile LGAcousticData)
        {
            return await LGTestDataUpload.UploadLGAcousticData(Model, StationName.ToUpper(), LGInfo, MachineName, LGAcousticData);
        }
    }
}
