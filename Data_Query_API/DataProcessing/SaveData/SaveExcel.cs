using Data_Query_API.GeneralMethod;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.Drawing;

namespace Data_Query_API.SaveData
{
    /// <summary>
    /// 保存为 Excel 数据处理区域
    /// </summary>
    public class SaveExcel
    {
        #region 测试数据统计
        /// <summary>
        /// 数据统计
        /// </summary>
        /// <param name="TestStatistics">传入测试数据</param>
        /// <param name="result">传入测试结果KEY名称</param>
        /// <param name="SN">传入SN KEY名称</param>
        /// <returns></returns>
        public static Dictionary<string, string> TotalDataAalysis(List<Dictionary<string, object>> TestStatistics, string result, string SN)
        {
            Dictionary<string, string> DataAnalysisTitle = new() { };
            string FalseValue = "";
            string TrueValue = "";
            if (SN.Equals("UID"))
            {
                FalseValue = "F";
                TrueValue = "P";
            }
            else
            {
                FalseValue = "False";
                TrueValue = "True";
            }
            try
            {
                // 测试记录
                // 存储测试总次数
                int Total_number_of_tests = TestStatistics.Count;
                // 存储测试失败总次数
                int Number_of_test_failures = TestStatistics.Where(x => x[result].ToString().Equals(FalseValue)).Count();
                // 存储测试成功总次数
                int Test_success_times = Total_number_of_tests - Number_of_test_failures;

                // 测试产品
                // 存储去除特殊条码的测试数据
                var test_data = TestStatistics.Where(x => !x[SN].ToString().Contains("TE_BZP") && !x[SN].ToString().Contains("TE_BZP_EN")
                && !x[SN].ToString().Contains("TE_BZP_QC") && !x[SN].ToString().Contains("TE_BZP_MF"));
                // 存储测试产品数
                int Total_number_of_products_tested = test_data.GroupBy(x => x[SN].ToString()).Count();
                // 得到去除特殊条码测试为Pass并以SN去重的所有数据
                var Test_product_pass_data = test_data.Where(x => x[result].ToString().Equals(TrueValue)).Select(x => x[SN].ToString()).Distinct();
                // 得到去除特殊条码测试为Fail并以SN去重的所有数据
                var Test_product_Fail_data = test_data.Where(x => x[result].ToString().Equals(FalseValue)).Select(x => x[SN].ToString()).Distinct();
                // 直通产品数
                int Number_of_direct_products = Test_product_pass_data.Except(Test_product_Fail_data).ToList().Count;// 存储通过计算Pass和Fail集合的差集得到直通产品数
                // 不良产品数
                int Number_of_defective_products = Test_product_Fail_data.Except(Test_product_pass_data).ToList().Count;// 存储通过计算Fail和Pass集合的差集得到不良产品数
                // 重测产品数
                int Total_number_of_retests = Test_product_Fail_data.Count() - Number_of_defective_products;

                // 存储计算的直通率(直通产品数/测试产品数)
                double Pass_through_rate = Math.Round((double)Number_of_direct_products / Total_number_of_products_tested * 100, 2);
                if (double.IsNaN(Pass_through_rate)) Pass_through_rate = 0;
                // 存储计算的直通率(新重测产品数/测试产品数)
                double Retest_rate_new = Math.Round((double)Total_number_of_retests / Total_number_of_products_tested * 100, 2);
                if (double.IsNaN(Retest_rate_new)) Retest_rate_new = 0;
                // 存储计算的不良率(不良产品数/测试产品数)
                double Defective_rate = Math.Round((double)Number_of_defective_products / Total_number_of_products_tested * 100, 2);
                if (double.IsNaN(Defective_rate)) Defective_rate = 0;
                // 得到重测总次数
                int Retest_times = test_data.Count() - Total_number_of_products_tested;
                // 统计旧测试重测率
                double Retest_rate_old = Math.Round((double)Retest_times / Total_number_of_products_tested * 100, 2);
                if (double.IsNaN(Retest_rate_old)) Retest_rate_old = 0;

                // 得到TE_BZP条码的测试数据
                int Engineering_barcode_times = TestStatistics.Where(x => x[SN].ToString().Contains("EN_TEST")).Count();
                // 得到Te_BZPWX条码的测试数据
                int Quality_bar_code_times = TestStatistics.Where(x => x[SN].ToString().Contains("QC_TEST")).Count();
                // 得到Te_BZPWX条码的测试数据
                int Manufacturing_barcode_times = TestStatistics.Where(x => x[SN].ToString().Contains("MF_TEST")).Count();
                // 存储TE_BZP测试总次数
                int Te_BZP_Count = TestStatistics.Where(x => x[SN].ToString().Contains("TE_BZP")).Count();

                DataAnalysisTitle.Add("测试产品数", Total_number_of_products_tested.ToString());
                DataAnalysisTitle.Add("直通产品数", Number_of_direct_products.ToString());
                DataAnalysisTitle.Add("重测产品数", Total_number_of_retests.ToString());
                DataAnalysisTitle.Add("不良产品数", Number_of_defective_products.ToString());
                DataAnalysisTitle.Add("直通率", $"{Pass_through_rate}%");
                DataAnalysisTitle.Add("重测率(新)", $"{Retest_rate_new}%");
                DataAnalysisTitle.Add("重测率(旧)", $"{Retest_rate_old}%");
                DataAnalysisTitle.Add("不良率", $"{Defective_rate}%");
                DataAnalysisTitle.Add("测试总次数", Total_number_of_tests.ToString());
                DataAnalysisTitle.Add("测试成功总次数", Test_success_times.ToString());
                DataAnalysisTitle.Add("测试失败总次数", Number_of_test_failures.ToString());
                DataAnalysisTitle.Add("包含TE_BZP测试总次数", Te_BZP_Count.ToString());
                DataAnalysisTitle.Add("工程专用条码测试次数", Engineering_barcode_times.ToString());
                DataAnalysisTitle.Add("品质专用条码测试次数", Quality_bar_code_times.ToString());
                DataAnalysisTitle.Add("制造专用条码测试次数", Manufacturing_barcode_times.ToString());
            }
            catch (Exception ex)
            {
                DataAnalysisTitle.Add("测试产品数", "0");
                DataAnalysisTitle.Add("直通产品数", "0");
                DataAnalysisTitle.Add("重测产品数", "0");
                DataAnalysisTitle.Add("不良产品数", "0");
                DataAnalysisTitle.Add("直通率", "0%");
                DataAnalysisTitle.Add("重测率(新)", "0%");
                DataAnalysisTitle.Add("重测率(旧)", "0%");
                DataAnalysisTitle.Add("测试总次数", "0");
                DataAnalysisTitle.Add("测试成功总次数", "0");
                DataAnalysisTitle.Add("测试失败总次数", "0");
                DataAnalysisTitle.Add("包含TE_BZP测试总次数", "0");
                DataAnalysisTitle.Add("工程专用条码测试次数", "0");
                DataAnalysisTitle.Add("品质专用条码测试次数", "0");
                DataAnalysisTitle.Add("制造专用条码测试次数", "0");
                Log.LogWrite(ex.Message.ToString(),"SaveExcelLog");
            }
            return DataAnalysisTitle;
        }
        #endregion

        #region 测试电脑数据统计
        /// <summary>
        /// 测试电脑统计
        /// </summary>
        /// <param name="TestStatistics">传入测试数据</param>
        /// <param name="result">传入测试结果KEY名称</param>
        /// <param name="SN">传入SN KEY名称</param>
        /// <param name="ComputerStatisticalResults">返回上传的电脑测试数据</param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> Test_computer_data_statistics(List<Dictionary<string, object>> TestStatistics, string result, string SN, out string ComputerStatisticalResults)
        {
            ComputerStatisticalResults = "";
            Dictionary<string, Dictionary<string, string>> value = new() { };
            var TestComputerStatistics = TestStatistics.GroupBy(x => x["MachineName"].ToString()).Where(x => x.Any()); ;
            foreach (var item in TestComputerStatistics)
            {
                // 获取指定电脑编号测试总次数
                //var Total_number_of_computer_tests_tested = TestStatistics.Where(x => x["MachineName"].ToString().Equals(item.Key.ToString()));
                List<Dictionary<string, object>> TestData = TestStatistics.Where(x => x["MachineName"].ToString().Equals(item.Key.ToString())).ToList();

                Dictionary<string, string> computer = TotalDataAalysis(TestData, result, SN);
                value.Add(item.Key.ToString(), computer);

                ComputerStatisticalResults += $"测试电脑编号:{item.Key}";
                foreach (var Kvp in computer)
                {
                    ComputerStatisticalResults += $",{Kvp.Key}:{Kvp.Value}";
                }
                ComputerStatisticalResults += "#";
                //ComputerStatisticalResults += $"测试电脑编号:{item.Key},测试总次数:{Test_data_statistics_value[0]},测试成功次数:{Test_data_statistics_value[1]}," +
                //   $"测试失败次数:{Test_data_statistics_value[2]},测试产品总数:{Test_data_statistics_value[3]}," +
                //   $"重测总次数:{Test_data_statistics_value[4]},测试Pass产品数:{Test_data_statistics_value[5]},标准品测试总次数:{Test_data_statistics_value[6]}," +
                //   $"维修品测试总次数:{Test_data_statistics_value[7]},重测率:{Test_data_statistics_value[8]}%,直通率:{Test_data_statistics_value[9]}%#";
            }
            return value;
        }
        #endregion

        #region 测试项失败统计
        /// <summary>
        /// 测试项失败统计
        /// </summary>
        /// <param name="TestStatistics">传入测试数据</param>
        /// <param name="result">传入测试结果KEY名称</param>
        /// <param name="RresultValue">传入测试结果比对值</param>
        /// <returns></returns>
        public static Dictionary<string, int> FailedItemStatistics(List<Dictionary<string, object>> TestStatistics, string result, string RresultValue)
        {
            Dictionary<string, int> FailureStatistics = new();
            // 获取测试失败的总数
            var FalseValues = TestStatistics.Where(y => y[result].ToString().Equals(RresultValue));
            foreach (var item in FalseValues)
            {
                string[] TestLogValue = item["Testlog"].ToString().Split(new char[2] { '#', ',' }); // 将数据分割;
                for (int i = 0; i < TestLogValue.Length - 3; i += 5)
                {
                    if (TestLogValue[i + 1].Equals("F"))
                    {
                        if (!FailureStatistics.ContainsKey(TestLogValue[i]))
                        {
                            FailureStatistics.Add(TestLogValue[i], 1);
                        }
                        else
                        {
                            FailureStatistics[TestLogValue[i]] = FailureStatistics[TestLogValue[i]] + 1;
                        }
                        break;
                    }
                }
            }
            return FailureStatistics;
        }
        #endregion

        #region 解析测试数据标题/上限/下限
        /// <summary>
        /// 解析测试数据标题/上限/下限
        /// </summary>
        /// <param name="TestDataTitle">传入页面的Title</param>
        /// <param name="TestDataLimit">传入页面的上下限shuj</param>
        /// <returns></returns>
        public static List<List<string>> HeaderData(List<Dictionary<string, string>> TestDataTitle, List<Dictionary<string, string>> TestDataLimit)
        {
            List<List<string>> value = new();
            List<string> DataTitle = new();
            List<string> UpperLimit = new();
            List<string> LowerLimit = new();
            List<string> TitleName = new();
            foreach (var item in TestDataTitle)
            {
                DataTitle.Add(item["label"].ToString());
                UpperLimit.Add(TestDataLimit[1][item["prop"].ToString()].ToString());
                LowerLimit.Add(TestDataLimit[0][item["prop"].ToString()].ToString());
                TitleName.Add(item["prop"].ToString());
            }
            value.Add(DataTitle);
            value.Add(UpperLimit);
            value.Add(LowerLimit);
            value.Add(TitleName);
            return value;
        }
        #endregion       

        #region 写入统计数据通用方法
        /// <summary>
        /// 写入统计数据通用方法
        /// </summary>
        /// <param name="DataValue"></param>
        /// <param name="worksheet"></param>
        /// <param name="StartPosition"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int WriteStatistics(Dictionary<string, Dictionary<string, string>> DataValue, ExcelWorksheet worksheet, int StartPosition, string text)
        {
            bool TtitleFlag = true;
            int Count = 1;
            foreach (var item in DataValue)
            {
                if (Count == 1)
                    worksheet.Cells[$"A{StartPosition - 1}:{GetCol(item.Value.Count + 1)}{StartPosition - 1}"].Merge = true; //合并单元格
                int i = 1;
                foreach (var kvp in item.Value)
                {
                    if (TtitleFlag)
                    {
                        if (i == 1)
                        {
                            worksheet.Cells[StartPosition, i].Value = text;
                            worksheet.Cells[StartPosition, i + 1].Value = kvp.Key;
                        }
                        else worksheet.Cells[StartPosition, i].Value = kvp.Key;
                    }
                    if (kvp.Key.Contains("重测率(新)"))
                    {
                        worksheet.Cells[StartPosition + Count, i].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                        worksheet.Cells[StartPosition + Count, i].Value = kvp.Value;
                        // 将重测率大于10%的标黄
                        RetestRateColor(Count, Convert.ToDouble((kvp.Value).Trim('%')), worksheet, StartPosition, item.Value.Count);
                    }
                    else if (kvp.Key.Contains("直通率"))
                    {
                        worksheet.Cells[StartPosition + Count, i].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                        worksheet.Cells[StartPosition + Count, i].Value = kvp.Value;
                        // 将直通率小于90%的标红
                        PassThroughRateColor(Count, Convert.ToDouble((kvp.Value).Trim('%')), worksheet, StartPosition, item.Value.Count + 1);
                    }
                    else if (kvp.Key.Contains("重测率(旧)") || kvp.Key.Contains("不良率"))
                    {
                        worksheet.Cells[StartPosition + Count, i].Style.Numberformat.Format = "#,##0.00";// 保留两位小数
                        worksheet.Cells[StartPosition + Count, i].Value = kvp.Value;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            worksheet.Cells[StartPosition + Count, i].Value = item.Key;
                            worksheet.Cells[StartPosition + Count, i + 1].Value = Convert.ToInt32(kvp.Value);
                            i++;
                        }
                        else worksheet.Cells[StartPosition + Count, i].Value = Convert.ToInt32(kvp.Value);
                    }
                    i++;
                }

                Count++;
                TtitleFlag = false;
            }
            return Count;
        }

        #endregion

        #region 写入测试数据通用方法
        /// <summary>
        /// 写入测试数据通用方法
        /// </summary>
        /// <param name="Title">传入标题数据</param>
        /// <param name="StartPosition">传入起始位置编号</param>
        /// <param name="TestDataWork">操作对象</param>
        /// <param name="HeaderData">头部固定数据</param>
        /// <param name="Data">写入数据</param>
        /// <param name="color">首行颜色</param>
        /// <returns></returns>
        public static bool WriteData(string Title, int StartPosition, ExcelWorksheet TestDataWork, List<List<string>> HeaderData, List<Dictionary<string, object>> Data, Color color)
        {
            try
            {
                TestDataWork.Cells[$"A{StartPosition}:{GetCol(HeaderData[0].Count)}{StartPosition}"].Merge = true; //合并单元格
                TestDataWork.Cells[StartPosition, 1].Value = Title;
                SetCell(TestDataWork, StartPosition);
                //TestDataWork.Row(StartPosition).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                StartPosition++;
                for (int i = 0; i < HeaderData[0].Count; i++)
                {
                    // 写入标题
                    TestDataWork.Cells[StartPosition, 1 + i].Value = HeaderData[0][i];
                    // 写入上限
                    TestDataWork.Cells[StartPosition + 1, 1 + i].Value = HeaderData[1][i];
                    // 写入下限
                    TestDataWork.Cells[StartPosition + 2, 1 + i].Value = HeaderData[2][i];
                    // 设置背景色
                    TestDataWork.Cells[StartPosition, 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    TestDataWork.Cells[StartPosition, 1 + i].Style.Fill.BackgroundColor.SetColor(color);
                }

                // 设置居中显示
                TestDataWork.Row(StartPosition).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // 设置文字大小
                TestDataWork.Row(StartPosition).Style.Font.Size = 12;

                StartPosition += 3;

                // 将测试数据写入
                foreach (var item in Data)
                {
                    for (int j = 0; j < HeaderData[0].Count; j++)
                    {
                        try
                        {
                            TestDataWork.Cells[StartPosition, j + 1].Value = item[HeaderData[3][j]].ToString();
                        }
                        catch
                        {
                            TestDataWork.Cells[StartPosition, j + 1].Value = "NULL";
                        }
                    }
                    // 设置测试记录为False的背景色为红色
                    if (item["Result"].ToString().Equals("False"))
                    {
                        // 设置背景色
                        for (int k = 0; k < HeaderData[3].Count; k++)
                        {
                            TestDataWork.Cells[StartPosition, k + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            TestDataWork.Cells[StartPosition, k + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }
                    }
                    StartPosition += 1;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.Message.ToString(), "SaveExcelLog");
                return false;
            }
        }

        #endregion

        #region Excel格式化通用方法
        /// <summary>
        /// 直通率标记方法
        /// </summary>
        /// <param name="Xaxis">传入单元格位置</param>
        /// <param name="value">传入重测率值</param>
        /// <param name="worksheet">传入操作对象</param>
        /// <param name="StartPosition">单元格起始位置</param>
        /// <param name="length">标记格数</param>
        /// <returns></returns>

        public static bool PassThroughRateColor(int Xaxis, double value, ExcelWorksheet worksheet, int StartPosition, int length)
        {
            if (value < 90)
            {
                // 设置背景色
                for (int j = 0; j < length; j++)
                {
                    worksheet.Cells[StartPosition + Xaxis, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[StartPosition + Xaxis, j + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重测率标记方法
        /// </summary>
        /// <param name="Xaxis">传入单元格位置</param>
        /// <param name="value">传入重测率值</param>
        /// <param name="worksheet">传入操作对象</param>
        /// <param name="StartPosition">单元格起始位置</param>
        /// <param name="length">标记格数</param>
        /// <returns></returns>
        public static bool RetestRateColor(int Xaxis, double value, ExcelWorksheet worksheet, int StartPosition, int length)
        {
            return true;
            //if (value > 10)
            //{
            //    // 设置背景色
            //    for (int j = 0; j < length; j++)
            //    {
            //        worksheet.Cells[StartPosition + Xaxis, j + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //        worksheet.Cells[StartPosition + Xaxis, j + 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
            //    }
            //    return true;
            //}
            //return false;
        }

        /// <summary>
        /// 设置单元格格式
        /// </summary>
        /// <param name="worksheet">传入操作对象</param>
        /// <param name="StartPosition">单元格位置</param>
        public static void SetCell(ExcelWorksheet worksheet, int StartPosition)
        {
            using (ExcelRange range = worksheet.Cells[StartPosition, 1, StartPosition, 1])
            {
                // 字体加粗
                range.Style.Font.Bold = true;
                // 设置居中显示
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                // 设置字体
                range.Style.Font.Name = "微软雅黑";
                // 设置文字大小
                //range.Style.Font.Size = 15;
            }
        }
        #endregion

        #region 保存统计数据通用方法
        /// <summary>
        /// 根据数值获取Excel列
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static string GetCol(int col)
        {
            //col从1开始
            string res = "";
            //col输入始终为非负数
            int remain = (col - 1) % 26;
            char addChar = (char)('A' + remain);
            res = addChar + res;
            col = (col - 1) / 26;
            while (col >= 1)
            {
                int left = (col - 1) % 26;
                char add = (char)('A' + left);
                res = add + res;
                col = (col - 1) / 26;
            }
            return res;
        }
        /// <summary>
        /// 保存统计数据为Excel表
        /// </summary>
        /// <param name="Path">传入路径</param>
        /// <param name="name">传入工作表名</param>
        /// <param name="TestDataStatistics">传入统计数据</param>
        /// <param name="NodeValues">传入节点测试数据统计</param>
        /// <param name="ComputerValues">传入测试电脑测试情况统计</param>
        /// <param name="FailureStatistics">传入测试项失败统计</param>
        /// <param name="flag">传入是否为存储测试数据</param>
        /// <returns></returns>    
        public static bool SaveDataAnalysis(string Path, string name, Dictionary<string, object> TestDataStatistics, Dictionary<string, Dictionary<string, string>> NodeValues, Dictionary<string, Dictionary<string, string>> ComputerValues, Dictionary<string, int> FailureStatistics, bool flag)
        {
            try
            {
                FileInfo newFile = new(Path);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using ExcelPackage package = new(newFile);
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(name);
                // 定义通用行起点
                int StartPosition = 2;

                #region 测试总数据统计写入
                // 测试数据分析写入
                worksheet.Cells[$"A1:{GetCol(TestDataStatistics.Count)}1"].Merge = true; //合并单元格
                worksheet.Cells[1, 1].Value = "测试数据统计";
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                SetCell(worksheet, 1);
                int num = 0;
                // 创建标题
                foreach (KeyValuePair<string, object> kvp in TestDataStatistics)
                {
                    worksheet.Cells[StartPosition, num + 1].Value = kvp.Key;
                    worksheet.Cells[StartPosition + 1, num + 1].Value = kvp.Value;
                    // 将直通率小于90%的标红
                    if (kvp.Key.Contains("直通率"))
                    {
                        PassThroughRateColor(1, Convert.ToDouble(kvp.Value.ToString().Trim('%')), worksheet, StartPosition, TestDataStatistics.Count);
                    }
                    // 将重测率大于10%的标黄
                    if (kvp.Key.Contains("重测率(新)"))
                    {
                        RetestRateColor(1, Convert.ToDouble(kvp.Value.ToString().Trim('%')), worksheet, StartPosition, TestDataStatistics.Count - 1);
                    }
                    num += 1;
                }
                #endregion

                StartPosition = 5;

                if (flag)
                {
                    #region 节点测试数据写入
                    // 节点测试数据写入
                    worksheet.Cells[StartPosition, 1].Value = "节点测试数据统计";
                    worksheet.Cells[StartPosition, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    SetCell(worksheet, StartPosition);
                    StartPosition += 1;
                    int Node_Count = WriteStatistics(NodeValues, worksheet, StartPosition, "节点名称");
                    #endregion
                    StartPosition += Node_Count;
                    if (StartPosition < 15) StartPosition = 15;
                }

                #region 测试电脑数据分析写入Excel
                // 测试电脑数据分析写入Excel
                worksheet.Cells[StartPosition, 1].Value = "测试电脑数据统计";
                worksheet.Cells[StartPosition, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                SetCell(worksheet, StartPosition);

                StartPosition++;
                int Computer_Count = WriteStatistics(ComputerValues, worksheet, StartPosition, "测试电脑编号");
                StartPosition++;
                // 生成饼图
                DrawPieChart(worksheet, StartPosition, StartPosition + Computer_Count - 2, 12, 1, (StartPosition + Computer_Count + 3) * 17, 10, 280, 300, "测试电脑失败次数比例");

                // 生成柱状图
                DrawColumnChart(worksheet, StartPosition, StartPosition + Computer_Count - 2, 3, 4, 5, 1, (StartPosition + Computer_Count + 3) * 17, 300, 300, 500, "测试电脑测试数据对比", "数量（次）", "测试电脑编号");
                #endregion

                // 处理单元格位置
                StartPosition += Computer_Count + 18;
                if (flag)
                {
                    if (StartPosition < 38) StartPosition = 38;
                }
                else
                {
                    if (StartPosition < 30) StartPosition = 30;
                }
                #region 测试项失败分析写入
                worksheet.Cells["A" + StartPosition + ":B" + StartPosition].Merge = true; //合并单元格
                worksheet.Cells[StartPosition, 1].Value = "测试项失败分析";
                worksheet.Cells[StartPosition, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                SetCell(worksheet, StartPosition);
                StartPosition += 1;
                worksheet.Cells[StartPosition, 1].Value = "测试项名称";
                worksheet.Cells[StartPosition, 2].Value = "失败次数";
                StartPosition += 1;
                int Fail_Item = 0;
                foreach (KeyValuePair<string, int> Fail in FailureStatistics)
                {
                    worksheet.Cells[StartPosition + Fail_Item, 1].Value = Fail.Key;
                    worksheet.Cells[StartPosition + Fail_Item, 2].Value = Convert.ToInt32(Fail.Value);
                    Fail_Item += 1;
                }
                // 生成饼图
                DrawPieChart(worksheet, StartPosition, StartPosition + Fail_Item - 1, 2, 1, (StartPosition + 4) * 17, 180, 400, 280, "测试项失败次数比例");
                #endregion

                StartPosition = StartPosition + Fail_Item + 10;
                //设置单元格边框
                for (int k = 1; k < StartPosition; k++)
                {
                    for (int j = 1; j <= 18; j++)
                    {
                        worksheet.Row(k).Height = 17;//设置行高
                        worksheet.Cells[k, j].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(41, 36, 33));//设置单元格所有边框
                    }
                }
                // 设置单元格宽度自适应
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                worksheet.Protection.SetPassword("merryTE");
                package.Save();
                //registered($"{Path}存储成功\r\n");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.Message.ToString(), "SaveExcelLog");
                //registered(ex.Message.ToString()+"\r\n");
                return false;
            }
        }
        #endregion

        #region Excel生成饼图
        /// <summary>
        /// 生成饼图
        /// </summary>
        /// <param name="worksheet">传入当前操作excel的对象</param>
        /// <param name="start">传入开始的位置</param>
        /// <param name="end">传入结束的位置</param>
        /// <param name="DataCells">传入数据列</param>
        /// <param name="TitleCells">传入标题列</param>
        /// <param name="top">传入距离顶部距离</param>
        /// <param name="left">传入左边距离</param>
        /// <param name="width">传入宽度</param>
        /// <param name="height">传入高度</param>
        /// <param name="title">传入标题</param>
        private static void DrawPieChart(ExcelWorksheet worksheet, int start, int end, int DataCells, int TitleCells, int top, int left, int width, int height, string title)
        {
            ExcelChart chart = worksheet.Drawings.AddChart(title, eChartType.Pie3D);
            ExcelChartSerie serie = chart.Series.Add(worksheet.Cells[start, DataCells, end, DataCells], worksheet.Cells[start, TitleCells, end, TitleCells]);
            serie.HeaderAddress = worksheet.Cells[TitleCells, DataCells];
            chart.SetPosition(top, left);
            chart.SetSize(width, height);
            chart.Title.Text = title;
            chart.Title.Font.Size = 15;
            chart.Title.Font.Bold = true;
            chart.Style = eChartStyle.Style18;
            chart.Legend.Border.LineStyle = eLineStyle.Solid;
            chart.Legend.Border.Fill.Color = Color.FromArgb(220, 220, 220);

        }
        #endregion

        #region Excel生成柱形图
        /// <summary>
        /// 生成柱形图
        /// </summary>
        /// <param name="worksheet">传入当前操作excel的对象</param>
        /// <param name="start">传入开始的位置</param>
        /// <param name="end">传入结束的位置</param>
        /// <param name="DataCellsOne">传入第一个数据列</param>
        /// <param name="DataCellsTwo">传入第二个数据列</param>
        /// <param name="DataCellsThree">传入第二个数据列</param>
        /// <param name="TitleCells">传入标题列</param>
        /// <param name="top">传入距离顶部距离</param>
        /// <param name="left">传入左边距离</param>
        /// <param name="title">传入标题</param>
        /// <param name="width">传入图标宽度</param>
        /// <param name="height">传入图标高度</param>
        /// <param name="YTitle">传入Y名称</param>
        /// <param name="XTitle">传入X轴名称</param>
        private static void DrawColumnChart(ExcelWorksheet worksheet, int start, int end, int DataCellsOne, int DataCellsTwo, int DataCellsThree, int TitleCells, int top, int left, int height, int width, string title, string YTitle, string XTitle)
        {
            ExcelChart chart1 = worksheet.Drawings.AddChart(title, eChartType.ColumnClustered);
            ExcelChartSerie serie = chart1.Series.Add(worksheet.Cells[start, DataCellsOne, end, DataCellsOne], worksheet.Cells[start, TitleCells, end, TitleCells]);
            serie.HeaderAddress = worksheet.Cells[start - 1, DataCellsOne];
            ExcelChartSerie serie1 = chart1.Series.Add(worksheet.Cells[start, DataCellsTwo, end, DataCellsTwo], worksheet.Cells[start, TitleCells, end, TitleCells]);
            serie1.HeaderAddress = worksheet.Cells[start - 1, DataCellsTwo];
            ExcelChartSerie serie2 = chart1.Series.Add(worksheet.Cells[start, DataCellsThree, end, DataCellsThree], worksheet.Cells[start, TitleCells, end, TitleCells]);
            serie2.HeaderAddress = worksheet.Cells[start - 1, DataCellsThree];
            chart1.SetPosition(top, left);
            chart1.SetSize(width, height);
            chart1.Title.Text = title;
            chart1.Title.Font.Size = 15;
            chart1.Title.Font.Bold = true;
            chart1.YAxis.MinValue = 0;// 设置最下值
            chart1.YAxis.AddTitle(YTitle);
            chart1.XAxis.AddTitle(XTitle);
            chart1.Style = eChartStyle.Style18;
            chart1.Legend.Border.LineStyle = eLineStyle.Solid;
            chart1.Legend.Border.Fill.Color = Color.FromArgb(217, 217, 217);
        }
        #endregion

        #region Excel生成折线图
        /// <summary>
        /// 生成柱形图
        /// </summary>
        /// <param name="worksheet">传入当前操作excel的对象</param>
        /// <param name="start">传入开始的位置</param>
        /// <param name="end">传入结束的位置</param>
        /// <param name="DataCells">传入第一个数据列</param>
        /// <param name="DataCells1">传入第二个数据列</param>
        /// <param name="TitleCells">传入标题列</param>
        /// <param name="top">传入距离顶部距离</param>
        /// <param name="left">传入左边距离</param>
        /// <param name="title">传入标题</param>
        /// <param name="width">传入图表宽度</param>
        /// <param name="height">传入图表高度</param>
        /// <param name="YTitle">传入Y名称</param>
        /// <param name="XTitle">传入X轴名称</param>
        public static void DrawLineChart(ExcelWorksheet worksheet, int start, int end, int DataCells, int DataCells1, int TitleCells, int top, int left, int height, int width, string title, string YTitle, string XTitle)
        {
            ExcelChart chart1 = worksheet.Drawings.AddChart(title, eChartType.Line);
            ExcelChartSerie serie = chart1.Series.Add(worksheet.Cells[start, DataCells, end, DataCells], worksheet.Cells[start, TitleCells, end, TitleCells]);
            serie.HeaderAddress = worksheet.Cells[2, DataCells];
            ExcelChartSerie serie1 = chart1.Series.Add(worksheet.Cells[start, DataCells1, end, DataCells1], worksheet.Cells[start, TitleCells, end, TitleCells]);
            serie1.HeaderAddress = worksheet.Cells[2, DataCells1];
            chart1.SetPosition(top, left);
            chart1.SetSize(width, height);
            chart1.Title.Text = title;
            chart1.Title.Font.Size = 15;
            chart1.Title.Font.Bold = true;
            chart1.YAxis.MinValue = 0;// 设置最小值
            chart1.YAxis.AddTitle(YTitle);
            chart1.XAxis.AddTitle(XTitle);
            chart1.Style = eChartStyle.Style26;
            //chart1.Legend.Border.LineStyle = eLineStyle.Solid;
            //chart1.Legend.Border.Fill.Color = Color.FromArgb(217, 217, 217);
        }
        #endregion

        #region 保存测试数据通用方法
        /// <summary>
        /// 存储Excel测试数据通用方法
        /// </summary>
        /// <param name="Path">传入文件路径</param>
        /// <param name="TestData">测试数据</param>
        /// <param name="TestDataTitle">标题数据</param>
        /// <param name="TestDataLimit">Limlt数据</param>
        /// <returns></returns>
        public static bool SaveTestData(string Path, List<Dictionary<string, object>> TestData, List<Dictionary<string, string>> TestDataTitle, List<Dictionary<string, string>> TestDataLimit)
        {
            try
            {
                //registered($"{Path}进入，{TestDataTitle}\r\n");
                FileInfo newFile = new(Path);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using ExcelPackage package = new(newFile);
                #region 测试数据写入
                List<List<string>> HeaderDataValue = HeaderData(TestDataTitle, TestDataLimit);
                // List<List<string>> HeaderDataValue = HeaderData(TestData);
                //var TestStatistics = JArray.Parse(TestData);
                // 得到TE_BZP条码的测试数据
                var Te_BZP_Values = TestData.Where(x => x["SN"].ToString().Contains("TE_BZP")).ToList();
                // 得到工程条码的测试数据
                var EN_TEST_Values = TestData.Where(x => x["SN"].ToString().Contains("TE_BZP_EN")).ToList();
                // 得到品质条码的测试数据
                var QC_TEST_Values = TestData.Where(x => x["SN"].ToString().Contains("TE_BZP_QC")).ToList();
                // 得到制造条码的测试数据
                var MF_TEST_Values = TestData.Where(x => x["SN"].ToString().Contains("TE_BZP_MF")).ToList();
                // 得到去除特殊条码的测试数据
                var DataValues = TestData.Where(x => !x["SN"].ToString().Contains("TE_BZP") && !x["SN"].ToString().Contains("TE_BZP_EN")
                && !x["SN"].ToString().Contains("TE_BZP_QC") && !x["SN"].ToString().Contains("TE_BZP_MF")).ToList();

                ExcelWorksheet TestDataWork = package.Workbook.Worksheets.Add("测试数据页");
                // 定义通用行起点
                int StartPosition = 1;

                // 写入TE_BZP条码测试数据
                if (!WriteData("SN包含TE_BZP条码测试数据", StartPosition, TestDataWork, HeaderDataValue, Te_BZP_Values, Color.DarkSeaGreen)) return false;

                StartPosition += Te_BZP_Values.Count() + 5;
                // 写入EN_TEST测试数据
                if (!WriteData("工程专用条码(TE_BZP_EN_TEST)测试数据", StartPosition, TestDataWork, HeaderDataValue, EN_TEST_Values, Color.AliceBlue)) return false;
                StartPosition += EN_TEST_Values.Count() + 5;

                // 写入QC_TEST测试数据
                if (!WriteData("品质专用条码(TE_BZP_QC_TEST)测试数据", StartPosition, TestDataWork, HeaderDataValue, QC_TEST_Values, Color.Yellow)) return false;
                StartPosition += QC_TEST_Values.Count() + 5;

                // 写入MF_TEST测试数据
                if (!WriteData("制造专用条码(TE_BZP_MF_TEST)测试数据", StartPosition, TestDataWork, HeaderDataValue, MF_TEST_Values, Color.IndianRed)) return false;
                StartPosition += MF_TEST_Values.Count() + 5;

                // 写入测试数据
                if (!WriteData("测试数据", StartPosition, TestDataWork, HeaderDataValue, DataValues, Color.GreenYellow)) return false;

                #endregion
                StartPosition += DataValues.Count() + 3;
                //设置单元格边框
                //for (int k = 1; k < StartPosition; k++)
                //{
                //    for (int j = 1; j <= HeaderDataValue[0].Count; j++)
                //    {
                //        TestDataWork.Cells[k, j].Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.FromArgb(41, 36, 33));//设置单元格所有边框

                //    }
                //}
                // 设置单元格宽度自适应
                TestDataWork.Cells[TestDataWork.Dimension.Address].AutoFitColumns();
                TestDataWork.Protection.SetPassword("merryTE");
                package.Save();
                //registered($"{Path}存储成功\r\n");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.Message.ToString(), "SaveExcelLog");
                //registered(ex.Message.ToString() + "\r\n");
                return false;
            }
        }

        /// <summary>
        /// 存储LG Excel测试数据通用方法
        /// </summary>
        /// <param name="path">传入文件路径</param>
        /// <param name="FileName">文件名称</param>
        /// <param name="FileTime">文件夹名称日期时间</param>
        /// <param name="HeaderData">头部固定数据</param>
        /// <param name="DataValues">写入数据</param>
        /// <returns></returns>
        public static bool SaveLGData(string path, string FileName, string FileTime, List<string[]> HeaderData, List<Dictionary<string, object>> DataValues)
        {
            try
            {
                // 判断需不需要创建日期文件夹
                if (FileTime.Length > 0)
                {
                    path = $@"{path}\{FileTime}";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                }
                path = $@"{path}\Excel";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                path += $@"\{FileName}";
                FileInfo newFile = new(path);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using ExcelPackage package = new(newFile);
                ExcelWorksheet TestDataWork = package.Workbook.Worksheets.Add("测试数据页");

                int StartPosition = 1;
                // 设置居中显示
                TestDataWork.Row(StartPosition).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                // 设置文字大小
                TestDataWork.Row(StartPosition).Style.Font.Size = 12;
                foreach (var item in HeaderData)
                {
                    for (int i = 0; i < item.Length; i++)
                    {
                        TestDataWork.Cells[StartPosition, 1 + i].Value = item[i];

                    }
                    StartPosition += 1;
                }
                // 将测试数据写入
                foreach (var item in DataValues)
                {
                    for (int j = 0; j < HeaderData[0].Length; j++)
                    {
                        try
                        {
                            TestDataWork.Cells[StartPosition, j + 1].Value = item[HeaderData[0][j]].ToString();
                        }
                        catch
                        {
                            TestDataWork.Cells[StartPosition, j + 1].Value = "NULL";
                        }
                    }
                    // 设置测试记录为False的背景色为红色
                    if (item["Status"].ToString().Equals("F"))
                    {
                        // 设置背景色
                        for (int k = 0; k < HeaderData[3].Length; k++)
                        {
                            TestDataWork.Cells[StartPosition, k + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            TestDataWork.Cells[StartPosition, k + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                        }
                    }
                    StartPosition += 1;

                }
                StartPosition += DataValues.Count + 3;
                // 设置单元格宽度自适应
                TestDataWork.Cells[TestDataWork.Dimension.Address].AutoFitColumns();
                TestDataWork.Protection.SetPassword("merryTE");
                package.Save();
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite(ex.Message.ToString(), "SaveExcelLog");
                return false;
            }
        }
        #endregion
    }
}
