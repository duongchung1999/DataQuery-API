using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;

namespace Data_Query_API.DataProcessing.LGTestData
{
    /// <summary>
    /// LG 测试数据上传类
    /// </summary>
    public class LGTestDataUpload
    {
        #region 上传LG测试数据
        /// <summary>
        /// 上传LG测试数据
        /// </summary>
        /// <param name="logi_data_value">传入测试数据</param>
        /// <returns></returns>
        public static async Task<ApiResponse> LogiTestDataUpload(LGDataUploadValue logi_data_value)
        {
            return await Task.Run(async() =>
            {
                try
                {
                    Mysql context = new(GetConfiguration.LGTestLogMysql);
                    string DateTimeValue = DateTime.Now.ToString("yy-MM-dd");
                    string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTimeValue.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
                    string TableName = $"{logi_data_value.ModelValue.Replace('-', '_')}_{DateTimeValue.Split('-')[0]}_{Quarter}"; // 生成表名
                    var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = '{TableName}' and TABLE_SCHEMA = 'LogiTestLog'; ");
                    if (TestDataQuery.Rows.Count == 0)
                    {
                        ApiResponse rs = await DataTableCreation.CreateLogiTestLogTable(logi_data_value.ModelValue);
                        if (rs.status != 200) return rs;
                    }
                    int Time = Time_conversion.ToUnixTimestamp(DateTime.Now);
                    string sql = $"INSERT into {TableName}(Status,Comment,Test_Start_Time,Test_Duration,Failed_Tests,BU,Project,Station,Stage,MAC_Addr,IP_Addr," +
                        $"oemSource,DLLName,DLLTime,MesFlag,MesName,workorders,MachineName,TestLog) VALUES('{logi_data_value.Status}','{logi_data_value.Comment}'," +
                        $"'{Time}','{logi_data_value.Test_Duration}','{logi_data_value.Failed_Tests}','{logi_data_value.BU}','{logi_data_value.Project}'," +
                        $"'{logi_data_value.Station}','{logi_data_value.Stage}','{logi_data_value.MAC_Addr}','{logi_data_value.IP_Addr}'," +
                        $"'{logi_data_value.OemSource}','{logi_data_value.DLLName}','{Time_conversion.ToUnixTimestamp(Convert.ToDateTime(logi_data_value.DLLTime))}','{logi_data_value.MesFlag}','" +
                        $"{logi_data_value.MesName}','{logi_data_value.workorders}','{logi_data_value.MachineName}','{logi_data_value.TestLog}')";
                    ApiResponse result = await context.commonExecute(sql);
                    if (result.status != 200) return result;
                    return result;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"上传LG测试数据(LogiDataUpload)异常,异常信息{ex.Message}", "LGTestDataUploadLog");
                    return ApiResponse.Error(500, $"{ex.Message}");
                }
            });
        }
        #endregion

        #region 上传 LG 声学测试数据
        /// <summary>
        /// 上传 LG 声学测试数据
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <param name="LGInfo">传入LG文件名称前缀信息（LogiProject_LogiStage）</param>
        /// <param name="MachineName">传入电脑编号</param>
        /// <param name="LGAcousticData">传入文件信息</param>
        /// <returns></returns>
        public static async Task<ApiResponse> UploadLGAcousticData(string Model, string StationName,string LGInfo,string MachineName,IFormFile LGAcousticData)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (LGAcousticData != null)
                    {
                        string NowTime = DateTime.Now.ToString("yyyy-MM-dd");
                        string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticData\{NowTime}\{Model}\{StationName.Split('|')[1]}";
                        if (!Directory.Exists(Path))
                            Directory.CreateDirectory(Path);
                        // 文件名称
                        string projectFileName = $"{LGInfo}_{StationName.Split('|')[1]}__{DateTime.Now:yyMMdd}_{MachineName}_01.xlsx";
                        // 上传的文件的路径
                        string filePath = Path + $@"\{projectFileName}";
                        using (FileStream fs = File.Create(filePath))
                        {
                            LGAcousticData.CopyTo(fs);
                            fs.Flush();
                        }
                        Mysql context = new(GetConfiguration.testDailyMysql);
                        string Sql = "";
                        ApiResponse DataQuery = await context.DataQuery($"SELECT Model from Logi_Acoustic_Data WHERE Model = '{Model}' AND Station='{StationName}';", 100);
                        if (((List<Dictionary<string, object>>)DataQuery.data).Count == 0)
                        {
                            Sql = $"INSERT into Logi_Acoustic_Data (Model,Station) VALUES ('{Model}','{StationName}')";
                            ApiResponse result = await context.commonExecute(Sql);
                        }
                        return ApiResponse.OK("上传成功");
                    }
                    else
                    {
                        return ApiResponse.Error(500, "文件上传失败");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"上传 LG 声学测试数据(UploadLGAcousticData)异常,异常信息{ex.Message}", "LGTestDataUploadLog");
                    return ApiResponse.Error(500, ex.Message);
                }
            });
        }
        #endregion
    }
}
