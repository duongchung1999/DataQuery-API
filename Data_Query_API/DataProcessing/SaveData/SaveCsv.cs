using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data_Query_API.SaveData
{
    /// <summary>
    /// 保存为 CSV 数据处理区
    /// </summary>
    public class SaveCsv
    {
        /// <summary>
        /// 保存当日LogiData数据
        /// </summary>
        /// <param name="path">传入存储路径</param>
        /// <param name="Name">文件名称</param>
        /// <param name="result">写入内容 ----单元格内容，单元格内容-----</param>
        public static bool WriteCsv(string path, string Name, string result)
        {
            try
            {
                path = $@"{path}\CSV";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                // 判断是否存在CSV文件，存在则删除
                string fileName = $@"{path}\{Name}";
                if (File.Exists(fileName)) File.Delete(fileName);
                StreamWriter swl = new(fileName, true, new UTF8Encoding(false));
                swl.Write(result);
                swl.Close();
                return true;
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 保存页面Logi测试数据
        /// </summary>
        /// <param name="path">传入存储路径</param>
        /// <param name="FileName">文件夹名称</param>
        /// <param name="FileTime">文件夹名称加入时间</param>
        /// <param name="CSVName">CSV文件名称</param>
        /// <param name="result">写入内容 ----单元格内容，单元格内容-----</param>
        /// <param name="flag">传入是否为第一次</param>
        public static void WritePageCsv(string path, string FileName, string FileTime, string CSVName, string result, bool flag)
        {
            if (flag)
            {
                // 判断是否存在LogiData的文件夹
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                // 判断是否存在当前存储文件夹，存在则删除
                path = $@"{path}\{FileName}";
                if (Directory.Exists(path)) Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            else
            {
                path = $@"{path}\{FileName}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }

            // 判断需不需要创建日期文件夹
            if (FileTime.Length > 0)
            {
                path = $@"{path}\{FileTime}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            path = $@"{path}\CSV";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string fileName = $@"{path}\{CSVName}";
            if (File.Exists(fileName)) File.Delete(fileName);
            StreamWriter sw = new(fileName, true, new UTF8Encoding(false));
            sw.Write(result);
            sw.Close();
        }

        /// <summary>
        /// 复制文件夹及文件
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <returns></returns>
        public static int CopyFolder(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                if (!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                //得到原文件根目录下的所有文件
                string[] files = Directory.GetFiles(sourceFolder);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destFolder, name);
                   File.Copy(file, dest);//复制文件
                }
                //得到原文件根目录下的所有文件夹
                string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(destFolder, name);
                    CopyFolder(folder, dest);//构建目标路径,递归复制文件
                }
                return 1;
            }
            catch
            {
                return 0;
            }

        }
    }
}
