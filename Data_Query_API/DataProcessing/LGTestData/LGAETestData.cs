using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.SaveData;
using Newtonsoft.Json;

namespace Data_Query_API.DataProcessing.LGTestData
{
    /// <summary>
    /// LG 声学数据查询
    /// </summary>
    public class LGAETestData
    {
        #region 获取上传LG声学数据的所有机型
        /// <summary>
        /// 获取上传LG声学数据的所有机型
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetLGAEDataModel()
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                return await context.DataQuery("SELECT Model from Logi_Acoustic_Data GROUP BY Model;", 3000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取上传LG声学数据的所有机型(GetLGAEDataModel)异常,异常信息{ex.Message}", "LGAETestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据机型获取上传LG声学数据的站别
        /// <summary>
        /// 根据机型获取上传LG声学数据的站别
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetLGAEDataStationFromModel(string Model)
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                return await context.DataQuery($"SELECT Station from Logi_Acoustic_Data where Model = '{Model}' ORDER BY Model;", 3000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据机型获取上传LG声学数据的站别(GetLGAEDataModel)异常,异常信息{ex.Message}", "LGAETestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据站别和时间获取上传的LG声学数据文件名称
        /// <summary>
        /// 根据站别和时间获取上传的LG声学数据文件名称
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="LGStation">传入LG站别名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetLGAEDataName(string Model,string LGStation, string StartTime, string EndTime)
        {
            return await Task.Run(() =>
            {
                try
                {
                    List<Dictionary<string, string>> AEDataNameInfo = new();
                    if (StartTime.Equals(EndTime))
                    {
                        string FilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticData\{StartTime}\{Model}\{LGStation}";
                        if (!Directory.Exists(FilePath))
                            return ApiResponse.Error(200, "暂无数据");
                        string[] FileInfo = Directory.GetFiles(FilePath);
                        foreach (string AEFilePath in FileInfo)
                        {
                            Dictionary<string, string> keyValuePairs = new();
                            keyValuePairs.Add("FileName", Path.GetFileName(AEFilePath));
                            keyValuePairs.Add("FilePath", AEFilePath);
                            AEDataNameInfo.Add(keyValuePairs);
                        }
                    }
                    else
                    {
                        TimeSpan timeDifference = Convert.ToDateTime(EndTime) - Convert.ToDateTime(StartTime);
                        for (int i = 0; i <= timeDifference.Days; i++)
                        {
                            string Time = Convert.ToDateTime(StartTime).AddDays(i).ToString("yyyy-MM-dd");
                            string FilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticData\{Time}\{Model}\{LGStation}";
                            if (!Directory.Exists(FilePath))
                                continue;
                            string[] FileInfo = Directory.GetFiles(FilePath);
                            foreach (string AEFilePath in FileInfo)
                            {
                                Dictionary<string, string> keyValuePairs = new();
                                keyValuePairs.Add("FileName", Path.GetFileName(AEFilePath));
                                keyValuePairs.Add("FilePath", AEFilePath);
                                AEDataNameInfo.Add(keyValuePairs);
                            }
                        }
                        if (AEDataNameInfo.Count == 0)
                            return ApiResponse.Error(200, "暂无数据");
                        AEDataNameInfo.Reverse();
                    }
                    return ApiResponse.OK(AEDataNameInfo);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"根据站别和时间获取上传的LG声学数据文件名称(GetLGAEDataName)异常,异常信息{ex.Message}", "LGAETestDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
            });
        }
        #endregion

        #region 下载页面选择的声学数据
        /// <summary>
        /// 下载页面选择的声学数据
        /// </summary>
        /// <param name="FileName">传入存储后的文件夹名称</param>
        /// <param name="FileInfo">以(JSON.stringify)入选择的文件信息</param>
        /// <returns></returns>
        public static async Task<string> DownloadSelectedAcousticData(string FileName, string FileInfo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string FilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload\{FileName.Replace(".zip", "")}";
                    // 删除原有同名压缩包
                    if (File.Exists($"{FilePath}.zip"))
                    {
                        File.Delete($"{FilePath}.zip");
                    }
                    // 创建文件夹
                    Directory.CreateDirectory(FilePath);
                    List<Dictionary<string, string>> FileInfoData = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(FileInfo);
                    foreach (var item in FileInfoData)
                    {
                        File.Copy(item["FilePath"], $@"{FilePath}\{item["FileName"]}", true);
                    }
                    // 压缩文件夹
                    string ZipPath = $@"{FilePath}.zip";
                    if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload");
                    Zip.CompressDirectory($"{FilePath}", ZipPath, 9, true);
                    return ZipPath;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"下载页面选择的声学数据(DownloadSelectedAcousticData)异常,异常信息{ex.Message}", "LGAETestDataLog");
                    return null;
                }
            });
        }
        #endregion

        #region 下载页面指定条件下的所有声学数据
        /// <summary>
        /// 下载页面指定条件下的所有声学数据
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="LGStation">传入LG站别名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <param name="FileName">传入存储后的文件夹名称</param>
        /// <returns></returns>
        public static async Task<string> DownloadAcousticData(string Model,string LGStation, string StartTime, string EndTime, string FileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string FilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload\{FileName.Replace(".zip", "")}";
                    // 删除原有同名压缩包
                    if (File.Exists($"{FilePath}.zip"))
                    {
                        File.Delete($"{FilePath}.zip");
                    }
                    // 创建文件夹
                    Directory.CreateDirectory(FilePath);
                    if (StartTime.Equals(EndTime))
                    {
                        string AcousticDataFilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticData\{StartTime}\{Model}\{LGStation}";
                        if (!Directory.Exists(AcousticDataFilePath))
                            return null;
                        string[] FileInfo = Directory.GetFiles(AcousticDataFilePath);
                        foreach (string AEFilePath in FileInfo)
                        {
                            File.Copy(AEFilePath, $@"{FilePath}\{Path.GetFileName(AEFilePath)}", true);
                        }
                    }
                    else
                    {
                        TimeSpan timeDifference = Convert.ToDateTime(EndTime) - Convert.ToDateTime(StartTime);
                        for (int i = 0; i <= timeDifference.Days; i++)
                        {
                            string Time = Convert.ToDateTime(StartTime).AddDays(i).ToString("yyyy-MM-dd");
                            string AcousticDataFilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticData\{Time}\{Model}\{LGStation}";
                            if (!Directory.Exists(AcousticDataFilePath)) continue;
                            if (!Directory.Exists($@"{FilePath}\{Time}"))
                                Directory.CreateDirectory($@"{FilePath}\{Time}");
                            string[] FileInfo = Directory.GetFiles(AcousticDataFilePath);
                            foreach (string AEFilePath in FileInfo)
                            {
                                File.Copy(AEFilePath, $@"{FilePath}\{Time}\{Path.GetFileName(AEFilePath)}", true);
                            }
                        }
                    }
                    // 压缩文件夹
                    string ZipPath = $@"{FilePath}.zip";
                    if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload");
                    Zip.CompressDirectory($"{FilePath}", ZipPath, 9, true);
                    return ZipPath;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"下载页面指定条件下的所有声学数据(DownloadAcousticData)异常,异常信息{ex.Message}", "LGAETestDataLog");
                    return null;
                }
            });
        }
        #endregion

        #region 根据数据生成上传源文件
        /// <summary>
        /// 根据数据生成上传源文件
        /// </summary>
        /// <param name="StationConfig">传入站别Config信息</param>
        /// <returns></returns>
        public static async Task<string> DownloadAndUploadFiles(string StationConfig)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Dictionary<string, string> StationConfigInfo = JsonConvert.DeserializeObject < Dictionary<string, string>>(StationConfig);
                    string FilePath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload\{StationConfigInfo["LogiStation"]}";
                    // 删除原有同名压缩包
                    if (File.Exists($"{FilePath}.zip"))
                    {
                        File.Delete($"{FilePath}.zip");
                    }
                    // 创建文件夹
                    Directory.CreateDirectory(FilePath);
                    Directory.CreateDirectory($@"{FilePath}\resource");
                    StreamWriter eventFile = File.CreateText($@"{FilePath}\resource\event.json");
                    eventFile.Flush();
                    eventFile.Close();
                    var projectConfig = new
                    {
                        bu = StationConfigInfo["LogiBU"].ToUpper(),
                        project = StationConfigInfo["LogiProject"].ToUpper(),
                        station = StationConfigInfo["LogiStation"].ToUpper(),
                        stage = StationConfigInfo["LogiStage"].ToUpper(),
                        oemSource = StationConfigInfo["LogioemSource"].ToUpper()
                    };
                    // 将对象序列化为JSON字符串
                    string json = JsonConvert.SerializeObject(projectConfig, Formatting.Indented);
                    // 将JSON字符串写入文件
                    StreamWriter sr = File.CreateText($@"{FilePath}\resource\projectConfig.json");
                    sr.WriteLine(json);
                    sr.Flush();
                    sr.Close();
                    // 压缩文件夹
                    string ZipPath = $@"{FilePath}.zip";
                    if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticDataDownload");
                    Zip.CompressDirectory($"{FilePath}", ZipPath, 9, true);
                    return ZipPath;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"根据数据生成上传源文件(DownloadAndUploadFiles)异常,异常信息{ex.Message}", "LGAETestDataLog");
                    return null;
                }
            });
        }
        #endregion

    }
}
