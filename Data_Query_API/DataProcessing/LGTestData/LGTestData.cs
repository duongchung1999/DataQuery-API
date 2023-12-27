using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.SaveData;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Data_Query_API.DataProcessing.LGTestData
{
    /// <summary>
    /// LG 测试数据查询类
    /// </summary>
    public class LGTestData
    {
        #region 获取LG测试数据数据库所有表名
        /// <summary>
        /// 获取LG测试数据数据库所有表名
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetLGDataTableName()
        {
            try
            {
                Mysql context = new(GetConfiguration.LGTestLogMysql);
                ApiResponse result = await context.DataQuery("select table_name from information_schema.tables where table_schema='LogiTestLog' ORDER BY table_name;", 3000);
                if (result.status != 200) return result;
                List<Dictionary<string, object>> data = new();
                foreach (Dictionary<string, object> item in (List<Dictionary<string, object>>)result.data)
                {
                    data.Add(item.ToDictionary(k => k.Key.ToLower(), k => k.Value));
                }
                return ApiResponse.OK(data);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取测试数据数据库所有表名(GetLGDataTableName)异常,异常信息{ex.Message}", "LGTestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据表名获取LG测试数据表中所有站别
        /// <summary>
        /// 根据表名获取LG测试数据表中所有站别
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDataTableLGStation(string TableName)
        {
            try
            {
                Mysql context = new(GetConfiguration.LGTestLogMysql);
                return await context.DataQuery($"SELECT DISTINCT Station FROM {TableName} ORDER BY Station;", 3000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取指定LG测试数据表中所有站别(GetDataTableLGStation)异常,异常信息{ex.Message}", "LGTestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据表名获取LG测试数据表中指定站别所有测试电脑名称
        /// <summary>
        /// 根据表名获取LG测试数据表中指定站别所有测试电脑名称
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="Station">传入站别名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetLGComputerName(string TableName, string Station)
        {
            try
            {
                Mysql context = new(GetConfiguration.LGTestLogMysql);
                return await context.DataQuery($"SELECT DISTINCT MachineName FROM {TableName} where Station = '{Station}' ORDER BY MachineName;", 3000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取指定LG测试数据表中指定站别所有测试电脑名称(GetLGComputerName)异常,异常信息{ex.Message}", "LGTestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 获取指定条件LG测试数据
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
        public static async Task<ApiResponse> GetLGTestData(string TableName, string Station, string MachineName, string MesStatus, string StartTime, string EndTime)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string QuerySql = $"select * from {TableName} WHERE Station = '{Station}'";
                    QuerySql += MachineName.Equals("null") ? "" : $" AND MachineName = '{MachineName}'";
                    QuerySql += MesStatus.Equals("True") ? " AND MesFlag > '0'" : " AND MesFlag = '0'";
                    QuerySql += StartTime.Equals("null") ? "" :
                        $" AND Test_Start_Time BETWEEN '{Time_conversion.ToUnixTimestamp(Convert.ToDateTime(StartTime))}' AND" +
                        $" '{Time_conversion.ToUnixTimestamp(Convert.ToDateTime(EndTime))}'";
                    QuerySql += " ORDER BY Test_Start_Time ASC";
                    Mysql context = new(GetConfiguration.LGTestLogMysql);
                    return await context.DataQuery(QuerySql, 100000);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"获取指定条件LG测试数据(GetLGTestData)异常,异常信息{ex.Message}", "LGTestDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
            });
        }
        #endregion

        #region 根据指定条件获取生成 Json File 文件原始数据 
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
        public static async Task<ApiResponse> GetJsonFileData(string TableName, string Station, string MachineName, string MesStatus, string StartTime, string EndTime)
        {
            try
            {
                string QuerySql = $"select * from {TableName} WHERE Station = '{Station}'";
                QuerySql += MachineName.Equals("null") ? "" : $" AND MachineName = '{MachineName}'";
                QuerySql += MesStatus.Equals("True") ? " AND MesFlag > '0'" : " AND MesFlag = '0'";
                QuerySql += StartTime.Equals("null") ? "" :
                    $" AND Test_Start_Time BETWEEN '{Time_conversion.ToUnixTimestamp(Convert.ToDateTime(StartTime))}' AND" +
                    $" '{Time_conversion.ToUnixTimestamp(Convert.ToDateTime(EndTime))}'";
                QuerySql += " ORDER BY Test_Start_Time DESC  limit 1";
                Mysql context = new(GetConfiguration.LGTestLogMysql);
                return await context.DataQuery(QuerySql, 2000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据指定条件获取生成 Json File 文件原始数据(GetJsonFileData)异常,异常信息{ex.Message}", "LGTestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 保存页面LG数据
        /// <summary>
        /// 保存页面LG数据
        /// </summary>
        /// <param name="SaveLGTestData">传入页面LG数据</param>
        /// <returns></returns>
        public static string SaveLGTestData(SaveLGDataValue SaveLGTestData)
        {
            try
            {
                string path = @$"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData\";//保存路径
                string ZipPath = "";
                // 获取存储日期的差值
                TimeSpan ts = DateTime.Parse(SaveLGTestData.EndTime) - DateTime.Parse(SaveLGTestData.StartTime);
                if (ts.Days == 0)
                {
                    var TestComputerStatistics = SaveLGTestData.TestData.GroupBy(x => x["MachineName"].ToString()).ToList();
                    string FileName = "";
                    bool flag = true;
                    List<Dictionary<string, object>> DataValue = new();
                    foreach (var item in TestComputerStatistics)
                    {
                        List<Dictionary<string, object>> Specify_computer_test_data = SaveLGTestData.TestData.Where(y => y["MachineName"].ToString().Equals(item.Key.ToString())).ToList();
                        //JToken[] Specify_computer_test_data = TestDataValue.Where(y => y["MachineName"].ToString().Equals(item.Key.ToString())).ToArray();
                        // 生成测试数据固定区域
                        string LG_data_title = LGDataTitle(Specify_computer_test_data, false, out string CSVName);
                        // 生成测试数据
                        string Data_Info_Value = LGDataInfo(Specify_computer_test_data, false, CSVName, out DataValue);
                        // 生成文件夹名称
                        FileName = $@"{SaveLGTestData.Model}_{Specify_computer_test_data[0]["Station"]}__LogiData";
                        // 保存CSV测试数据
                        SaveCsv.WritePageCsv(path, FileName, "", CSVName, LG_data_title + Data_Info_Value, flag);
                        // 保存Excel测试数据
                        if (!SaveExcel.SaveLGData($@"{path}{FileName}", CSVName.Replace("csv", "xlsx"), "", LG_Excel_Fixed_Data, DataValue)) return null;
                        flag = false;
                    }
                    ZipPath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData_Zip\{FileName}.zip";
                    // 保存测试数据统计
                    Dictionary<string, int> FailStatistics = new();
                    foreach (var item in SaveLGTestData.TestFailStatistics)
                    {
                        FailStatistics.Add(item["name"].ToString(), Convert.ToInt32(item["value"].ToString()));
                    }
                    Dictionary<string, Dictionary<string, string>> computer_data_statistics = SaveExcel.Test_computer_data_statistics
                    (SaveLGTestData.TestData, "Status", "UID", out string ComputerStatisticalResults);
                    // Excel存储路径
                    string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData\{FileName}\测试数据统计.xlsx";                // 保存统计数据
                    if (!SaveExcel.SaveDataAnalysis(Path, "测试数据统计分析页",
                      SaveLGTestData.TestDataStatistics[0], null,
                       computer_data_statistics, FailStatistics, false)) return null;

                    // 转换为压缩包
                    if (!Directory.Exists(@"{GetConfiguration.RootDirectory}\DataQuery\LogiData\LGPageData_Zip")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData_Zip");
                    Zip.CompressDirectory($"{path}{FileName}", ZipPath, 9, true);
                }
                else
                {
                    string dateTime = Convert.ToDateTime(SaveLGTestData.StartTime).ToString("yyyy-MM-dd");
                    var TestComputerStatistics = SaveLGTestData.TestData.GroupBy(x => x["MachineName"].ToString()).ToList();
                    string FileName = "";
                    string FileTime = "";
                    bool flag = true;
                    DateTime start_time = DateTime.Now;
                    DateTime end_time = DateTime.Now;
                    for (int i = 0; i < ts.Days; i++)
                    {
                        foreach (var item in TestComputerStatistics)
                        {
                            start_time = Convert.ToDateTime(dateTime + " 00:00:00").AddDays(i); // 查询数据的开始时间
                            end_time = Convert.ToDateTime(dateTime + " 23:59:59").AddDays(i); // 查询数据的结束时间
                            List<Dictionary<string, object>> SpecifyComputerTestData = SaveLGTestData.TestData.Where(y => y["MachineName"].ToString().Equals(item.Key.ToString()) &&
                            Convert.ToDateTime(y["Test_Start_Time"].ToString()) > start_time && Convert.ToDateTime(y["Test_Start_Time"].ToString()) < end_time).ToList();
                            if (SpecifyComputerTestData.Count == 0) continue;
                            string LG_data_title = LGDataTitle(SpecifyComputerTestData, false, out string CSVName);
                            string Data_Info_Value = LGDataInfo(SpecifyComputerTestData, false, CSVName, out List<Dictionary<string, object>> DataValue);
                            FileName = $"{SaveLGTestData.Model}_{SpecifyComputerTestData[0]["Station"]}__LogiData";
                            FileTime = start_time.ToString("yyyy-MM-dd");
                            // 保存CSV测试数据
                            SaveCsv.WritePageCsv(path, FileName, FileTime, CSVName, LG_data_title + Data_Info_Value, flag);
                            // 保存Excel测试数据
                            if (!SaveExcel.SaveLGData($@"{path}{FileName}", CSVName.Replace("csv", "xlsx"), FileTime, LG_Excel_Fixed_Data, DataValue)) return null;
                            flag = false;
                        }

                        // Excel存储路径
                        string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData\{FileName}\{FileTime}\测试数据统计.xlsx";
                        // 测试数据统计
                        List<Dictionary<string, object>> StatisticalData = SaveLGTestData.TestData.Where(y => Convert.ToDateTime(y["Test_Start_Time"].ToString())
                        > start_time && Convert.ToDateTime(y["Test_Start_Time"].ToString()) < end_time).ToList();
                        if (StatisticalData.Count == 0) continue;
                        Dictionary<string, string> TestDataStatistics = SaveExcel.TotalDataAalysis(StatisticalData, "Status", "UID");
                        Dictionary<string, object> dictionaryOfObject = TestDataStatistics.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                        // 测试电脑数据统计
                        Dictionary<string, Dictionary<string, string>> computer_data_statistics = SaveExcel.Test_computer_data_statistics
                            (StatisticalData, "Status", "UID", out string ComputerStatisticalResults);
                        // 统计测试项失败统计
                        Dictionary<string, int> FailStatistics = SaveExcel.FailedItemStatistics(StatisticalData, "Status", "F");
                        if (!SaveExcel.SaveDataAnalysis(Path, "测试数据统计分析页", dictionaryOfObject, null,
                           computer_data_statistics, FailStatistics, false)) return null;
                    }
                    // 压缩文件夹
                    ZipPath = $@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData_Zip\{FileName}.zip";
                    if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData_Zip")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\LogiData\LGPageData_Zip");
                    Zip.CompressDirectory($"{path}{FileName}", ZipPath, 9, true);
                }
                return ZipPath;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"保存页面LG数据(SaveLGTestData)异常,异常信息{ex.Message}", "LGTestDataLog");
                return null;
            }
        }
        #endregion

        #region 生成标题和格式数据
        /// <summary>
        /// 存储固定栏位数据
        /// </summary>
        private static List<string[]> LG_Excel_Fixed_Data = new();
        /// <summary>
        /// 生成标题和格式数据
        /// </summary>
        /// <param name="Specify_computer_test_data">传入数据</param>
        /// <param name="flag">页面或者自动生成flag</param>
        /// <param name="CSVName">CSV名称</param>
        /// <returns></returns>
        public static string LGDataTitle(List<Dictionary<string, object>> Specify_computer_test_data, bool flag, out string CSVName)
        {
            try
            {
                LG_Excel_Fixed_Data.Clear();
                // 标题数据(客户要求格式，字段名称禁止更改)
                string title = "SeqNum,Status,Comment,Test_Start_Time,Test_Duration,Failed_Tests,BU,Project,Station,Stage,CSVFileName," +
                    "MAC_Addr,IP_Addr,oemSource";
                DateTime Test_Start_Time = new();
                if (flag) Test_Start_Time = Time_conversion.ToLocalDateTime(Convert.ToInt32(Specify_computer_test_data[0]["Test_Start_Time"]));
                else Test_Start_Time = Convert.ToDateTime(Specify_computer_test_data[0]["Test_Start_Time"].ToString());
                Test_Start_Time = Test_Start_Time.AddSeconds(-1);
                // 第一行数据
                string Line_1 = $",T,{Test_Start_Time:MM-dd-yyyy HH:mm:ss},,,,,,,,,,,";
                // 第二行数据
                string Line_2 = $",U,,,,,,,,,,,,";
                // 第三行数据
                string Line_3 = $",L,,,,,{Specify_computer_test_data[0]["BU"]},{Specify_computer_test_data[0]["Project"]}," +
                    $"{Specify_computer_test_data[0]["Station"]},{Specify_computer_test_data[0]["Stage"]},," +
                    $",,{Specify_computer_test_data[0]["oemSource"]}";
                // 第四行数据
                string Line_4 = $",#P,,,,,,,,,,,,";
                // 第五行数据
                DateTime DllTime = Time_conversion.ToLocalDateTime(Convert.ToInt32(Specify_computer_test_data[0]["DLLTime"].ToString()));
                string Line_5 = $",#I,,,{DllTime:MM-dd-yyyy HH:mm:ss},{Specify_computer_test_data[0]["DLLName"]},,,,,,,,";
                // 第六行数据
                string Line_6 = $",#L,,,,,,,,,,,,";
                // 第七行数据
                string Line_7 = $",#M,,,,,,,,,,,,";
                // 第八行数据
                string Line_8 = $",#E,,,,,,,,,,,,";
                var Testlog = Specify_computer_test_data[0]["Testlog"].ToString().Split(new char[2] { '#', ',' });
                int id = 1;
                for (int i = 0; i < Testlog.Length - 4; i += 5)
                {
                    string logTitle = Regex.Replace(Testlog[i], @"[\/\\\-+.,，]", ""); // 去除特殊字符
                    //string logTitle = Testlog[i];
                    title += $",{logTitle}"; // 标题数据
                    Line_1 += ",";
                    // 第二行数据
                    // 上限数据
                    Line_2 += $",{Testlog[i + 4].Trim()}";
                    // 第三行数据
                    // 下限数据
                    Line_3 += $",{Testlog[i + 3].Trim()}";
                    Line_8 += $",E{id.ToString().PadLeft(3, '0')}";
                    id++;
                }
                LG_Excel_Fixed_Data.Add(title.Split(','));
                LG_Excel_Fixed_Data.Add(Line_1.Split(','));
                LG_Excel_Fixed_Data.Add(Line_2.Split(','));
                LG_Excel_Fixed_Data.Add(Line_3.Split(','));
                LG_Excel_Fixed_Data.Add(Line_4.Split(','));
                LG_Excel_Fixed_Data.Add(Line_5.Split(','));
                LG_Excel_Fixed_Data.Add(Line_6.Split(','));
                LG_Excel_Fixed_Data.Add(Line_7.Split(','));
                LG_Excel_Fixed_Data.Add(Line_8.Split(','));
                CSVName = $"{Specify_computer_test_data[0]["Project"]}_{Specify_computer_test_data[0]["Stage"]}_{Specify_computer_test_data[0]["Station"]}__" +
                    $"{Test_Start_Time:yyMMdd}_{Specify_computer_test_data[0]["MachineName"]}_01.csv";
                return $"{title},\n{Line_1},\n{Line_2},\n{Line_3},\n{Line_4},\n{Line_5},\n{Line_6},\n{Line_7},\n{Line_8},\n";
            }
            catch (Exception ex)
            {
                Log.LogWrite($"生成标题和格式数据(LGDataTitle)异常,异常信息{ex.Message}", "LGTestDataLog");
                CSVName = "";
                return null;
            }
        }
        #endregion

        #region 生成测试数据
        /// <summary>
        /// 生成测试数据
        /// </summary>
        /// <param name="Specify_computer_test_data">传入数据</param>
        /// <param name="flag">页面或者自动生成flag</param>
        /// <param name="FileName">文件名称</param>
        /// <param name="DataValue">返回Excel数据</param>
        /// <returns></returns>
        public static string LGDataInfo(List<Dictionary<string, object>> Specify_computer_test_data, bool flag, string FileName, out List<Dictionary<string, object>> DataValue)
        {
            string Data_Info = "";
            DataValue = new();
            try
            {
                int id = 1;
                foreach (var item in Specify_computer_test_data)
                {

                    Dictionary<string, object> info = new();
                    if (flag) item["Test_Start_Time"] = Time_conversion.ToLocalDateTime(Convert.ToInt32(item["Test_Start_Time"])).ToString();

                    info.Add("SeqNum", id);
                    Data_Info += $"{id},";
                    for (int i = 1; i < LG_Excel_Fixed_Data[0].Length; i++)
                    {
                        try
                        {
                            if (i == 10)
                            {
                                Data_Info += $"{FileName},";
                                info.Add(LG_Excel_Fixed_Data[0][i], FileName);
                                continue;
                            }
                            string Key = LG_Excel_Fixed_Data[0][i];
                            if (!flag)
                                if (Key.Contains('.')) Key = Key.Replace(".", "");
                            string result = item[Key].ToString();
                            if (result.Equals("True")) result = "Pass";
                            if (LG_Excel_Fixed_Data[0][i].Equals("Test_Start_Time"))
                            {
                                string time = Convert.ToDateTime(result).ToString("HH:mm:ss");
                                Data_Info += $"{time},";
                                info.Add(LG_Excel_Fixed_Data[0][i], time);
                                continue;
                            }
                            Data_Info += $"{result},";
                            info.Add(LG_Excel_Fixed_Data[0][i], result);
                        }
                        catch (Exception ex)
                        {
                            Log.LogWrite(ex.Message.ToString(), "LGTestDataLog");
                            Data_Info += $",";
                            info.Add(LG_Excel_Fixed_Data[0][i], "");
                            continue;
                        }
                    }
                    Data_Info += "\n";
                    DataValue.Add(info);
                    id++;
                }
            }
            catch(Exception ex)
            {
                Log.LogWrite($"生成测试数据(LGDataInfo)异常,异常信息{ex.Message}", "LGTestDataLog");
            }
            return Data_Info;
        }
        #endregion
    }
}
