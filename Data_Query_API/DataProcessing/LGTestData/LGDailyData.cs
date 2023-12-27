using Data_Query_API.DataProcessing.BackgroundData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.SaveData;
using System.Text.RegularExpressions;

namespace Data_Query_API.DataProcessing.LGTestData
{
    /// <summary>
    /// 生成单日所有LG数据
    /// </summary>
    public class LGDailyData
    {
        /// <summary>
        /// 根据传入日期生成日LG测试数据和AE数据
        /// </summary>
        /// <param name="DateTime">传入生成的日期</param>
        /// <returns></returns>
        public static async Task<string> GenerateLGDailyData(string DateTime)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    // 创建数据库对象
                    Mysql context = new(GetConfiguration.LGTestLogMysql);
                    string path = @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData\";//保存路径
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                    int StartTime = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(DateTime + " 00:00:00")); // 查询数据的开始时间
                    int EndTime = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(DateTime + " 23:59:59")); // 查询数据的结束时间
                    string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTime.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
                                                                                                                                     // 获取后台有打开LG测试数据上传的机型和站别
                    ApiResponse UploadLGDataModelStation = await Background.GetUploadLGDataModelStation();
                    if (UploadLGDataModelStation.status != 200) return null;
                    Dictionary<int, string> UploadLGDataModelStationInfo = ((Dictionary<int, string>)UploadLGDataModelStation.data).OrderBy(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                    string Year = DateTime.Split('-')[0][^2..];
                    // 遍历获取的机型和站别
                    foreach (var value in UploadLGDataModelStationInfo)
                    {
                        string[] model_Station = value.Value.Split('|');
                        // 生成机型测试数据表名
                        string DataTableName = $"{model_Station[0].Replace('-', '_')}_{Year}_{Quarter}";
                        // 获取测试数据
                        string GetTestDataSql = $"SELECT * from {DataTableName} where Station = '{model_Station[1]}' " +
                                    $"AND Test_Start_Time BETWEEN {StartTime} AND {EndTime} ORDER BY Test_Start_Time ASC;";
                        ApiResponse LGTestDataInfo = await context.DataQuery(GetTestDataSql, 10000);
                        if (LGTestDataInfo.status != 200 || ((List<Dictionary<string, object>>)LGTestDataInfo.data).Count == 0) continue;
                        List<Dictionary<string, object>> LGDailyTestData = (List<Dictionary<string, object>>)LGTestDataInfo.data;

                        // 分割测试数据添加至字典
                        for (int i = 0; i < LGDailyTestData.Count; i++)
                        {
                            Dictionary<string, object> Data = new();
                            string[] TestLogValue = LGDailyTestData[i]["Testlog"].ToString().Split(new char[2] { '#', ',' });
                            for (int j = 0; j < TestLogValue.Length - 1; j += 5)
                            {
                                LGDailyTestData[i].Add(Regex.Replace(TestLogValue[j], @"[\/\\\-+.,，]", ""), TestLogValue[j + 2]);
                            }
                        }
                        Dictionary<string, string> TestDataStatistics = SaveExcel.TotalDataAalysis(LGDailyTestData, "Status", "UID"); // 生成测试数据统计
                        Dictionary<string, Dictionary<string, string>> TestComputerData = SaveExcel.Test_computer_data_statistics(LGDailyTestData, "Status", "UID", out string ComputerStatisticalResults); // 生成电脑数据统计
                        Dictionary<string, int> FailStatistics = SaveExcel.FailedItemStatistics(LGDailyTestData, "Status", "F"); // 生成测试项失败统计
                        // 获取该站所有测试电脑名称
                        var TestComputerStatistics = LGDailyTestData.GroupBy(x => x["MachineName"]);
                        foreach (var item in TestComputerStatistics)
                        {
                            path = @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData\{DateTime}\{model_Station[0]}\{model_Station[2]}";
                            // 获取对应测试电脑的测试数据
                            List<Dictionary<string, object>> SpecifyComputerTestData = LGDailyTestData.Where(y => y["MachineName"].ToString().Equals(item.Key.ToString())).ToList();
                            string LGDataTitle = LGTestData.LGDataTitle(SpecifyComputerTestData, true, out string CSVName);
                            string LGDataInfoValue = LGTestData.LGDataInfo(SpecifyComputerTestData, true, CSVName, out List<Dictionary<string, object>> DataValue);
                            string[] FixedData = LGDataTitle.Split('\n');
                            List<string[]> LGExcelFixedData = new()
                    {
                        FixedData[0][..^1].Split(','),
                        FixedData[1][..^1].Split(','),
                        FixedData[2][..^1].Split(','),
                        FixedData[3][..^1].Split(','),
                        FixedData[4][..^1].Split(','),
                        FixedData[5][..^1].Split(','),
                        FixedData[6][..^1].Split(','),
                        FixedData[7][..^1].Split(','),
                        FixedData[8].Split(',')
                    };
                            SaveCsv.WriteCsv(path, CSVName, LGDataTitle + LGDataInfoValue); // 保存CSV测试数据
                            //SaveExcel.SaveLGData($@"{path}", CSVName.Replace("csv", "xlsx"), "", LGExcelFixedData, DataValue); // 保存 Excel 数据
                        }
                        // 复制生成的CSV文件至指定目录
                        SaveCsv.CopyFolder($@"{path}\CSV", @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData\{DateTime}\CSV");
                        // 存储统计数据
                        Dictionary<string, object> dictionaryOfObject = TestDataStatistics.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                        SaveExcel.SaveDataAnalysis(path + $"\\{model_Station[0]}_{model_Station[1]}_测试数据统计.xlsx", "测试数据统计", dictionaryOfObject, null, TestComputerData, FailStatistics, false);
                    }
                    path = @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData\{DateTime}";
                    // 检查AE数据是否存在
                    string AEPath = @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGAcousticData\{DateTime}";
                    if (Directory.Exists(AEPath))
                    {
                        if (Directory.GetDirectories(AEPath).Length != 0 || Directory.GetFiles(AEPath).Length != 0)
                        {
                            string[] DirectoriesInfo = Directory.GetDirectories(AEPath);
                            foreach (var item in DirectoriesInfo)
                            {
                                string[] FileInfo = Directory.GetFiles(item, "*", SearchOption.AllDirectories);
                                string[] FileName = item.Split('\\');
                                string destFilePath = $@"{path}\AEData\{FileName[FileName.Length - 1]}";
                                if (!Directory.Exists(destFilePath)) Directory.CreateDirectory(destFilePath);
                                foreach (string AEFilePath in FileInfo)
                                {
                                    File.Copy(AEFilePath, $@"{destFilePath}\{Path.GetFileName(AEFilePath)}", true);
                                }
                            }
                        }
                    }
                    // 判断文件夹是否为空
                    if (Directory.GetDirectories(path).Length == 0 && Directory.GetFiles(path).Length == 0)
                    {
                        Log.LogWrite($"{DateTime} 无 LG 测试数据", "LGDailyLog");
                        return null;
                    }
                    // 压缩文件夹
                    string ZipPath = @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData\{DateTime} Logi测试数据.zip";
                    if (!Directory.Exists(@$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData"))
                        Directory.CreateDirectory(@$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGDailyData");
                    Zip.CompressDirectory(path, ZipPath, 9, true);
                    return ZipPath;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"生成日 LG 测试数据异常，异常信息：{ex.Message}", "LGDailyLog");
                    return null;
                }
            });
        }
    }
}