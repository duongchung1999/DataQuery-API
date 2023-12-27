using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Data_Query_API.GeneralMethod
{
    /// <summary>
    /// 读取配置文件内容
    /// </summary>
    public class GetConfiguration
    {
        /// <summary>
        /// 测试数据数据库
        /// </summary>
        public static string testLogMysql = GetDefinition("ConnectionStrings:testLogMysql");

        /// <summary>
        /// 后台数据库
        /// </summary>
        public static string te_testMysql = GetDefinition("ConnectionStrings:te_testMysql");

        /// <summary>
        /// 生产实时数据数据库
        /// </summary>
        public static string testDailyMysql = GetDefinition("ConnectionStrings:testDailyMysql");

        /// <summary>
        /// Logi测试数据数据库
        /// </summary>
        public static string LGTestLogMysql = GetDefinition("ConnectionStrings:LGTestLogMysql");

        /// <summary>
        /// 获取秘钥
        /// </summary>
        public static string SecretKey = GetDefinition("Authentication:SecretKey");

        /// <summary>
        /// 获取发行者
        /// </summary>
        public static string Issuer = GetDefinition("Authentication:Issuer");

        /// <summary>
        /// 文件存储位置
        /// </summary>
        public static string FileStorageLocation = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;


        /// <summary>
        /// 读取配置文件指定Key的内容
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string GetDefinition(string Key)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");
            var config = builder.Build();
            return config[Key];
        }
    }
}
