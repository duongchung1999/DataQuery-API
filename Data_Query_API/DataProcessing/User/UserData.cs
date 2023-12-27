using Data_Query_API.GeneralMethod;
using Data_Query_API.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Data_Query_API.DataProcessing.User
{
    /// <summary>
    /// 用户数据处理
    /// </summary>
    public class UserData
    {
        const string UserTableName = "user_table";

        /// <summary>
        /// 登录并返回Token
        /// </summary>
        /// <param name="UserName">账号</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public static async Task<ApiResponse> Login(string UserName, string Password)
        {
            try
            {
                string TokenStr = GenerateToken(UserName);
                Password = Encryption.Md5(Password);
                Mysql context = new(GetConfiguration.te_testMysql);
                string sql = $"SELECT * from user WHERE username='{UserName}' and password='{Password}'";
                var result = await context.DataQuery(sql, 2000);
                if (result.ToString().Contains("500"))
                {
                    var rs = result.ToString().Split('|');
                    return ApiResponse.Error(int.Parse(rs[0]), rs[1]);
                }
                List<Dictionary<string, object>> UserInfo = (List<Dictionary<string, object>>)result.data;
                if (UserInfo.Count == 0) return ApiResponse.OK(UserInfo);
                UserInfo[0]["Token"] = TokenStr;
                return ApiResponse.OK(UserInfo);
            }
            catch (Exception ex)
            {
                Log.LogWrite($"登录并返回Token异常(Login) 异常信息{ex.Message}", "UserDataLog");
                return ApiResponse.Error(500, ex.Message);
            }
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="AccountNumber">传入账号</param>
        /// <returns></returns>
        public static string GenerateToken(string AccountNumber)
        {
            //1.生成JWT
            //Header,选择签名算法
            var signingAlogorithm = SecurityAlgorithms.HmacSha256;
            //Payload,存放用户信息，下面我们放了一个用户id
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub,AccountNumber)
                };
            //Signature
            //取出私钥并以utf8编码字节输出
            var secretByte = Encoding.UTF8.GetBytes(GetConfiguration.SecretKey);
            //使用非对称算法对私钥进行加密
            var signingKey = new SymmetricSecurityKey(secretByte);
            //使用HmacSha256来验证加密后的私钥生成数字签名
            var signingCredentials = new SigningCredentials(signingKey, signingAlogorithm);
            // 生成Token
            var Token = new JwtSecurityToken(
                    issuer: GetConfiguration.Issuer, //发布者
                    audience: AccountNumber, //接收者
                    claims: claims, //存放的用户信息
                    notBefore: DateTime.UtcNow, //发布时间
                    expires: DateTime.UtcNow.AddHours(6), //有效期设置为6小时
                    signingCredentials //数字签名
                );
            //生成字符串token
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }

    }
}
