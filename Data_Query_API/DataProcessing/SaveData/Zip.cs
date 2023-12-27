using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

namespace Data_Query_API.SaveData
{
    /// <summary>
    /// 压缩指定文件夹
    /// </summary>
    public class Zip
    {
        #region 压缩
        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="dirPath">要打包的文件夹</param>
        /// <param name="GzipFileName">目标文件名</param>
        /// <param name="CompressionLevel">压缩品质级别（0~9）</param>
        /// <param name="deleteDir">是否删除原文件夹</param>
        public static void CompressDirectory(string dirPath, string GzipFileName, int CompressionLevel, bool deleteDir)
        {

            //压缩文件为空时默认与压缩文件夹同一级目录
            if (GzipFileName == string.Empty)
            {
                GzipFileName = dirPath.Substring(dirPath.LastIndexOf("\\"));
                GzipFileName = dirPath.Substring(0, dirPath.LastIndexOf("\\")) + "\\" + GzipFileName + ".zip";
            }
            using (ZipOutputStream zipoutputstream = new ZipOutputStream(File.Create(GzipFileName)))
            {
                //设置压缩文件级别
                zipoutputstream.SetLevel(CompressionLevel);
                Crc32 crc = new Crc32();
                Dictionary<string, DateTime> fileList = GetAllFies(dirPath);
                foreach (KeyValuePair<string, DateTime> item in fileList)
                {
                    //将文件数据读到流里面
                    FileStream fs = File.OpenRead(item.Key.ToString());
                    byte[] buffer = new byte[fs.Length];
                    //从流里读出来赋值给缓冲区
                    fs.Read(buffer, 0, buffer.Length);
                    ZipEntry entry = new ZipEntry(item.Key.Substring(dirPath.Length));
                    entry.DateTime = item.Value;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipoutputstream.PutNextEntry(entry);
                    zipoutputstream.Write(buffer, 0, buffer.Length);
                }
            }
            if (deleteDir)
            {
                Directory.Delete(dirPath, true);
            }
        }
        
        /// <summary>
        /// 获取所有文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static Dictionary<string, DateTime> GetAllFies(string dir)
        {
            Dictionary<string, DateTime> FilesList = new Dictionary<string, DateTime>();
            DirectoryInfo fileDire = new DirectoryInfo(dir);
            if (!fileDire.Exists)
            {
                throw new System.IO.FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
            }
            GetAllDirFiles(fileDire, FilesList);
            GetAllDirsFiles(fileDire.GetDirectories(), FilesList);
            return FilesList;
        }
        
        /// <summary>
        /// 获取一个文件夹下的所有文件夹里的文件
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="filesList"></param>
        private static void GetAllDirsFiles(DirectoryInfo[] dirs, Dictionary<string, DateTime> filesList)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                foreach (FileInfo file in dir.GetFiles("*.*"))
                {
                    filesList.Add(file.FullName, file.LastWriteTime);
                }
                GetAllDirsFiles(dir.GetDirectories(), filesList);
            }
        }
        
        /// <summary>
        /// 获取一个文件夹下的文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="filesList"></param>
        private static void GetAllDirFiles(DirectoryInfo dir, Dictionary<string, DateTime> filesList)
        {
            foreach (FileInfo file in dir.GetFiles("*.*"))
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
        }
        #endregion
    }
}
