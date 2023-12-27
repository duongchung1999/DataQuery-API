
using Data_Query_API.Model;
using Data_Query_API.DataProcessing.User;
using System.IdentityModel.Tokens.Jwt;

namespace Data_Query_API.GeneralMethod
{
    /// <summary>
    /// 
    /// </summary>
    public class EncryptionCheck
    {
        /// <summary>
        /// 校验是否为合法请求
        /// </summary>
        /// <param name="httpRequest">传入Http内容</param>
        /// <returns></returns>
        public static async Task<ApiResponse> CheckInfo(HttpRequest httpRequest)
        {
            return await Task.Run(() => {
                string AccountNumber = httpRequest.Headers["AccountNumber"].ToString(); // 获取操作者账号
                string Token = httpRequest.Headers["Token"].ToString();
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = tokenHandler.ReadJwtToken(Token);
                DateTime expirationTime = jwtToken.ValidTo;
                DateTime currentTime = DateTime.UtcNow; // 使用UTC时间以避免时区问题
                // 获取Token
                //Mysql context = new(GetConfiguration.OfficialWebsiteMysql);
                //string sql = $"SELECT * from user_table WHERE AccountNumber='{AccountNumber}' and Token='{Token}';";
                //ApiResponse result = await context.DataQuery(sql, 2000);
                //if (result.status == 500) return result;
                //List<Dictionary<string, object>> UserInfo = (List<Dictionary<string, object>>)result.data;
                //if (UserInfo.Count>0)
                if (Token.Equals(UserData.GenerateToken(AccountNumber)))
                {
                    return ApiResponse.OK("");
                }
                else
                {
                    return ApiResponse.Error(401, "验证失败，请重新登录");
                }
            });
        }
    }

}
