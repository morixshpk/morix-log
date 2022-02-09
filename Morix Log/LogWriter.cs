using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morix
{
    internal class LogWriter
    {
        private DateTime _date;
        private readonly object _sync = new object();
        private int _versionNo = 0;

        public DateTime LastWriteDateTime { get; private set; }

        public string FileName { get; set; }

        public void Write(string content, bool sameLine = false)
        {
            lock (_sync)
            {
                LastWriteDateTime = DateTime.Now;
                if (_date != LastWriteDateTime.Date)
                {
                    _versionNo = 0;
                    _date = LastWriteDateTime.Date;
                }
                string file = string.Format(@"{0}\{1}-{2:yyyy-MM-dd}-{3}.txt", Log.Dir, FileName, _date, _versionNo);
                FileInfo fi = new FileInfo(file);

                while (fi.Exists)
                {
                    if (fi.Length > Log.MaxSize)
                    {
                        _versionNo++;
                        file = string.Format(@"{0}\{1}-{2:yyyy-MM-dd}-{3}.txt", Log.Dir, FileName, _date, _versionNo);
                        fi = new FileInfo(file);
                        if (fi.Exists)
                            continue;
                    }
                    else
                        break;
                }

                var sb = new StringBuilder();
                if (sameLine)
                {
                    sb.AppendLine(LastWriteDateTime.ToString("HH:mm:ss.fff") + ":" + content);
                }
                else
                {
                    sb.AppendLine("----------" + LastWriteDateTime.ToString("HH:mm:ss.fff") + "----------");
                    sb.AppendLine(content);
                }

                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(sb.ToString());
                    sw.Close();
                }
            }
        }
    }
}