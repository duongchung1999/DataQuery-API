using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.SaveData;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Data_Query_API.DataProcessing.DataDailyData
{
    /// <summary>
    /// 日报表数据类
    /// </summary>
    public class DataDailyData
    {
        #region 获取日报数据所有表名
        /// <summary>
        /// 获取日报数据所有表名
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDailyTableName()
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                ApiResponse result = await context.DataQuery("select table_name from information_schema.tables where table_schema='test_1'", 3000);
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
                Log.LogWrite($"获取日报数据所有表名(GetDailyTableName)异常,异常信息{ex.Message}", "DailyDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据年获取当年所有有数据的机型
        /// <summary>
        /// 根据年获取当年所有有数据的机型
        /// </summary>
        /// <param name="Year">传入年份</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetModelFromYear(string Year)
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                return await context.DataQuery($"SELECT DISTINCT(Model) FROM Test_Statistics_{Year} ORDER BY Model", 1000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据年获取当年所有有数据的机型(GetModelFromYear)异常,异常信息{ex.Message}", "DailyDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 查询指定机型指定时间段每日生产数据统计
        /// <summary>
        ///  查询指定机型指定时间段每日生产数据统计
        /// </summary>
        /// <param name="Year">传入年份</param>
        /// <param name="Model">传入机型名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDailyData(string Year, string Model, string StartTime, string EndTime)
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                int StartTimeValue = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(StartTime));
                int EndTimeValue = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(EndTime));
                return await context.DataQuery($"SELECT Model,Station,Time,Total_number_of_tests,Test_success_times,Number_of_test_failures," +
                    $"Total_number_of_products_tested, Total_number_of_retests, Total_times_of_standard_test,Total_number_of_maintenance_product_tests," +
                    $" Retest_rate, Test_pass_through_rate from Test_Statistics_{Year} WHERE Model='{Model}' AND Time  BETWEEN {StartTimeValue} " +
                    $"AND {EndTimeValue}", 2000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"查询指定机型指定时间段日报表数据(GetDailyData)异常,异常信息{ex.Message}", "DailyDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 查询指定机型指定时间段单日生产数据统计
        /// <summary>
        /// 查询指定机型指定时间段单日生产数据统计
        /// </summary>
        /// <param name="Year">传入年份</param>
        /// <param name="Model">传入机型名称</param>
        /// <param name="StartTime">传入查询开始时间</param>
        /// <param name="EndTime">传入查询结束时间</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDailyReportData(string Year, string Model, string StartTime, string EndTime)
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                int StartTimeValue = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(StartTime));
                int EndTimeValue = Time_conversion.ToUnixTimestamp(Convert.ToDateTime(EndTime));
                return await context.DataQuery($"SELECT Model,Station,Total_number_of_tests,Retest_rate,Test_pass_through_rate,Node_data_statistics from Test_Statistics_{Year}" +
                    $" WHERE Model='{Model}' AND Time  BETWEEN {StartTimeValue} " +
                    $"AND {EndTimeValue}", 2000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"查询指定机型指定时间段日报表数据(GetDailyReportData)异常,异常信息{ex.Message}", "DailyDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 保存日报页面良率数据
        /// <summary>
        /// 保存日报页面良率数据
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="DailyReportData">传入页面日报数据</param>
        /// <param name="SaveType">存储数据类型</param>
        /// <returns></returns>
        public static string SaveDailyReport(string ModelName, string DailyReportData, string SaveType)
        {
            try
            {


                var DailyReportInfo = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(DailyReportData);
                string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\SaveDailyReport\{ModelName} 报表.xlsx";
                if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\SaveDailyReport")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\SaveDailyReport");
                FileInfo newFile = new(Path);
                if (newFile.Exists)
                {
                    newFile.Delete();
                    newFile = new FileInfo(Path);
                }
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using ExcelPackage package = new(newFile);

                foreach (var item in DailyReportInfo)
                {
                    // 定义通用行起点
                    int StartPosition = 1;
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add($"{item.Keys.ToArray()[0]} 测试日报表");
                    // 测试数据分析写入
                    worksheet.Cells["A" + StartPosition + ":J" + StartPosition].Merge = true; //合并单元格
                    worksheet.Cells[StartPosition, 1].Value = $"{item.Keys.ToArray()[0]} 测试日报表";
                    worksheet.Cells[StartPosition, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    using (ExcelRange range = worksheet.Cells[StartPosition, 1, StartPosition, 1])
                    {
                        // 字体加粗
                        range.Style.Font.Bold = true;
                        // 设置居中显示
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        // 设置字体
                        range.Style.Font.Name = "微软雅黑";
                        // 设置文字大小
                        // range.Style.Font.Size = 15;
                    }
                    StartPosition += 1;
                    worksheet.Cells[StartPosition, 1].Value = "时间";
                    worksheet.Cells[StartPosition, 2].Value = "测试总次数";
                    worksheet.Cells[StartPosition, 3].Value = "测试成功总次数";
                    worksheet.Cells[StartPosition, 4].Value = "测试失败总次数";
                    worksheet.Cells[StartPosition, 5].Value = "测试产品数";
                    worksheet.Cells[StartPosition, 6].Value = "重测总次数";
                    worksheet.Cells[StartPosition, 7].Value = "TE_BZP测试次数";
                    worksheet.Cells[StartPosition, 8].Value = "制造专用条码测试次数";
                    worksheet.Cells[StartPosition, 9].Value = "重测率";
                    worksheet.Cells[StartPosition, 10].Value = "直通率";
                    StartPosition += 1;
                    int num = 0;
                    foreach (var value in item.Values.ToList()[0])
                    {
                        if (SaveType.Equals("单日"))
                        {
                            worksheet.Cells[StartPosition + num, 1].Value = value["节点名称"];
                            worksheet.Cells[StartPosition + num, 2].Value = value["测试总次数"];
                            worksheet.Cells[StartPosition + num, 3].Value = value["测试成功总次数"];
                            worksheet.Cells[StartPosition + num, 4].Value = value["测试失败总次数"];
                            worksheet.Cells[StartPosition + num, 5].Value = value["测试产品数"];
                            worksheet.Cells[StartPosition + num, 6].Value = value["重测产品数"];
                            worksheet.Cells[StartPosition + num, 7].Value = value["包含TE_BZP测试总次数"];
                            worksheet.Cells[StartPosition + num, 8].Value = value["制造专用条码测试次数"];
                            worksheet.Cells[StartPosition + num, 9].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                            worksheet.Cells[StartPosition + num, 9].Value = Convert.ToDouble(value["重测率新"].Replace('%', ' '));
                            worksheet.Cells[StartPosition + num, 10].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                            worksheet.Cells[StartPosition + num, 10].Value = Convert.ToDouble(value["直通率"].Replace('%', ' '));

                            // 将直通率小于90%的标红
                            if (Convert.ToDouble(value["直通率"].Replace('%', ' ')) < 90)
                            {
                                // 设置背景色
                                for (int j = 0; j < 10; j++)
                                {
                                    worksheet.Cells[StartPosition + num, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells[StartPosition + num, j + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                }
                            }
                        }
                        else
                        {
                            worksheet.Cells[StartPosition + num, 1].Value = value["Time"];
                            worksheet.Cells[StartPosition + num, 2].Value = value["Total_number_of_tests"];
                            worksheet.Cells[StartPosition + num, 3].Value = value["Test_success_times"];
                            worksheet.Cells[StartPosition + num, 4].Value = value["Number_of_test_failures"];
                            worksheet.Cells[StartPosition + num, 5].Value = value["Total_number_of_products_tested"];
                            worksheet.Cells[StartPosition + num, 6].Value = value["Total_number_of_retests"];
                            worksheet.Cells[StartPosition + num, 7].Value = value["Total_times_of_standard_test"];
                            worksheet.Cells[StartPosition + num, 8].Value = value["Total_number_of_maintenance_product_tests"];
                            worksheet.Cells[StartPosition + num, 9].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                            worksheet.Cells[StartPosition + num, 9].Value = Convert.ToDouble(value["Retest_rate"]);
                            worksheet.Cells[StartPosition + num, 10].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                            worksheet.Cells[StartPosition + num, 10].Value = Convert.ToDouble(value["Test_pass_through_rate"]);

                            // 将直通率小于90%的标红
                            if (Convert.ToDouble(value["Test_pass_through_rate"]) < 90)
                            {
                                // 设置背景色
                                for (int j = 0; j < 10; j++)
                                {
                                    worksheet.Cells[StartPosition + num, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells[StartPosition + num, j + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                                }
                            }
                        }
                        num += 1;
                    }
                    StartPosition += item.Values.ToList()[0].Count;

                    // 生成折线图
                    SaveExcel.DrawLineChart(worksheet, StartPosition - item.Values.ToList()[0].Count, StartPosition - 1, 9, 10, 1, StartPosition * 18, 10, 300, 500, $"{item.Keys.ToArray()[0]} 测试统计图", "单位(%)", "时间");
                    StartPosition += 18;
                    //设置单元格边框
                    for (int k = 1; k < StartPosition; k++)
                    {
                        for (int j = 1; j <= 10; j++)
                        {
                            worksheet.Row(k).Height = 17;//设置行高
                            worksheet.Cells[k, j].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(41, 36, 33));//设置单元格所有边框
                        }
                    }
                    worksheet.Protection.SetPassword("merryTE");
                    // 设置单元格宽度自适应
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    package.Save();
                }
                return Path;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"保存日报页面良率数据(SaveDailyReport)异常,异常信息{ex.Message}", "DailyDataLog");
                return "";
            }
        }
        #endregion
    }
}
