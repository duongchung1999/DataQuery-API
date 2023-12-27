using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Quartz;

namespace Data_Query_API.DataProcessing.RealTimeDataStatistics
{
    /// <summary>
    /// 实时数据生成
    /// </summary>
    public class RealTimeDataGeneration: IJob
    {
        static ApiResponse Query_statistics_Value;

        /// <summary>
        /// 实现定时接口内容
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Execute(IJobExecutionContext context)
        {
            // 调用实时数据
            await Task.Run(() =>
            {
                Query_statistics();
                var a = 1123;
            });
        }

        /// <summary>
        /// 对外统计数据接口
        /// </summary>
        /// <returns></returns>
        public ApiResponse Get_Query_statistics()
        {
            return Query_statistics_Value;
        }
        /// <summary>
        /// 生成数据接口
        /// </summary>
        public async void Query_statistics()
        {
            List<Dictionary<string, object>> valuePairs = new();
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                // 获取统计表数据
                ApiResponse TestStatisticsValue = await context.DataQuery("SELECT * from Test_statistics ORDER BY Model,Station;", 2000);
                // 获取电脑统计表数据
                ApiResponse ComputerStatisticsValue = await context.DataQuery("SELECT * from Computer_statistics ORDER BY Model,Station;", 2000);
                if (TestStatisticsValue.status !=200) Query_statistics_Value = ComputerStatisticsValue;
                if (ComputerStatisticsValue.status != 200) Query_statistics_Value = ComputerStatisticsValue;
                foreach (var item in (List<Dictionary<string, object>>)TestStatisticsValue.data)
                {
                    await Task.Run(async () =>
                    {
                        // 查询该站别的所有测试电脑
                        var Computer = ((List<Dictionary<string, object>>)ComputerStatisticsValue.data).Where(x => x["Model"].ToString().Equals(item["Model"].ToString())).Where(y => y["Station"].ToString().Equals(item["Station"].ToString())).ToList();
                        // 计算最近三十次该站别失败最多的电脑
                        List<object> ComputerValue = await SetComputerFail(Computer);
                        // 存储单次数据
                        Dictionary<string, object> DataValue = new();
                        string DateTimeValue = DateTime.Now.ToString("HH:mm:ss");
                        // 重测次数
                        int Retest_times = Convert.ToInt32(item["Total_number_of_tests"]) - Convert.ToInt32(item["Number_of_products_tested"]);
                        // 统计测试重测率
                        double RetestRate = Math.Round((double)Retest_times / Convert.ToInt32(item["Number_of_products_tested"]) * 100, 2);
                        // 计算直通率
                        double DirectRate = Math.Round((double)Convert.ToInt32(item["Number_of_through_products"]) / Convert.ToInt32(item["Number_of_products_tested"]) * 100, 2);
                        DataValue.Add("Id", item["Id"]);
                        DataValue.Add("Model", item["Model"].ToString());
                        DataValue.Add("Station", item["Station"].ToString());
                        DataValue.Add("Total_number_of_tests", item["Total_number_of_tests"]);
                        DataValue.Add("Number_of_products_tested", item["Number_of_products_tested"]);
                        DataValue.Add("Number_of_through_products", item["Number_of_through_products"]);
                        DataValue.Add("Retest_times", Retest_times);
                        DataValue.Add("MachineName", ComputerValue[1].ToString());
                        DataValue.Add("ComputerValue", ComputerValue[0]);
                        DataValue.Add("RetestRate", RetestRate);
                        DataValue.Add("DirectRate", DirectRate);
                        valuePairs.Add(DataValue);
                    });

                }
                // 根据机型和站别重新排序
                valuePairs = valuePairs.OrderBy(x => x["Model"]).ThenBy(x => x["Station"]).ToList();
                Query_statistics_Value =ApiResponse.OK(valuePairs);
                Get_Echarts(Query_statistics_Value);
            }
            catch
            {
                Query_statistics_Value = ApiResponse.OK(valuePairs);
                //Get_Echarts(Query_statistics_Value);
            }
        }

        /// <summary>
        /// 统计电脑最近30次连续失败次数
        /// </summary>
        /// <param name="Computer">传入电脑数据</param>
        /// <returns></returns>
        public Task<List<object>> SetComputerFail(List<Dictionary<string, object>> Computer)
        {
            return Task.Run(() =>
            {
                List<object> ComputerValue = new();
                try
                {
                    foreach (var item in Computer)
                    {
                        string MachineName = item["MachineName"].ToString();
                        int ComputerResult = 0;
                        if (Convert.ToInt32(item["Number_of_tests"].ToString()) > 30)
                        {
                            string TestResult = Convert.ToString(Convert.ToUInt32(item["TestResult"].ToString()), 2);
                            var data = TestResult.ToCharArray();
                            for (int i = data.Length - 1; i > 0; i--)
                            {
                                if (data[i].ToString().Equals("0"))
                                {
                                    ComputerResult++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            ComputerResult = 0;
                        }
                        if (ComputerValue.Count == 0)
                        {
                            ComputerValue.Add(ComputerResult);
                            ComputerValue.Add(MachineName);
                        }
                        else
                        {
                            if (Convert.ToInt32(ComputerValue[0]) < ComputerResult)
                            {
                                ComputerValue[0] = ComputerResult;
                                ComputerValue[1] = MachineName;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                return ComputerValue;
            });
        }

        #region 计算图表数据并返回
        static Dictionary<string, List<Dictionary<string, object>>> EchartsDataValue = new() { };
        /// <summary>
        /// 图表数据处理方法
        /// </summary>
        /// <param name="Data">传入测试数据</param>
        public void Get_Echarts(ApiResponse Data)
        {
            try
            {
                string ModelName = ""; // 存储机型名称
                string DateTimeValue = DateTime.Now.ToString("HH:mm:ss");
                Dictionary<string, List<Dictionary<string, object>>> DataValue = new() { }; // 存储临时图表数据
                var TestValue = (List<Dictionary<string,object>>)Data.data;
                foreach (var item in TestValue)
                {
                    Dictionary<string, object> StationDataValues = new() { }; // 存储站别图表数据
                    if (EchartsDataValue.Count == 0)
                    {
                        StationDataValues.Add("Station", item["Station"].ToString());
                        StationDataValues.Add("Time", new List<string> { DateTimeValue });
                        StationDataValues.Add("DirectRate", new List<string> { item["DirectRate"].ToString() });
                        StationDataValues.Add("RetestRate", new List<string> { item["RetestRate"].ToString() });
                        StationDataValues.Add("Total_number_of_tests", new List<string> { item["Total_number_of_tests"].ToString() });
                        StationDataValues.Add("Retest_times", new List<string> { item["Retest_times"].ToString() });
                        if (!DataValue.ContainsKey(item["Model"].ToString()))
                        {
                            DataValue.Add(item["Model"].ToString(), new List<Dictionary<string, object>> { StationDataValues });
                        }
                        else
                        {
                            List<Dictionary<string, object>> value = DataValue[item["Model"].ToString()];
                            value.Add(StationDataValues);
                            DataValue[item["Model"].ToString()] = value;
                        }
                    }
                    else
                    {
                        if (!EchartsDataValue.ContainsKey(item["Model"].ToString()) && !DataValue.ContainsKey(item["Model"].ToString()))
                        {
                            List<Dictionary<string, object>> StationValue = EchartsDataValue[ModelName];
                            List<string> TimeValue = (List<string>)StationValue[0]["Time"];
                            List<string> DirectRateValue = new();
                            List<string> RetestRateValue = new();
                            List<string> Total_number_of_testsValue = new();
                            List<string> Retest_timesValue = new();

                            for (int i = 0; i < TimeValue.Count - 1; i++)
                            {
                                DirectRateValue.Add("0");
                                RetestRateValue.Add("0");
                                Total_number_of_testsValue.Add("0");
                                Retest_timesValue.Add("0");
                            }
                            DirectRateValue.Add(item["DirectRate"].ToString());
                            RetestRateValue.Add(item["RetestRate"].ToString());
                            Total_number_of_testsValue.Add(item["Total_number_of_tests"].ToString());
                            Retest_timesValue.Add(item["Retest_times"].ToString());

                            StationDataValues.Add("Station", item["Station"].ToString());
                            StationDataValues["Time"] = TimeValue;
                            StationDataValues["DirectRate"] = DirectRateValue;
                            StationDataValues["RetestRate"] = RetestRateValue;
                            StationDataValues["Total_number_of_tests"] = Total_number_of_testsValue;
                            StationDataValues["Retest_times"] = Retest_timesValue;
                            DataValue.Add(item["Model"].ToString(), new List<Dictionary<string, object>> { StationDataValues });
                        }
                        else if (!EchartsDataValue.ContainsKey(item["Model"].ToString()) && DataValue.ContainsKey(item["Model"].ToString()))
                        {
                            List<Dictionary<string, object>> StationValue = DataValue[ModelName];
                            List<string> TimeValue = (List<string>)StationValue[0]["Time"];
                            List<string> DirectRateValue = new();
                            List<string> RetestRateValue = new();
                            List<string> Total_number_of_testsValue = new();
                            List<string> Retest_timesValue = new();

                            for (int i = 0; i < TimeValue.Count - 1; i++)
                            {
                                DirectRateValue.Add("0");
                                RetestRateValue.Add("0");
                                Total_number_of_testsValue.Add("0");
                                Retest_timesValue.Add("0");
                            }
                            DirectRateValue.Add(item["DirectRate"].ToString());
                            RetestRateValue.Add(item["RetestRate"].ToString());
                            Total_number_of_testsValue.Add(item["Total_number_of_tests"].ToString());
                            Retest_timesValue.Add(item["Retest_times"].ToString());

                            StationDataValues.Add("Station", item["Station"].ToString());
                            StationDataValues["Time"] = TimeValue;
                            StationDataValues["DirectRate"] = DirectRateValue;
                            StationDataValues["RetestRate"] = RetestRateValue;
                            StationDataValues["Total_number_of_tests"] = Total_number_of_testsValue;
                            StationDataValues["Retest_times"] = Retest_timesValue;
                            if (!DataValue.ContainsKey(item["Model"].ToString()))
                            {
                                DataValue.Add(item["Model"].ToString(), new List<Dictionary<string, object>> { StationDataValues });
                            }
                            else
                            {
                                List<Dictionary<string, object>> value = DataValue[item["Model"].ToString()];
                                value.Add(StationDataValues);
                                DataValue[item["Model"].ToString()] = value;
                            }
                        }
                        else
                        {
                            ModelName = item["Model"].ToString();
                            List<Dictionary<string, object>> StationValue = EchartsDataValue[item["Model"].ToString()];
                            for (int i = 0; i < StationValue.Count; i++)
                            {
                                StationDataValues = StationValue[i];
                                if (StationDataValues["Station"].Equals(item["Station"].ToString()))
                                {
                                    List<string> TimeValue = (List<string>)StationDataValues["Time"];
                                    List<string> DirectRateValue = (List<string>)StationDataValues["DirectRate"];
                                    List<string> RetestRateValue = (List<string>)StationDataValues["RetestRate"];
                                    List<string> Total_number_of_testsValue = (List<string>)StationDataValues["Total_number_of_tests"];
                                    List<string> Retest_timesValue = (List<string>)StationDataValues["Retest_times"];
                                    if (TimeValue.Count > DirectRateValue.Count)
                                    {
                                        for (int j = 0; j < TimeValue.Count - DirectRateValue.Count; j++)
                                        {
                                            DirectRateValue.Add("0");
                                        }
                                    }
                                    if (TimeValue.Count > RetestRateValue.Count)
                                    {
                                        for (int j = 0; j < TimeValue.Count - RetestRateValue.Count; j++)
                                        {
                                            RetestRateValue.Add("0");
                                        }
                                    }
                                    if (TimeValue.Count > Total_number_of_testsValue.Count)
                                    {
                                        for (int j = 0; j < TimeValue.Count - Total_number_of_testsValue.Count; j++)
                                        {
                                            Total_number_of_testsValue.Add("0");
                                        }
                                    }
                                    if (TimeValue.Count > Retest_timesValue.Count)
                                    {
                                        for (int j = 0; j < TimeValue.Count - Retest_timesValue.Count; j++)
                                        {
                                            Retest_timesValue.Add("0");
                                        }
                                    }
                                    if (TimeValue.Count == 20)
                                    {
                                        TimeValue.RemoveAt(0);
                                        DirectRateValue.RemoveAt(0);
                                        RetestRateValue.RemoveAt(0);
                                        Total_number_of_testsValue.RemoveAt(0);
                                        Retest_timesValue.RemoveAt(0);
                                    }

                                    TimeValue.Add(DateTimeValue);
                                    DirectRateValue.Add(item["DirectRate"].ToString());
                                    RetestRateValue.Add(item["RetestRate"].ToString());
                                    Total_number_of_testsValue.Add(item["Total_number_of_tests"].ToString());
                                    Retest_timesValue.Add(item["Retest_times"].ToString());

                                    StationDataValues["Time"] = TimeValue;
                                    StationDataValues["DirectRate"] = DirectRateValue;
                                    StationDataValues["RetestRate"] = RetestRateValue;
                                    StationDataValues["Total_number_of_tests"] = Total_number_of_testsValue;
                                    StationDataValues["Retest_times"] = Retest_timesValue;
                                    if (!DataValue.ContainsKey(item["Model"].ToString()))
                                    {
                                        DataValue.Add(item["Model"].ToString(), new List<Dictionary<string, object>> { StationDataValues });
                                    }
                                    else
                                    {
                                        List<Dictionary<string, object>> value = DataValue[item["Model"].ToString()];
                                        value.Add(StationDataValues);
                                        DataValue[item["Model"].ToString()] = value;
                                    }
                                }
                            }
                        }
                    }

                }
                EchartsDataValue = DataValue;
            }
            catch
            {
                EchartsDataValue.Clear();
                Query_statistics_Value =ApiResponse.OK("");
            }
        }

        /// <summary>
        /// 对外获取图表数据接口
        /// </summary>
        /// <returns></returns>
        public string Get_Echarts()
        {
            return "";
        }

        #endregion
    }
}
