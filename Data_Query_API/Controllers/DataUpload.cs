using Data_Query_API.DataProcessing.DataQuery;
using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Test_data_API.Controllers
{
    /// <summary>
    /// 测试数据上传接口旧
    /// </summary>
    [ApiController]
    [Route("/upload")]
    public class DataUpload
    {
        /// <summary>
        /// 判断log表是否存在，不存在则创建
        /// </summary>
        /// <param name="dataValue">传入机型名(ModelValue=机型名称)</param>
        /// <returns></returns>
        [HttpPost("Create_test_log_table")]
        public async Task<string> Create_test_log_table(TsetDataUploadValue dataValue)
        {
            ApiResponse result =  await DataTableCreation.CreateTestLogTable(dataValue.ModelValue);
            return result.status == 200 ? "200" : $"205{result.message}";
        }

        /// <summary>
        /// 上传测试数据
        /// </summary>
        /// <param name="dataValue">传入对应参数(ModelValue=机型名称,SN=SN,Result=测试结果,Station=站别名称,
        /// workorders=工单,MachineName=电脑编号,TestTime=测试时长,WireNumber=线材使用次数,TestLog=测试数据)</param>
        /// <returns></returns>
        [HttpPost("Test_data_upload")]
        public async Task<string> Test_data_upload(TsetDataUploadValue dataValue)
        {
            //Log.WriteTest($"进入API 电脑编号:{dataValue.MachineName} 机型：{dataValue.ModelValue} 站别：{dataValue.Station}", "data_upload");
            ApiResponse result= await TestDataUpload.UploadTestData(dataValue);
            return result.status == 200 ? "200" : $"205{result.message}";
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
    }
}
