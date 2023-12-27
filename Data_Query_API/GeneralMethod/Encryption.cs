using System.Security.Cryptography;
using System.Text;

namespace Data_Query_API.GeneralMethod
{
    /// <summary>
    /// MD5 加密
    /// </summary>
    public class Encryption
    {
        /// <summary>
        /// MDF加密
        /// </summary>
        /// <param name="txt">传入加密字符串</param>
        /// <returns></returns>
        public static string Md5(string txt)
        {
            byte[] sor = Encoding.UTF8.GetBytes(txt);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new(40);
            for (int i = 0; i < result.Length; i++)
            {
                //加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
                strbul.Append(result[i].ToString("x2"));
            }
            return strbul.ToString();
        }
    }
}
