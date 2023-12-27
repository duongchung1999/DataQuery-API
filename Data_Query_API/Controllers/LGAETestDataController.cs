using Data_Query_API.DataProcessing.LGTestData;
using Data_Query_API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Data_Query_API.Controllers
{
    /// <summary>
    /// Logi 声学数据查询和存储接口
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LGAETestDataController : ControllerBase
    {
        /// <summary>
        /// 获取上传LG声学数据的所有机型
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLGAEDataModel")]
        public async Task<ApiResponse> GetLGAEDataModel()
        {
            return await LGAETestData.GetLGAEDataModel();
        }

        /// <summary>
        /// 根据机型获取上传LG声学数据的站别
        /// </summary>
        /// <returns></returns>
        [HttpPost("GetLGAEDataStationFromModel")]
        public async Task<ApiResponse> GetLGAEDataStationFromModel([FromForm] string Model)
        {
            return await LGAETestData.GetLGAEDataStationFromModel(Model);
        }

        /// <summary>
        /// 根据站别和时间获取上传的LG声学数据文件名称
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="LGStation">传入LG站别名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        [HttpPost("GetLGAEDataName")]
        public async Task<ApiResponse> GetLGAEDataName([FromForm] string Model,[FromForm] string LGStation, [FromForm] string StartTime, [FromForm] string EndTime)
        {
            return await LGAETestData.GetLGAEDataName(Model,LGStation, StartTime, EndTime);
        }

        /// <summary>
        /// 根据页面选择下载单个声学数据文件
        /// </summary>
        /// <param name="FilePath">传入文件路径</param>
        /// <returns></returns>
        [HttpPost("DownloadSingleAcousticData")]
        public async Task<IActionResult> DownloadSingleAcousticData([FromForm] string FilePath)
        {
            return await Task.Run(() =>
            {
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(FilePath);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(FilePath), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }

        /// <summary>
        /// 下载页面选择的声学数据
        /// </summary>
        /// <param name="FileName">传入存储后的文件夹名称</param>
        /// <param name="FileInfo">以(JSON.stringify)入选择的文件信息</param>
        /// <returns></returns>
        [HttpPost("DownloadSelectedAcousticData")]
        public async Task<IActionResult> DownloadSelectedAcousticData([FromForm] string FileName,[FromForm] string FileInfo)
        {
            return await Task.Run(async () =>
            {
                string FilePath = await LGAETestData.DownloadSelectedAcousticData(FileName, FileInfo);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(FilePath);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(FilePath), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }

        /// <summary>
        /// 下载页面指定条件下的所有声学数据
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="LGStation">传入LG站别名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <param name="FileName">传入存储后的文件夹名称</param>
        /// <returns></returns>
        [HttpPost("DownloadAcousticData")]
        public async Task<IActionResult> DownloadAcousticData([FromForm] string Model, [FromForm] string LGStation, [FromForm] string StartTime, [FromForm] string EndTime,[FromForm] string FileName)
        {
            return await Task.Run(async () =>
            {
                string FilePath = await LGAETestData.DownloadAcousticData(Model,LGStation, StartTime, EndTime,FileName);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(FilePath);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(FilePath), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }

        /// <summary>
        /// 根据数据生成上传源文件
        /// </summary>
        /// <param name="StationConfig">传入站别Config信息</param>
        /// <returns></returns>
        [HttpPost("DownloadAndUploadFiles")]
        public async Task<IActionResult> DownloadAndUploadFiles([FromForm] string StationConfig)
        {
            return await Task.Run(async () =>
            {
                string FilePath = await LGAETestData.DownloadAndUploadFiles(StationConfig);
                var provider = new FileExtensionContentTypeProvider();
                FileInfo fileInfo = new(FilePath);
                var ext = fileInfo.Extension;
                new FileExtensionContentTypeProvider().Mappings.TryGetValue(ext, out var contenttype);
                return File(System.IO.File.ReadAllBytes(FilePath), contenttype ?? "application/octet-stream", fileInfo.Name);
            });
        }
    }
}
