using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;

namespace Data_Query_API.DataProcessing.BackgroundData
{
    /// <summary>
    /// 后台获取数据类
    /// </summary>
    public class Background
    {
        #region 获取后台所有有上传测试数据的机型和站别
        /// <summary>
        /// 获取后台所有有上传测试数据的机型和站别
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetUploadDataModelStation()
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse result = await context.DataQuery($"SELECT id,name FROM model", 1000);
                if (result.status != 200) return result;
                Dictionary<int, string> keyValuePairs = new() { };
                int count = 0;
                foreach (var model in (List<Dictionary<string, object>>)result.data)
                {
                    ApiResponse StationData = await context.DataQuery($"select name,config from station WHERE model_id ={model["id"]}", 1000);
                    if (StationData.status != 200 || StationData.data.ToString().Length == 0) continue;
                    foreach (var station in (List<Dictionary<string, object>>)StationData.data)
                    {
                        if (station["config"].ToString().Contains("TestLogUploadMySQL=1"))
                        {
                            keyValuePairs.Add(count, model["name"].ToString().ToLower() + "|" + station["name"].ToString());
                            count++;
                        }
                    }
                }
                return ApiResponse.OK(keyValuePairs);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取所有有上传测试数据的机型和站别(GetUploadDataModel)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 获取后台所有有上传LG测试数据的机型和站别
        /// <summary>
        /// 获取后台所有有上传LG测试数据的机型和站别
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResponse> GetUploadLGDataModelStation()
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse resut = await context.DataQuery($"SELECT id,name FROM model", 1000);
                Dictionary<int, string> keyValuePairs = new() { };
                int count = 0;
                foreach (var model in (List<Dictionary<string, object>>)resut.data)
                {
                    ApiResponse StationData = await context.DataQuery($"select name,config from station WHERE model_id ={model["id"]}", 1000);
                    if (StationData.status != 200 || StationData.data.ToString().Length == 0) continue;
                    foreach (var station in (List<Dictionary<string, object>>)StationData.data)
                    {
                        if (station["config"].ToString().Contains("LogiLogFlag=1"))
                        {
                            var config = station["config"].ToString().Split("\n");
                            var stationLine = config.FirstOrDefault(line => line.Contains("LogiStation"));
                            keyValuePairs.Add(count, model["name"].ToString() + "|" + stationLine.Split('=')[1].Replace('\r', ' ').Trim().ToUpper() + "|" + station["name"].ToString());
                            count++;
                        }
                    }
                }
                return ApiResponse.OK(keyValuePairs);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"获取后台所有有上传LG测试数据的机型和站别(GetUploadDataModel)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据用户ID获取用户所有机型
        /// <summary>
        /// 根据用户ID获取用户所有机型
        /// </summary>
        /// <param name="UserID">传入用户Id</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetUserModel(string UserID)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                return await context.DataQuery($"SELECT m.name from model m INNER JOIN user_model u on u.model_id = m.id WHERE u.user_id = {UserID} ORDER BY name", 1000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据用户获取用户所有机型(GetUserModel)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据用户ID获取用户所有有上传声学数据的机型
        /// <summary>
        /// 根据用户ID获取用户所有有上传声学数据的机型
        /// </summary>
        /// <param name="UserID">传入用户Id</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetUploadAEDataUserModel(string UserID)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                return await context.DataQuery($"SELECT m.name AS Model FROM model m INNER JOIN user_model u ON u.model_id = m.id WHERE u.user_id = {UserID} " +
                    $"AND m.name IN (SELECT Model FROM test_1.Logi_Acoustic_Data ) ORDER BY m.name", 1000);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据用户ID获取用户所有有上传声学数据的机型(GetUploadAEDataUserModel)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据机型名称获取后台有上传测试数据的站别
        /// <summary>
        /// 根据机型名称获取后台有上传测试数据的站别
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetStation(string ModelName)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse resut = await context.DataQuery($"SELECT name,config from station WHERE model_id = (SELECT id from model WHERE `name` = '{ModelName}') ORDER BY name", 1000);
                if (resut.status != 200) return resut;
                List<Dictionary<string, string>> StationInfo = new() { };
                foreach (var value in (List<Dictionary<string, object>>)resut.data)
                {
                    if (value["config"].ToString().Contains("TestLogUploadMySQL=1"))
                    {
                        Dictionary<string, string> keyValues = new Dictionary<string, string> { };
                        keyValues.Add("Station", $"{value["name"]}");
                        StationInfo.Add(keyValues);
                    }
                }
                return ApiResponse.OK(StationInfo);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据机型名称获取后台有上传测试数据的站别(GetStation)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据机型名称获取后台有上传Logi测试数据的站别
        /// <summary>
        /// 根据机型名称获取后台有上传Logi测试数据的站别
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetLGStation(string ModelName)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse resut = await context.DataQuery($"SELECT name,config from station WHERE model_id = (SELECT id from model WHERE `name` = '{ModelName}') ORDER BY name", 1000);
                if (resut.status != 200) return resut;
                List<Dictionary<string, string>> StationInfo = new() { };
                foreach (var value in (List<Dictionary<string, object>>)resut.data)
                {
                    if (value["config"].ToString().Contains("LogiLogFlag=1"))
                    {
                        var configLines = value["config"].ToString().Split('\n');
                        var stationLine = configLines.FirstOrDefault(line => line.Contains("LogiStation"));
                        Dictionary<string, string> keyValues = new() { };
                        keyValues.Add("Station", $"{value["name"]}|{stationLine.Split('=')[1].Replace('\r', ' ').Trim()}");
                        StationInfo.Add(keyValues);
                    }
                }
                return ApiResponse.OK(StationInfo);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据机型名称获取后台有上传Logi测试数据的站别(GetLGStation)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据机型和站别名称获取后台站别测试项目
        /// <summary>
        /// 根据机型和站别名称获取后台站别测试项目
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetStationTestItem(string ModelName, string StationName)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse resut = await context.DataQuery($"SELECT t.name, t.unit, t.lower_value, t.upper_value, t.no, t.cmd from model " +
                    $"inner join station on model.id = station.model_id and model.`name` = '{ModelName}' " +
                    $"inner join station_testitem on station.name = '{StationName}' and station.id = station_testitem.station_id " +
                    $"inner join testitem t on station_testitem.testitem_id = t.id ORDER BY station_testitem.sort_index", 1000);
                return resut;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据机型和站别名称获取后台站别测试项目(GetStationTestItem)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据机型和站别名称获取后台站别Config
        /// <summary>
        /// 根据机型和站别名称获取后台站别Config
        /// </summary>
        /// <param name="ModelName">传入机型名称</param>
        /// <param name="StationName">传入站别名称</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetStationConfig(string ModelName, string StationName)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse resut = await context.DataQuery($"SELECT s.config FROM model m INNER JOIN station s ON s.model_id = m.id WHERE " +
                    $"m.name = '{ModelName}' AND s.name = '{StationName}'", 1000);
                return resut;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据机型和站别名称获取后台站别Config(GetStationConfig)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion

        #region 根据料号获取对应信息
        /// <summary>
        /// 根据料号获取对应信息
        /// </summary>
        /// <param name="ItemNumber">传入料号</param>
        /// <returns></returns>
        public static async Task<ApiResponse> GetItemNumberInfo(string ItemNumber)
        {
            try
            {
                Mysql context = new(GetConfiguration.te_testMysql);
                ApiResponse resut = await context.DataQuery($"SELECT n3.config,n2.name FROM part_no n1 INNER JOIN model n2 on n2.id = n1.model_id " +
                    $"INNER JOIN part_no_config n3 on n3.id = n1.part_no_config_id where n1.no = '{ItemNumber}'; ", 1000);
                return resut;
            }
            catch (Exception ex)
            {
                Log.LogWrite($"根据料号获取对应信息(GetStationConfig)异常,异常信息{ex.Message}", "BackgroundDataLog");
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }
        #endregion
    }
}
