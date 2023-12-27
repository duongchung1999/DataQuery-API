using Data_Query_API.DataProcessing.BackgroundData;
using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.SaveData;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Data_Query_API.DataProcessing.TestData
{
    /// <summary>
    /// 生成测试日报
    /// </summary>
    public class TestDataDaily
    {
        #region 根据传入日期生成日报表文件
        /// <summary>
        /// 根据传入日期生成日报表文件
        /// </summary>
        /// <param name="DateTimeValue">传入生成报表的日期</param>
        /// <returns></returns>
        public static async Task<string> GenerateDailyReportData(string DateTimeValue)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string Year = DateTimeValue.Split('-')[0]; // 获取当前年份
                    string TableName = $"Test_Statistics_{Year}"; // 存储测试日报表的表名
                                                                  // 创建测试日报表
                    bool DailyReportDataResult = await DataTableCreation.CreateTable(TableName);
                    if (!DailyReportDataResult) DailyReportDataResult = await DataTableCreation.CreateTable(TableName);
                    if (!DailyReportDataResult) return null;
                    // 创建备份测试日报表
                    DailyReportDataResult = await DataTableCreation.CreateTable_backups(TableName);
                    if (!DailyReportDataResult) DailyReportDataResult = await DataTableCreation.CreateTable_backups(TableName);
                    if (!DailyReportDataResult) return null;
                    int time = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(DateTimeValue + " 07:00:00")); // 获取时间戳
                                                                                                                 // 初始化数据库
                    Mysql TestLogConn = new(GetConfiguration.testLogMysql);
                    Mysql TestDailyConn = new(GetConfiguration.testDailyMysql);
                    // 创建日报表存储文件夹
                    string path = @$"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\DataDailyReport\{DateTimeValue}";
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                    string SqlValue = "";
                    string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTimeValue.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
                                                                                                                                          // 获取后台有打开测试数据上传的机型和站别
                    ApiResponse UploadDataModelStation = await Background.GetUploadDataModelStation();
                    if (UploadDataModelStation.status != 200) return null;
                    Dictionary<int, string> UploadDataModelStationInfo = ((Dictionary<int, string>)UploadDataModelStation.data).OrderBy(s => s.Value).ToDictionary(s => s.Key, s => s.Value);
                    // 遍历获取的机型和站别
                    foreach (var value in UploadDataModelStationInfo)
                    {
                        string[] model_Station = value.Value.Split('|');
                        string DataTableName = model_Station[0].Replace('-', '_') + "_" + Year[^2..] + "_" + Quarter;
                        // 获取测试数据
                        string GetTestDataSql = $"SELECT * from {DataTableName} where " +
                            $"Station = '{model_Station[1]}' AND Time BETWEEN '{DateTimeValue} 00:00:00' AND '{DateTimeValue} 23:59:59' ORDER BY Time DESC;";
                        ApiResponse TestDataInfo = await TestLogConn.DataQuery(GetTestDataSql, 10000);
                        if (TestDataInfo.status != 200 || ((List<Dictionary<string, object>>)TestDataInfo.data).Count == 0) continue;
                        List<Dictionary<string, object>> TestData = (List<Dictionary<string, object>>)TestDataInfo.data;
                        Dictionary<string, string> TestDataStatistics = SaveExcel.TotalDataAalysis(TestData, "Result", "SN"); // 生成测试数据统计
                        Dictionary<string, Dictionary<string, string>> TestNodeData = Node_data_statistics(DateTimeValue, TestData, out string NodeData); // 生成节点数据统计
                        Dictionary<string, Dictionary<string, string>> TestComputerData = SaveExcel.Test_computer_data_statistics(TestData, "Result", "SN", out string ComputerStatisticalResults); // 生成电脑数据统计
                        Dictionary<string, int> FailStatistics = SaveExcel.FailedItemStatistics(TestData, "Result", "False"); // 生成测试项失败统计
                        StringBuilder TestFailItems = new StringBuilder();
                        foreach (KeyValuePair<string, int> Fail in FailStatistics)
                        {
                            TestFailItems.Append($"{Fail.Key}:{Fail.Value},");
                        }
                        if (TestFailItems.Length == 0) TestFailItems.Append("NULL");


                        // 生成机型Excel文件
                        string ModelPath = path + @$"\{model_Station[0]}-{DateTimeValue}-测试日报.xlsx";
                        Dictionary<string, object> TestDataStatisticsValue = TestDataStatistics.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                        bool result = SaveExcel.SaveDataAnalysis(ModelPath, model_Station[1], TestDataStatisticsValue, TestNodeData, TestComputerData, FailStatistics, true);
                        if (!result)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                if (SaveExcel.SaveDataAnalysis(ModelPath, model_Station[1], TestDataStatisticsValue, TestNodeData, TestComputerData, FailStatistics, true)) break;
                            }
                        }
                        SqlValue += "('" + $"{model_Station[0]}', '{model_Station[1]}', '{time}'," +
                                $"{Convert.ToInt32(TestDataStatistics["测试总次数"])},{Convert.ToInt32(TestDataStatistics["测试成功总次数"])}," +
                                $"{Convert.ToInt32(TestDataStatistics["测试失败总次数"])},{Convert.ToInt32(TestDataStatistics["测试产品数"])}," +
                                $"{Convert.ToInt32(TestDataStatistics["重测产品数"])},{Convert.ToInt32(TestDataStatistics["包含TE_BZP测试总次数"])}," +
                                $"{Convert.ToInt32(TestDataStatistics["制造专用条码测试次数"])},{Convert.ToDouble(TestDataStatistics["重测率(新)"].Trim('%'))}," +
                                $"{Convert.ToDouble(TestDataStatistics["直通率"].Trim('%'))},'{NodeData}','{ComputerStatisticalResults}','{TestFailItems}'),";
                    }
                    #region 上传数据到数据库
                    //registered($"{DateTime.Now} 上传了一次\n");
                    string dailySql = $"INSERT INTO {TableName} " +
                               $"(Model, Station, Time, Total_number_of_tests,Test_success_times,Number_of_test_failures,Total_number_of_products_tested,Total_number_of_retests," +
                               $"Total_times_of_standard_test," + "Total_number_of_maintenance_product_tests,Retest_rate,Test_pass_through_rate ,Node_data_statistics, Test_computer_data_statistics, " +
                               $"Test_failed_items)VALUES{SqlValue.Trim(',')}";
                    Log.LogWrite($"{DateTime.Now} 生成的MySQL为{dailySql}\n\n", "ReportMysqlData");
                    //var rs = TestDailyConn.commonExecute(dailySql);// 主表
                    #endregion

                    // 判断文件夹是否为空
                    if (Directory.GetDirectories(path).Length == 0 && Directory.GetFiles(path).Length == 0)
                    {
                        Log.LogWrite($"{DateTimeValue} 无测试数据", "DataDailyLog");
                        return null;
                    }
                    // 压缩文件夹
                    string ZipPath = @$"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\DataDailyReport\{DateTimeValue} 测试报表.zip";
                    Zip.CompressDirectory(path, ZipPath, 9, true);
                    return ZipPath;
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"生成日测试数据报表异常，异常信息：{ex.Message}", "DataDailyLog");
                    return null;
                }
            });
        }
        #endregion

        #region 节点测试数据统计
        /// <summary>
        /// 返回节点测试数据
        /// </summary>
        /// <param name="DataTime">传入时间</param>
        /// <param name="TestData">传入测试数据</param>
        /// <param name="values">返回字符串节点数据统计</param>
        /// <returns></returns>
        private static Dictionary<string, Dictionary<string, string>> Node_data_statistics(string DataTime, List<Dictionary<string, object>> TestData, out string values)
        {
            Dictionary<string, Dictionary<string, string>> TestNodeData = new() { };
            string[] node = ReadConfig("node").Split(',');
            string[] nodeStartTime = ReadConfig("nodeStartTime").Split(',');
            string[] nodeEndTime = ReadConfig("nodeEndTime").Split(',');
            values = "";
            for (int i = 0; i < node.Length; i++)
            {
                if (node[i].Equals("")) continue;
                // 得到当前节点的测试数据
                var TestStatistics = TestData.Where(x => Convert.ToDateTime(x["Time"].ToString())
                >= Convert.ToDateTime($"{DataTime} {nodeStartTime[i].PadLeft(2, '0')}:00:00")
                && Convert.ToDateTime(x["Time"].ToString()) <= Convert.ToDateTime($"{DataTime} {nodeEndTime[i].PadLeft(2, '0')}:00:00")).ToList();
                var Node = SaveExcel.TotalDataAalysis(TestStatistics, "Result", "SN");
                values += $"节点名称:{node[i]}";
                foreach (var item in Node)
                {
                    values += $",{item.Key}:{item.Value}";
                }
                values += "#";
                TestNodeData.Add(node[i], Node);
            }
            return TestNodeData;
        }
        #endregion

        #region 读取文件
        // 读取ini文件的dll
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, byte[] lpszReturnBuffer, int nSize, string lpFileName);

        // 根据传入的节点读取该节点下的所有内容
        public static Dictionary<string, string> GetKeys(string iniFile, string category)
        {
            byte[] buffer = new byte[2048];
            GetPrivateProfileSection(category, buffer, 2048, iniFile);
            String[] tmp = Encoding.UTF8.GetString(buffer).Trim('\0').Split('\0');
            Dictionary<string, string> result = new();
            foreach (String entry in tmp)
            {
                string[] v = entry.Split('=');
                result.Add(v[0], v[1]);
            }
            return result;
        }

        /// <summary>
        /// 读取内容并进行处理
        /// </summary>
        /// <param name="data">读取的节点名称</param>
        /// <returns></returns>
        public static string ReadConfig(string data)
        {
            try
            {
                string value = "";
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Config.ini"; // 获取读取文件的路径
                Dictionary<string, string> content = GetKeys(path, data); // 根据传入的节点在该文件中读取
                foreach (var item in content)
                {
                    value += item.Value + ",";
                }
                return value;
            }
            catch
            {
                return "False";
            }
        }
        #endregion

    }
}
