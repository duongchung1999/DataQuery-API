using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.SaveData;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Drawing;
using System.Reflection;

namespace Data_Query_API.DataProcessing.SNDataQuery
{
    /// <summary>
    /// SN查询所有
    /// </summary>
    public class SNDataQuery
    {
        #region 获取指定SN测试数据
        /// <summary>
        /// 获取指定SN测试数据
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="YearFalgValue">是否查整年</param>
        /// <param name="SN">SN 号</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetTestDataFromSN(string TableName,string YearFalgValue, string SN)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string QuerySql = "";
                    if (YearFalgValue.Equals("False"))
                    {
                        QuerySql = $"select SN,Time,Result,Station,workorders,MachineName,TestTime,WireNumber,Testlog from {TableName} WHERE" +
                            $" SN = '{SN}' ORDER BY Station ASC";
                    }
                    else
                    {
                        string CurrentMonth = DateTime.Now.ToString("MM");
                        int Quarter = int.Parse(Math.Ceiling(Math.Round((double)(Convert.ToDouble(CurrentMonth)) / 3, 4)).ToString()); // 计算当前季度
                        for (int i = 1; i <= Quarter; i++)
                        {
                            TableName = $"{TableName[..^1]}{i}";
                            QuerySql += $"SELECT SN,Time,Result,Station,workorders,MachineName,TestTime,WireNumber,Testlog FROM {TableName} WHERE SN='{SN}' UNION ";
                        }
                        QuerySql = QuerySql.TrimEnd("UNION ".ToCharArray());
                        QuerySql += " ORDER BY Station ASC";
                    }
                    Mysql context = new(GetConfiguration.testLogMysql);
                    return await context.DataQuery(QuerySql, 100000);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"获取指定SN测试数据(GetTestDataFromSN)异常,异常信息{ex.Message}", "SNDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
            });
        }
        #endregion

        #region 保存页面SN查询测试数据
        /// <summary>
        /// 保存页面SN查询测试数据
        /// </summary>
        /// <returns></returns>
        public static string SaveFuzzyQueryTestData()
        {
            try
            {
                string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data\1.xlsx";
                if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data");
                FileInfo newFile = new(Path);
                if (newFile.Exists)
                {
                    newFile.Delete();
                    newFile = new FileInfo(Path);
                }
                // 保存测试数据
                return Path;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"保存页面SN查询测试数据(SaveFuzzyQueryTestData)异常,异常信息{ex.Message}", "SNDataLog");
                return null;
            }
        }
        #endregion

        #region 保存SN查询所有测试数据
        /// <summary>
        /// 保存SN查询所有测试数据
        /// </summary>
        /// <param name="Model">传入机型名称</param>
        /// <param name="SNTestData">传入保存的测试数据</param>
        /// <returns></returns>
        public static string SaveSNTestData(string Model, string SNTestData)
        {
            try
            {
                string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\SaveSNData\{Model}.xlsx";
                if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\SaveSNData")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\SaveSNData");
                FileInfo newFile = new(Path);
                if (newFile.Exists)
                {
                    newFile.Delete();
                    newFile = new FileInfo(Path);
                }
                var SNTestDataInfo = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(SNTestData);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using ExcelPackage package = new(newFile);
                ExcelWorksheet TestDataWork = package.Workbook.Worksheets.Add("测试数据页");
                // 定义行起点
                int StartPosition = 1;
                foreach (var item in SNTestDataInfo)
                {
                    var StationData = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(item["Testdata"].ToString());
                    List<Dictionary<string, string>> StationTitleData = StationData[0]["Title"];
                    List<Dictionary<string, string>> StationTestData = StationData[1]["StationTestdata"];
                    List<List<string>> HeaderDataValue = SaveExcel.HeaderData(StationTitleData, StationTestData);
                    List<Dictionary<string, object>> Data = StationTestData.Select(dict =>dict.ToDictionary(kv => kv.Key, kv => (object)kv.Value)).ToList();
                    Data.RemoveAt(0);
                    Data.RemoveAt(0);
                    // 保存测试数据
                    if (!SaveExcel.WriteData(item["StationName"].ToString(), StartPosition, TestDataWork, HeaderDataValue, Data, Color.GreenYellow)) return null;
                    StartPosition += StationTestData.Count + 4;
                }
                // 设置单元格宽度自适应
                TestDataWork.Cells[TestDataWork.Dimension.Address].AutoFitColumns();
                TestDataWork.Protection.SetPassword("merryTE");
                package.Save();
                return Path;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"保存SN查询所有测试数据(SaveSNTestData)异常,异常信息{ex.Message}", "SNDataLog");
                return null;
            }
        }
        #endregion

        #region 获取指定站别、SN和测试项的测试值
        /// <summary>
        /// 获取指定站别、SN和测试项的测试值
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <param name="SN">传入产品SN</param>
        /// <param name="TestItemName">传入测试项目名称</param>
        /// <param name="QuarterValue">传入查询几个季度</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetTestItemDataFromSN(string ModelName,string StationName, string SN, string TestItemName, int QuarterValue)
        {
            try
            {
                string QuerySql = "";
                int QuarterCount = 0; // 判断当前季度是否超过最后一个季度
                int CurrentYear =Convert.ToInt32(DateTime.Now.ToString("yy")); // 获取当前年份
                string CurrentMonth = DateTime.Now.ToString("MM"); // 获取当前月份
                int Quarter = int.Parse(Math.Ceiling(Math.Round((double)(Convert.ToDouble(CurrentMonth)) / 3, 4)).ToString()); // 计算当前季度
                int TableQuarter = Quarter;
                for (int i = 0; i < QuarterValue; i++)
                {
                    string TableName = $"{ModelName.Replace('-', '_').ToLower()}_{CurrentYear}_{TableQuarter}";
                    QuerySql += $"SELECT SN,Time,Station,Testlog FROM {TableName} WHERE Station='{StationName}'AND SN='{SN}' UNION ";
                    TableQuarter--;
                    QuarterCount++;
                    if(QuarterCount== Quarter|| QuarterCount == 4)
                    {
                        TableQuarter = 4;
                        QuarterCount = 0;
                        CurrentYear--;
                    }
                }
                QuerySql = QuerySql.TrimEnd("UNION ".ToCharArray());
                QuerySql += " ORDER BY Time DESC";
                Mysql context = new(GetConfiguration.testLogMysql);
                ApiResponse result = await context.DataQuery(QuerySql, 100000);
                if (result.status != 200) return result;
                List<Dictionary<string, object>> TestData = (List<Dictionary<string,object>>)result.data;
                if (TestData.Count == 0)
                {
                    string Message = QuarterValue == 1 ? "当前季度" : $"当前季度往前{QuarterValue-1}个季度";
                    return ApiResponse.Error(500, $"该SN{Message}在{StationName}站暂无测试数据");
                }
                string[] TestItemData = TestData[0]["Testlog"].ToString().Split("#,".ToCharArray());
                int TestItemIndex = Array.IndexOf(TestItemData, TestItemName);
                if (TestItemIndex == -1)
                {
                    return ApiResponse.Error(500, $"该SN测试数据不存在测试项{TestItemName}");
                }
              return ApiResponse.OK(TestItemData[TestItemIndex+2]);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取指定站别、SN和测试项的测试值数据(GetTestDataFromSN)异常,异常信息{ex.Message}", "SNDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion
    }
}
