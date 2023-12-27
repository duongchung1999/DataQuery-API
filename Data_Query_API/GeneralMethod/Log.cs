using System.Reflection;

namespace Data_Query_API.GeneralMethod
{
    /// <summary>
    /// Log记录类
    /// </summary>
    public class Log
    {
        #region 记录Log档
        /// <summary>
        /// 记录log
        /// </summary>
        /// <param name="str"></param>
        /// <param name="Name"></param>
        public static void LogWrite(string str,string Name)
        {
            string path = GetConfiguration.FileStorageLocation + "\\Log";
            StreamWriter sr;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += @$"\{Name}.txt";
            if (!File.Exists(path))
            {
                sr = File.CreateText(path);
            }
            else
            {
                sr = File.AppendText(path);
            }
            sr.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {str}");
            sr.Flush();
            sr.Close();
        }
        #endregion
    }
}
