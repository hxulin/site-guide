using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace site_guide.log
{
    class LogHelper
    {
        // 定义日志存储目录
        // C:\Users\Administrator\AppData\Local\Temp\SiteGuide
        private static readonly string LOG_FILE_PATH = Path.GetTempPath() + "SiteGuide";

        // 定义日志文件大小
        private static readonly long LOG_FILE_SIZE = 30000L;

        // 定义日志文件的最大数量
        private static readonly int LOG_FILE_COUNT = 3;

        public static void WriteLog(string logText)
        {
            // 判断日志目录是否存在, 不存在则创建
            if (!Directory.Exists(LOG_FILE_PATH))
            {
                Directory.CreateDirectory(LOG_FILE_PATH);
            }

            // 初始定义当前操作的日志文件名
            string currLogFile = LOG_FILE_PATH + Path.DirectorySeparatorChar + "0.log";

            // 获取当前已经存在的日志文件名
            string[] logFiles = Directory.GetFiles(LOG_FILE_PATH, "*.log");
            if (logFiles.Length > 0)
            {
                var logFileDic = new Dictionary<int, string>();
                foreach (var file in logFiles)
                {
                    try
                    {
                        int fileIndex = int.Parse(Path.GetFileNameWithoutExtension(file));
                        logFileDic.Add(fileIndex, file);
                    }
                    catch
                    {
                    }
                }

                // 对日志文件信息进行排序
                Dictionary<int, string> sortlogFileDic = logFileDic.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

                // 获取最后一个日志文件, 即当前需要读写的日志文件
                var logFileInfo = sortlogFileDic.LastOrDefault();

                var fileSize = new FileInfo(logFileInfo.Value).Length;

                // 如果日志文件超过预定大小, 创建新的日志文件
                if (fileSize > LOG_FILE_SIZE)
                {
                    // 如果日志文件超过预定数量, 删除最早的一个历史日志文件
                    if (sortlogFileDic.Count >= LOG_FILE_COUNT)
                    {
                        File.Delete(sortlogFileDic.FirstOrDefault().Value);
                    }
                    currLogFile = LOG_FILE_PATH + Path.DirectorySeparatorChar +  (logFileInfo.Key + 1) + ".log";
                    File.Create(currLogFile).Close();
                }
                else
                {
                    currLogFile = logFileInfo.Value;
                }
            }
            else
            {
                File.Create(currLogFile).Close();
            }

            // ==========  当前需要操作的日志文件是: currLogFile  ==========

            // 获取当前记录日志的时间
            DateTime current = DateTime.Now;

            StringBuilder strBuilder = new StringBuilder();

            // 判断是否是同一天记录日志, 不同日期记录的日志中间添加换行
            StreamReader reader = new StreamReader(currLogFile);
            string nextLine = string.Empty;
            while (!reader.EndOfStream)
            {
                nextLine = reader.ReadLine();
            }
            if (!string.IsNullOrEmpty(nextLine) && (nextLine.Length < 10 || current.ToString("yyyy-MM-dd") != nextLine.Substring(0, 10)))
            {
                strBuilder.Append("\r\n\r\n");
            }
            reader.Close();

            // 添加时间戳, 记录日志
            strBuilder.Append(current.ToString("yyyy-MM-dd HH:mm:ss"));
            strBuilder.Append("  ---  ");
            strBuilder.Append(logText);
            StreamWriter writer = File.AppendText(currLogFile);
            writer.WriteLine(strBuilder.ToString());
            writer.Close();
        }
    }
}
