using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;

namespace Data_Query_API.DataProcessing.DataTableCreationData
{
    /// <summary>
    /// 数据表创建类
    /// </summary>
    public class DataTableCreation
    {
        #region 创建日报数据表
        /// <summary>
        /// 创建日报数据表
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static async Task<bool> CreateTable(string TableName)
        {
            try
            {
                Mysql conn = new(GetConfiguration.testDailyMysql);
                var TestDataQuery = await conn.Query($"SELECT * FROM information_schema.TABLES where table_name='{TableName}' and TABLE_SCHEMA ='test_1';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    /*
                    * Id 主键
                    * Model 机型名
                    * Station 站别名
                    * Time 时间
                    * Total_number_of_tests 测试总次数
                    * Test_success_times 测试成功总次数
                    * Number_of_test_failures  测试失败总次数
                    * Total_number_of_products_tested 测试产品总数
                    * Total_number_of_retests  重测总次数
                    * Total_times_of_standard_test 标准品测试总次数
                    * Total_number_of_maintenance_product_tests 维修品测试总次数
                    * Retest_rate  重测率
                    * Test_pass_through_rate  直通率
                    * Node_data_statistics 节点测试统计数据
                    * Test_computer_data_statistics  测试电脑数据统计
                    * Test_failed_items 测试失败项统计
                    */
                    string sql = $"create table {TableName} " +
                    "(Id int primary key auto_increment," +
                    "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '机型'," +
                    "Station char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别'," +
                    "Time int(12) not null COMMENT '数据日期'," +
                    "Total_number_of_tests int(5) not null COMMENT '测试总次数'," +
                    "Test_success_times int(5) not null COMMENT '测试成功总次数'," +
                    "Number_of_test_failures int(5) not null COMMENT '测试失败总次数'," +
                    "Total_number_of_products_tested int(5) not null COMMENT '测试产品总数'," +
                    "Total_number_of_retests int(5) not null COMMENT '重测总次数'," +
                    "Total_times_of_standard_test int(5) not null COMMENT '标准品测试总次数'," +
                    "Total_number_of_maintenance_product_tests int(5) not null COMMENT '维修品测试总次数'," +
                    "Retest_rate double not null COMMENT '重测率'," +
                    "Test_pass_through_rate double not null COMMENT '直通率'," +
                    "Node_data_statistics varchar(2000) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '节点测试数据统计'," +
                    "Test_computer_data_statistics varchar(4000) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试电脑数据统计'," +
                    "Test_failed_items varchar(2000) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试失败项统计');" +
                    $"create trigger {TableName}_backups " +
                    $"BEFORE INSERT on {TableName} for each row Begin " +
                    $"INSERT INTO te_test.{TableName}_backups(Model, Station, Time, Total_number_of_tests, Test_success_times," +
                    "Number_of_test_failures, Total_number_of_products_tested, Total_number_of_retests," +
                    "Total_times_of_standard_test, Total_number_of_maintenance_product_tests, Retest_rate, Test_pass_through_rate," +
                    "Node_data_statistics, Test_computer_data_statistics, Test_failed_items)VALUES(new.Model, new.Station, new.Time, new.Total_number_of_tests," +
                    "new.Test_success_times, new.Number_of_test_failures, new.Total_number_of_products_tested, new.Total_number_of_retests, new.Total_times_of_standard_test," +
                    "new.Total_number_of_maintenance_product_tests, new.Retest_rate, new.Test_pass_through_rate, new.Node_data_statistics, new.Test_computer_data_statistics," +
                    "new.Test_failed_items);END";
                    ApiResponse DataQuery = await conn.commonExecute(sql);
                    if (DataQuery.status != 200) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建日报数据表(CreateTable)异常,异常信息{ex.Message}", "CreateTableLog");
                return false;
            }
        }
        /// <summary>
        /// 创建备份日报数据表
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static async Task<bool> CreateTable_backups(string TableName)
        {
            try
            {
                Mysql conn = new(GetConfiguration.te_testMysql);
                var TestDataQuery = await conn.Query($"SELECT * FROM information_schema.TABLES where table_name='{TableName}_backups' and TABLE_SCHEMA ='te_test';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    // Id 主键
                    // Model 机型名
                    // Station 站别名
                    // Time 时间
                    // Total_number_of_tests 测试总次数
                    // Test_success_times 测试成功总次数
                    // Number_of_test_failures  测试失败总次数
                    // Total_number_of_products_tested 测试产品总数
                    // Total_number_of_retests  重测总次数
                    // Total_times_of_standard_test 标准品测试总次数
                    // Total_number_of_maintenance_product_tests 维修品测试总次数
                    // Retest_rate  重测率
                    // Test_pass_through_rate  直通率
                    // Node_data_statistics 节点测试统计数据
                    // Test_computer_data_statistics  测试电脑数据统计
                    // Test_failed_items 测试失败项统计
                    string sql = $"create table {TableName}_backups " +
                    "(Id int primary key auto_increment," +
                    "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                    "Station char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                    "Time int(12) not null," +
                    "Total_number_of_tests int(5) not null," +
                    "Test_success_times int(5) not null," +
                    "Number_of_test_failures int(5) not null," +
                    "Total_number_of_products_tested int(5) not null," +
                    "Total_number_of_retests int(5) not null," +
                    "Total_times_of_standard_test int(5) not null," +
                    "Total_number_of_maintenance_product_tests int(5) not null," +
                    "Retest_rate double not null," +
                    "Test_pass_through_rate double not null," +
                    "Node_data_statistics varchar(2000) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                    "Test_computer_data_statistics varchar(4000) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                    "Test_failed_items varchar(2000) CHARACTER SET utf8 COLLATE utf8_general_ci not null);";
                    ApiResponse DataQuery = await conn.commonExecute(sql);
                    if (DataQuery.status != 200) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建备份日报数据表(CreateTable_backups)异常,异常信息{ex.Message}", "CreateTableLog");
                return false;
            }
        }
        #endregion

        #region 创建测试成功数据表
        /// <summary>
        /// 创建测试成功数据表
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateTestSuccessTable()
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = 'Test_success_table' and TABLE_SCHEMA = 'test_1';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    /*
                  * ID 主键
                  * SN SN
                  * Model 机型
                  * Station 站别
                  * MachineName 电脑编号
                  */
                    string sql = "create table Test_success_table(" +
                        "Id int primary key auto_increment," +
                        "SN char(80) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT 'SN'," +
                        "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '机型'," +
                        "Station char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别'," +
                        "MachineName char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '电脑编号');" +
                        "create trigger Test_success BEFORE " +
                        "INSERT " +
                        "on Test_success_table " +
                        "for each row " +
                        "Begin " +
                        // 判断数据表是否存在该站别
                        "IF((SELECT COUNT(Station) FROM Test_statistics WHERE Model = new.Model and Station = new.Station) = 0) THEN " +
                        "INSERT INTO Test_statistics(Model, Station, Total_number_of_tests, Number_of_products_tested, Number_of_through_products)VALUES(new.Model, new.Station, 0, 0, 0);" +
                        "END IF;" +
                        // 判断测试电脑表是否存在该电脑
                        "IF((SELECT COUNT(MachineName) FROM Computer_statistics WHERE Model = new.Model and Station = new.Station and MachineName = new.MachineName) = 0)THEN " +
                        "INSERT INTO Computer_statistics(Model, Station, MachineName, TestResult, Number_of_tests)VALUES(new.Model, new.Station, new.MachineName, 0, 0);" +
                        "update Computer_statistics set TestResult = ((TestResult << 1) + 1) & 1073741823, Number_of_tests = (Number_of_tests + 1) where Model = new.Model and Station = new.Station and MachineName = new.MachineName;" +
                        "ELSE " +
                        "update Computer_statistics set TestResult = ((TestResult << 1) + 1) & 1073741823,Number_of_tests = (Number_of_tests + 1) where Model = new.Model and Station = new.Station and MachineName = new.MachineName;" +
                        "END IF;" +
                        // SN不存在NG表并且不存在OK表
                        "IF((select count(SN) from Test_failure_table where Model = new.Model and Station = new.Station and SN = new.SN) = 0 " +
                        "and (select count(SN) from Test_success_table where Model = new.Model and Station = new.Station and SN = new.SN) = 0) THEN " +
                        // 修改测试产品数"
                        "update Test_statistics set Number_of_products_tested = (Number_of_products_tested + 1) where Model = new.Model and Station = new.Station;" +
                        // 修改未失败产品数
                        "update Test_statistics set Number_of_through_products = (Number_of_through_products + 1) where Model = new.Model and Station = new.Station;" +
                        "END IF;" +
                        // 修改测试次数 
                        "update Test_statistics set Total_number_of_tests = (Total_number_of_tests + 1) where Model = new.Model and Station = new.Station;" +
                        "END";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建测试成功数据表(CreateTestSuccessTable)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }

        }
        #endregion

        #region 创建测试失败数据表
        /// <summary>
        /// 创建测试失败数据表
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateTestFailureTable()
        {
            try
            {
                Mysql context = new(GetConfiguration.testDailyMysql);
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = 'Test_failure_table' and TABLE_SCHEMA = 'test_1';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    /*
                     * ID 主键
                     * SN SN
                     * Model 机型
                     * Station 站别
                     * MachineName 电脑编号
                     */
                    string sql = "create table Test_failure_table(" +
                        "Id int primary key auto_increment," +
                        "SN char(80) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT 'SN'," +
                        "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '机型'," +
                        "Station char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别'," +
                        "MachineName char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '电脑编号');" +
                        "create trigger Test_failure BEFORE " +
                        "INSERT " +
                        "on Test_failure_table " +
                        "for each row " +
                        "Begin " +
                        // 判断是否存在该站别
                        "IF((SELECT COUNT(Station) FROM Test_statistics WHERE Model = new.Model and Station = new.Station) = 0) THEN " +
                        "INSERT INTO Test_statistics(Model, Station, Total_number_of_tests, Number_of_products_tested, Number_of_through_products)VALUES(new.Model, new.Station, 0, 0, 0);" +
                        "END IF;" +
                        // 判断测试电脑表是否存在该电脑
                        "IF((SELECT COUNT(MachineName) FROM Computer_statistics WHERE Model = new.Model and Station = new.Station and MachineName = new.MachineName) = 0)THEN " +
                        "INSERT INTO Computer_statistics(Model, Station, MachineName, TestResult, Number_of_tests)VALUES(new.Model, new.Station, new.MachineName, 0, 0);" +
                        "update Computer_statistics set TestResult = (TestResult << 1) & 1073741823, Number_of_tests = (Number_of_tests + 1) where Model = new.Model and Station = new.Station and MachineName = new.MachineName;" +
                        "ELSE " +
                        "update Computer_statistics set TestResult = (TestResult << 1) & 1073741823,Number_of_tests = (Number_of_tests + 1) where Model = new.Model and Station = new.Station and MachineName = new.MachineName;" +
                        "END IF;" +
                        // SN存在于OK表但不存在NG表
                        "IF((select count(SN) from Test_success_table where Model = new.Model and Station = new.Station and SN = new.SN) > 0" +
                        " and (select count(SN) from Test_failure_table where Model = new.Model and Station = new.Station and SN = new.SN) = 0) THEN " +
                        // 修改直通产品数
                        "update Test_statistics set Number_of_through_products = (Number_of_through_products - 1) where Model = new.Model and Station = new.Station;" +
                        // SN不存在OK表并且不存在NG表
                        "ELSEIF((select count(SN) from Test_success_table where Model = new.Model and Station = new.Station and SN = new.SN) = 0 " +
                        "and (select count(SN) from Test_failure_table where Model = new.Model and Station = new.Station and SN = new.SN) = 0) THEN " +
                        // 修改测试产品数
                        "update Test_statistics set Number_of_products_tested = (Number_of_products_tested + 1) where Model = new.Model and Station = new.Station;" +
                        "END IF;" +
                        // 修改测试次数
                        "update Test_statistics set Total_number_of_tests = (Total_number_of_tests + 1) where Model = new.Model and Station = new.Station;" +
                        "END";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建测试失败数据表(CreateTestFailureTable)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }

        }
        #endregion

        #region 创建测试数据统计表
        /// <summary>
        /// 创建测试数据统计表
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateTestStatistics()
        {
            try
            {
                /* 
                 * id 主键
                 * Model 机型名称
                 * Station 站别名称
                 * Total_number_of_tests 测试总次数
                 * Number_of_products_tested 测试产品数
                 * Number_of_through_products 直通产品数
                */
                Mysql context = new(GetConfiguration.testDailyMysql);
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = 'Test_statistics' and TABLE_SCHEMA = 'test_1';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    string sql = "create table Test_statistics(" +
                        "Id int primary key auto_increment," +
                        "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '机型'," +
                        "Station char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别'," +
                        "Total_number_of_tests int(5) not null COMMENT '测试总次数'," +
                        "Number_of_products_tested int(5) not null COMMENT '测试产品数'," +
                        "Number_of_through_products int(5) not null COMMENT '直通产品数')";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建测试数据统计表(CreateTestStatistics)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 创建电脑测试数据统计表
        /// <summary>
        /// 创建电脑测试数据统计表
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateComputerStatistics()
        {
            try
            {
                /* 
                 * id 主键
                 * Model 机型名称
                 * Station 站别名称
                 * MachineName 电脑编号
                 * TestResult 电脑最近30次结果记录
                 * Number_of_tests 测试总次数
                */
                Mysql context = new(GetConfiguration.testDailyMysql);
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = 'Computer_statistics' and TABLE_SCHEMA = 'test_1';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    string sql = "create table Computer_statistics(" +
                        "Id int primary key auto_increment," +
                        "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '机型'," +
                        "Station char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别'," +
                        "MachineName char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '电脑编号'," +
                        "TestResult bigint(60) not null COMMENT '电脑最近30次测试结果记录'," +
                        "Number_of_tests int(5) not null COMMENT '电脑测试总次数')";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建电脑测试数据统计表(CreateComputerStatistics)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 创建TestLog表并创建触发器
        /// <summary>
        /// 创建TestLog表并创建触发器
        /// </summary>
        /// <param name="ModelValue">机型名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateTestLogTable(string ModelValue)
        {
            try
            {
                Mysql context = new(GetConfiguration.testLogMysql);
                string DateTimeValue = DateTime.Now.ToString("yy-MM-dd");
                string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTimeValue.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
                string TableName = $"{ModelValue.Replace('-', '_')}_{DateTimeValue.Split('-')[0]}_{Quarter}".ToLower();
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = '{TableName}' and TABLE_SCHEMA = 'testlog';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    /* 
                     * id 主键
                     * SN SN码
                     * Time 数据上传时间
                     * Result 测试结果
                     * Station 站别名称
                     * workorders 工单
                     * MachineName 电脑编号
                     * TestTime 测试时长
                     * WireNumber 线材使用次数
                     * Testlog 测试数据
                    */
                    string sql = $"create table {TableName} " +
                   "(Id int primary key auto_increment," +
                   "SN char(80) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                   "Time TIMESTAMP not NULL DEFAULT  CURRENT_TIMESTAMP COMMENT '测试数据上传时间'," +
                   "Result char(20) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试结果'," +
                   "Station char(100) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别'," +
                   "workorders char(20) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '工单号'," +
                   "MachineName char(50) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '电脑编号'," +
                   "TestTime char(20) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试时间', " +
                   "WireNumber char(10) CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '线材使用次数', " +
                   "Testlog varchar(10000) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试数据'," +
                   "INDEX(Station));" +
                   $"create trigger {TableName}_Test_day_data BEFORE INSERT on {TableName} for each row Begin " +
                   "if (new.Result = 'True') THEN " +
                   $"INSERT INTO test_1.Test_success_table(SN, Model, Station, MachineName)VALUES(new.SN, '{ModelValue}', new.Station, new.MachineName);" +
                   $"ELSE INSERT INTO test_1.Test_failure_table(SN, Model, Station, MachineName)VALUES(new.SN, '{ModelValue}', new.Station, new.MachineName);" +
                   "END IF;" +
                   "END";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建TestLog表并创建触发器(CreateTestLogTable)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 创建Logi Test Log表
        /// <summary>
        /// 创建Logi Test Log表
        /// </summary>
        /// <param name="ModelValue">机型名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateLogiTestLogTable(string ModelValue)
        {
            try
            {
                Mysql context = new(GetConfiguration.LGTestLogMysql);
                string DateTimeValue = DateTime.Now.ToString("yy-MM-dd");
                string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTimeValue.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
                string TableName = $"{ModelValue.Replace('-', '_')}_{DateTimeValue.Split('-')[0]}_{Quarter}";
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = '{TableName}' and TABLE_SCHEMA = 'LogiTestLog';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    /* 
                     * id 主键
                     * Status 测试结果
                     * Comment 测试失败项和编号
                     * Test_Start_Time 测试开始时间
                     * Test_Duration 测试时长
                     * Failed_Tests 
                     * BU 
                     * Project
                     * Station 站别名称
                     * Stage 状态
                     * MAC_Addr MAC地址
                     * IP_Addr IP地址
                     * oemSource 产商
                     * DLLName 机型DLL名称
                     * DLLTime DLL生成时间
                     * MesFlag MES状态
                     * MesName MES名称
                     * workorders 工单
                     * MachineName 电脑编号
                     * Testlog 测试数据
                    */
                    string sql = $"create table {TableName} " +
                   "(Id int primary key auto_increment," +
                   "Status char(5) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试结果'," +
                   "Comment char(200) CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '测试失败项和编号'," +
                   "Test_Start_Time int(12) not null COMMENT '数据上传时间'," +
                   "Test_Duration char(10) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试时长'," +
                   "Failed_Tests char(100) CHARACTER SET utf8 COLLATE utf8_general_ci," +
                   "BU char(100) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                   "Project varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci not null," +
                   "Station varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别名称'," +
                   "Stage char(10) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '状态'," +
                   "MAC_Addr char(20) CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT 'MAC地址'," +
                   "IP_Addr char(20) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT 'IP地址'," +
                   "oemSource char(20) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '产商'," +
                   "DLLName varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT 'DLL名称'," +
                   "DLLTime int(12) not null COMMENT 'DLL生成时间'," +
                   "MesFlag char(5) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT 'MES状态'," +
                   "MesName char(20) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT 'MES名称'," +
                   "workorders char(15) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '工单号'," +
                   "MachineName char(50) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '电脑编号'," +
                   "Testlog varchar(10000) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '测试数据'," +
                   "INDEX(Station));";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建Logi Test Log表(CreateLogiTestLogTable)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 创建Logi声学数据上传机型站别表
        /// <summary>
        /// 创建Logi声学数据上传机型站别表
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> CreateLogiAcousticData()
        {
            try
            {
                /* 
                 * id 主键
                 * Model 机型名称
                 * Station 站别名称
                 * MesFlag MES状态
                */
                Mysql context = new(GetConfiguration.testDailyMysql);
                var TestDataQuery = await context.Query($"SELECT * FROM information_schema.TABLES where table_name = 'Logi_Acoustic_Data' and TABLE_SCHEMA = 'LogiTestLog';");
                if (TestDataQuery.Rows.Count == 0)
                {
                    string sql = "create table Logi_Acoustic_Data(" +
                        "Id int primary key auto_increment," +
                        "Model char(30) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '机型'," +
                        "Station varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci not null COMMENT '站别')";
                    return await context.commonExecute(sql);
                }
                return ApiResponse.OK("");
            }
            catch (Exception ex)
            {
                Log.LogWrite($"创建Logi声学数据上传机型站别表(CreateLogiAcousticData)异常,异常信息{ex.Message}", "CreateTableLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 触发器区域
        #region 创建所有log表数据分析触发器
        /// <summary>
        /// 创建所有log表数据分析触发器
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GenerateTestDdataTrigger()
        {
            Mysql context = new(GetConfiguration.testLogMysql);
            ApiResponse result = await context.DataQuery("select table_name from information_schema.tables where table_schema='testlog' ORDER BY table_name", 3000);
            if (result.status != 200) return result;
            foreach (var item in (List<Dictionary<string, object>>)result.data)
            {
                string modelValue = "";
                for (int i = 0; i < item["table_name"].ToString().Split('_').Length - 2; i++)
                {
                    modelValue += item["table_name"].ToString().Split('_')[i] + "-";
                }
                string sql = @$"create trigger {item["table_name"]}_Test_day_data BEFORE INSERT on {item["table_name"]} for each row Begin " +
                   "if (new.Result = 'True') THEN " +
                   $"INSERT INTO test_1.Test_success_table(SN, Model, Station, MachineName)VALUES(new.SN, '{modelValue.TrimEnd('-')}', new.Station, new.MachineName);" +
                   $"ELSE INSERT INTO test_1.Test_failure_table(SN, Model, Station, MachineName)VALUES(new.SN, '{modelValue.TrimEnd('-')}', new.Station, new.MachineName);" +
                   "END IF;" +
                   "END";
                var Value = await context.commonExecute(sql);
            }
            return ApiResponse.OK("创建所有log表数据分析触发器完成");
        }
        #endregion

        #region 删除所有log表数据分析触发器
        /// <summary>
        /// 删除所有log表数据分析触发器
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> DeleteTestDdataTrigger()
        {
            Mysql context = new(GetConfiguration.testLogMysql);
            ApiResponse result = await context.DataQuery("select table_name from information_schema.tables where table_schema='testlog' ORDER BY table_name", 3000);
            if (result.status != 200) return result;
            foreach (var item in (List<Dictionary<string, object>>)result.data)
            {
                var Value = await context.commonExecute($"DROP TRIGGER {item["table_name"]}_Test_day_data;");
            }
            return ApiResponse.OK("删除所有log表数据分析触发器完成");
        }
        #endregion

        //#region 创建所有log表电脑管理触发器
        ///// <summary>
        ///// 创建所有log表电脑管理触发器
        ///// </summary>
        ///// <returns></returns>
        //public static async Task<string> GenerateComputerManagementTrigger(string Model)
        //{
        //    Mysql context = new(GetConfiguration.testLogMysql);
        //    if (Model.Length > 0)
        //    {
        //        string Year = DateTime.Now.Year.ToString(); // 获取当前年份
        //        string DateTimeValue = DateTime.Now.ToString("yyyy-MM-dd"); // 获取当前年月日
        //        string Quarter = Math.Ceiling(Math.Round((double)(Convert.ToDouble(DateTimeValue.Split('-')[1])) / 3, 4)).ToString(); // 计算当前季度
        //        Model+= "_" + Year[^2..] + "_" + Quarter;
        //        string sql = @$"create trigger {Model}_Computer_management BEFORE INSERT on {Model} for each row Begin " +
        //              $"test_1.Computer_management_table C set Test_Duration = (C.Test_Duration + new.TestTime) " +
        //              $"WHERE C.MachineName = new.MachineName AND C.Station = new.Station; END";
        //        await context.commonExecute(sql);
        //    }
        //    else
        //    {
        //        string result = await context.DataQueryToJson("select table_name from information_schema.tables where table_schema='testlog' ORDER BY table_name", 3000);
        //        if (result.Equals("]") || result.Equals("404")) return "404";
        //        foreach (var item in JArray.Parse(result))
        //        {
        //            string modelValue = "";
        //            for (int i = 0; i < item["table_name"].ToString().Split('_').Length - 2; i++)
        //            {
        //                modelValue += item["table_name"].ToString().Split('_')[i] + "-";
        //            }
        //            string sql = @$"create trigger {item["table_name"]}_Computer_management BEFORE INSERT on {item["table_name"]} for each row Begin " +
        //               $"test_1.Computer_management_table C set Test_Duration = (C.Test_Duration + new.TestTime) " +
        //               $"WHERE C.MachineName = new.MachineName AND C.Station = new.Station; END";
        //            await context.commonExecute(sql);
        //        }
        //    }

        //    return "200";
        //}
        //#endregion

        //#region 删除所有log表电脑管理触发器
        ///// <summary>
        ///// 删除所有log表触发器
        ///// </summary>
        ///// <returns></returns>
        //public static async Task<string> DeleteComputerManagementTrigger(string Model)
        //{
        //    Mysql context = new(GetConfiguration.testLogMysql);

        //        string result = await context.DataQueryToJson("select table_name from information_schema.tables where table_schema='testlog' ORDER BY table_name", 3000);
        //        if (result.Equals("]") || result.Equals("404")) return "404";
        //        foreach (var item in JArray.Parse(result))
        //        {
        //            await context.commonExecute($"DROP TRIGGER {item["table_name"]}_Computer_management;");
        //        }


        //    return "200";
        //}
        //#endregion

        #endregion
    }
}
