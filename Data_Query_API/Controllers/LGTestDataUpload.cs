using Microsoft.AspNetCore.Mvc;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.DataProcessing.LGTestData;
using Data_Query_API.DataProcessing.DataTableCreationData;

namespace Test_data_API.Controllers
{
    /// <summary>
    /// Logi 测试数据上传接口旧
    /// </summary>
    [ApiController]
    [Route("/LGdata")]
    public class LGTestDataUpload : Controller
    {
        /// <summary>
        /// 判断Logi log表是否存在，不存在则创建
        /// </summary>
        /// <param name="logiDataUploadValue">传入机型名</param>
        /// <returns></returns>
        [HttpPost("CreateLogiTestLogTable")]
        public async Task<string> CreateLogiTestLogTable(LGDataUploadValue logiDataUploadValue)
        {
            ApiResponse result = await DataTableCreation.CreateLogiTestLogTable(logiDataUploadValue.ModelValue);
            return result.status == 200 ? "200" : $"205{result.message}";
        }

        /// <summary>
        /// 上传Logi测试数据
        /// </summary>
        /// <param name="logiDataUploadValue">传入对应参数(Status=测试结果,Comment=失败项,Test_Duration=测试时长,Failed_Tests,BU,Project,Station=站别
        /// ,Stage=生产状态,MAC_Addr,IP_Addr,OemSource,DLLName=Dll名称,DLLTime=DLL生成时间,MesName=MES名称,workorders=工单,MachineName=电脑编号,
        /// TestLog=测试数据)</param>
        /// <returns></returns>
        [HttpPost("Logi_test_data_upload")]
        public async Task<string> Logi_test_data_upload(LGDataUploadValue logiDataUploadValue)
        {
            //Log.WriteTest($"进入API 电脑编号:{logidataValue.MachineName} 机型：{logidataValue.ModelValue} 站别：{logidataValue.Station}", "LGdata_upload");
            ApiResponse result = await Data_Query_API.DataProcessing.LGTestData.LGTestDataUpload.LogiTestDataUpload(logiDataUploadValue);
            return result.status == 200 ? "200" : $"205{result.message}";

        }
    }
}
