using Data_Query_API.SaveData;
using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Data_Query_API.DataProcessing.DataTableCreationData;
using Data_Query_API.DataProcessing.BackgroundData;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using MySqlX.XDevAPI.Common;
using static System.Collections.Specialized.BitVector32;

namespace Data_Query_API.DataProcessing.DataQuery
{
    /// <summary>
    /// 测试数据查询类
    /// </summary>
    public class TestData
    {
        #region 获取测试数据数据库所有表名
        /// <summary>
        /// 获取测试数据数据库所有表名
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDataTableName()
        {
            try
            {
                Mysql context = new(GetConfiguration.testLogMysql);
                ApiResponse result = await context.DataQuery("select table_name from information_schema.tables where table_schema='testlog' ORDER BY table_name", 3000);
                if (result.status != 200) return result;
                List<Dictionary<string, object>> data = new();
                foreach (Dictionary<string, object> item in (List<Dictionary<string, object>>)result.data) {
                    data.Add(item.ToDictionary(k => k.Key.ToLower(), k => k.Value));
                }
                return ApiResponse.OK(data);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取测试数据数据库所有表名(GetDataTableName)异常,异常信息{ex.Message}", "TestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 获取指定测试数据表中所有站别
        /// <summary>
        /// 获取指定测试数据表中所有站别
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDataTableStation(string TableName)
        {
            try
            {
                Mysql context = new(GetConfiguration.testLogMysql);
                return await context.DataQuery($"SELECT DISTINCT Station FROM {TableName} ORDER BY Station", 5000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取指定测试数据表中所有站别(GetDataTableStation)异常,异常信息{ex.Message}", "TestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 获取指定测试数据表中指定站别所有测试电脑编号
        /// <summary>
        /// 获取指定测试数据表中指定站别所有测试电脑编号
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <param name="Station">传入站别名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetComputerName(string TableName, string Station)
        {
            try
            {
                Mysql context = new(GetConfiguration.testLogMysql);
                return await context.DataQuery($"SELECT DISTINCT MachineName FROM {TableName} where Station = '{Station}' ORDER BY MachineName;", 2000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取指定测试数据表中指定站别所有测试电脑名称(GetComputerName)异常,异常信息{ex.Message}", "TestDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 获取测试数据表中指定站别所有工单信息
        /// <summary>
        /// 获取测试数据表中指定站别所有工单信息
        /// </summary>
        /// <param name="TableName">传入数据表表名</param>
        /// <param name="Station">传入站别名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetDataTableWorkOrder(string TableName, string Station)
        {
                try
                {
                    Mysql context = new(GetConfiguration.testLogMysql);
                    return await context.DataQuery($"SELECT DISTINCT workorders from {TableName} WHERE Station = '{Station}' ORDER BY workorders", 2000);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"获取测试数据表中指定站别所有工单信息(GetDataTableWorkOrder)异常,异常信息{ex.Message}", "TestDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
        }
        #endregion

        #region 获取指定条件测试数据
        /// <summary>
        /// 获取指定条件测试数据
        /// </summary>
        /// <param name="TableName">传入表名</param>
        /// <param name="Station">站别名称</param>
        /// <param name="MachineName">电脑编号</param>
        /// <param name="Result">测试结果</param>
        /// <param name="DuplicateRemoval">是否去重</param>
        /// <param name="Workorders">工单号</param>
        /// <param name="SN">SN 号</param>
        /// <param name="IncludeValue">测试数据包含内容</param>
        /// <param name="StartTime">查询开始时间</param>
        /// <param name="EndTime">查询结束时间</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetTestData(string TableName, string Station, string MachineName, string Result, string DuplicateRemoval,
            string Workorders, string SN, string IncludeValue, string StartTime, string EndTime)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string QuerySql = $"select SN,Time,Result,Station,workorders,MachineName,TestTime,WireNumber,Testlog from {TableName} WHERE Station = '{Station}'";
                    QuerySql += MachineName.Equals("null") ? "" : $" AND MachineName = '{MachineName}'";
                    QuerySql += Result.Equals("null") ? "" : $" AND Result = '{Result}'";
                    QuerySql += Workorders.Equals("null") ? "" : $" AND Workorders = '{Workorders}'";
                    QuerySql += SN.Equals("null") ? "" : $" AND SN = '{SN}'";
                    QuerySql += IncludeValue.Equals("null") ? "" : $" AND Testlog LIKE '%{IncludeValue}%'";
                    QuerySql += StartTime.Equals("null") ? "" : $" AND Time BETWEEN '{StartTime}' AND '{EndTime}'";
                    QuerySql += DuplicateRemoval.Equals("False") ? "" : $" group by SN";
                    QuerySql += " ORDER BY Time DESC";
                    Mysql context = new(GetConfiguration.testLogMysql);
                    return await context.DataQuery(QuerySql, 100000);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"获取指定条件测试数据(GetTestData)异常,异常信息{ex.Message}", "TestDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
            });
        }
        #endregion

        #region 自定义查询测试数据
        /// <summary>
        /// 自定义查询测试数据
        /// </summary>
        /// <param name="Sql">传入自定义查询语句</param>
        /// <returns></returns>
        public static async Task<ApiResponse> CustomQuery(string Sql)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string QuerySql = $"select SN,Time,Result,Station,workorders,MachineName,TestTime,WireNumber,Testlog from {Sql} ORDER BY Time DESC";
                    Mysql context = new(GetConfiguration.testLogMysql);
                    return await context.DataQuery(QuerySql, 100000);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"自定义查询测试数据(CustomQuery)异常,异常信息{ex.Message}", "TestDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
            });
        }
        #endregion

        #region 保存页面测试数据和数据分析
        /// <summary>
        /// 保存页面测试数据
        /// </summary>
        /// <param name="saveTestDataValue">传入对应测试数据 </param>
        /// <returns></returns>
        public static string SaveData(SaveTestDataValue saveTestDataValue)
        {
            try
            {
                string station = saveTestDataValue.TestData[0]["Station"].ToString();
                Dictionary<string, Dictionary<string, string>> computer_data_statistics = SaveExcel.Test_computer_data_statistics
                    (saveTestDataValue.TestData, "Result", "SN", out string ComputerStatisticalResults);
                string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data\{saveTestDataValue.Model}-{station}.xlsx";
                if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data");
                FileInfo newFile = new(Path);
                if (newFile.Exists)
                {
                    newFile.Delete();
                    newFile = new FileInfo(Path);
                }
                Dictionary<string, int> FailStatistics = new();
                foreach (var item in saveTestDataValue.TestFailStatistics)
                {
                    FailStatistics.Add(item["name"].ToString(), Convert.ToInt32(item["value"].ToString()));
                }
                // 保存测试数据统计
                if (!SaveExcel.SaveDataAnalysis(Path, "测试数据统计分析页", saveTestDataValue.TestDataStatistics[0], null,
                   computer_data_statistics, FailStatistics, false)) return null;
                // 保存测试数据
                if (!SaveExcel.SaveTestData(Path, saveTestDataValue.TestData, saveTestDataValue.TestDataTitle, saveTestDataValue.TestDataLimit)) return null;
                return Path;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"保存页面测试数据和数据分析(SaveData)异常,异常信息{ex.Message}", "TestDataLog");
                return null;
            }
        }
        #endregion

        #region 保存页面模糊查询测试数据
        /// <summary>
        /// 保存页面测试数据
        /// </summary>
        /// <param name="saveTestDataValue">传入对应测试数据</param>
        /// <returns></returns>
        public static string SaveFuzzyQueryTestData(SaveTestDataValue saveTestDataValue)
        {
            try
            {
                string station = saveTestDataValue.TestData[0]["Station"].ToString();
                Dictionary<string, Dictionary<string, string>> computer_data_statistics = SaveExcel.Test_computer_data_statistics
                    (saveTestDataValue.TestData, "Result", "SN", out string ComputerStatisticalResults);
                string Path = $@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data\{saveTestDataValue.Model}-{station}.xlsx";
                if (!Directory.Exists($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data")) Directory.CreateDirectory($@"{GetConfiguration.FileStorageLocation}\DataQuery\TestData\Save_Data");
                FileInfo newFile = new(Path);
                if (newFile.Exists)
                {
                    newFile.Delete();
                    newFile = new FileInfo(Path);
                }
                // 保存测试数据
                if (!SaveExcel.SaveTestData(Path, saveTestDataValue.TestData, saveTestDataValue.TestDataTitle, saveTestDataValue.TestDataLimit)) return null;
                return Path;
            }
            catch(Exception ex)
            {
                Log.LogWrite($"保存页面模糊查询测试数据(SaveFuzzyQueryTestData)异常,异常信息{ex.Message}", "TestDataLog");
                return null;
            }
        }
        #endregion

        #region DyTest Performency Data
        /// <summary>
        /// Get All Model's Performency
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetPerformencyData(string model)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    string QuerySql = $"Select computer,type_mode,test_times,pass_times,performency from Network where status1 = 'connected'";
                    QuerySql += model.Equals("null") ? "" : $" AND model = '{model}'";
                   // QuerySql += " ORDER BY Time DESC";
                    Mysql context = new(GetConfiguration.DyTestPerformencySql);
                    return await context.DataQuery(QuerySql, 100000);
                }
                catch (Exception ex)
                {
                    Log.LogWrite($"获取指定条件测试数据(GetTestData)异常,异常信息{ex.Message}", "TestDataLog");
                    return ApiResponse.Error(500, ex.Message.ToString());
                }
            });
        }
        #endregion
    }
}
