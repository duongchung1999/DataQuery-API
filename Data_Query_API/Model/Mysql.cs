using MySql.Data.MySqlClient;
using System.Data;

// 数据库操作类
namespace Data_Query_API.Model
{
    /// <summary>
    /// 数据库操作类
    /// </summary>
    public class Mysql
    {
        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="connectionString"></param>
        public Mysql(string connectionString)
        {
            ConnectionString = connectionString;
        }

        //建立连接mysql
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /// <summary>
        /// 增、删、改公共方法
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse> commonExecute(string sql)
        {
            using MySqlConnection conn = GetConnection();
            try
            {
                //当连接处于关闭状态时,打开连接
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();
                MySqlCommand command = new(sql, conn);
                int res = await command.ExecuteNonQueryAsync();
                if (res != -1)
                {
                    return ApiResponse.OK("");
                }
                else
                {
                    return ApiResponse.Error(500, "操作数据库失败");
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询方法,直接返回结果
        /// </summary>
        /// <returns></returns>
        public async Task<DataTable?> Query(string sql)
        {
            try
            {
                using MySqlConnection conn = GetConnection();
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
                await conn.OpenAsync();
                MySqlCommand command = new()
                {
                    Connection = conn,
                    CommandText = sql,
                }; 
                DataTable dt = new();
                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    dt.Load(reader);
                }
                return dt;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 查询方法,返回API格式
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse> DataQuery(string sql, int time)
        {
            try
            {
                using MySqlConnection conn = GetConnection();
                //当连接处于打开状态时关闭,然后再打开,避免有时候数据不能及时更新
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
                await conn.OpenAsync();
                MySqlCommand command = new()
                {
                    Connection = conn,
                    CommandText = sql,
                    //设置命令的执行超时
                    CommandTimeout = time
                };
                MySqlDataAdapter da = new(command);
                DataTable dt = new ();
                await da.FillAsync(dt);
                return ApiResponse.OK(await DataTableToDictionary(dt));
            }
            catch (Exception ex)
            {
                return ApiResponse.Error(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// DataTable 转字典
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, object>>> DataTableToDictionary(DataTable dataTable)
        {
            return await Task.Run(() =>
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>> { };
                if (dataTable != null || dataTable.Columns.Count > 0)
                {
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        Dictionary<string, object> row = new Dictionary<string, object>();
                        foreach (DataColumn dataColumn in dataTable.Columns)
                        {
                            row.Add(dataColumn.ColumnName, dataRow[dataColumn].ToString());
                        }
                        result.Add(row);
                    }
                }
                else
                {
                    result = new List<Dictionary<string, object>> { };
                }
                return result;
            });           
        }
    }
}
