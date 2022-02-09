using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.Timers;

namespace Morix
{
    public static class Log
    {
        private static readonly object _sync = new object();        
        private static readonly Dictionary<string, LogWriter> _logWriters = new Dictionary<string, LogWriter>();

        private static readonly Timer _timer;
        private static string _dir = "";
        private static int _noDays = 30;

        public const long MaxSize = 100000000;

        public static string Dir
        {
            get { return _dir; }

            set
            {
                if (Directory.Exists(value) == false)
                {
                    Directory.CreateDirectory(value);

                    if (Directory.Exists(value) == false)
                        throw new Exception("Directory " + value + " can't be created!");
                }
                _dir = value;
            }
        }

        public static int NoDays
        {
            get { return _noDays; }
            set
            {
                if (value > 0)
                    _noDays = value;
                else
                    _noDays = 30;
            }
        }

        static Log()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            var dir = Path.Combine(Path.GetDirectoryName(path), "..\\App_Data");
            if (Directory.Exists(dir))
                dir = Path.Combine(Path.GetDirectoryName(path), "..\\App_Data\\Logs");
            else
                dir = Path.Combine(Path.GetDirectoryName(path), "Logs");

            Dir = dir;
            NoDays = 30;

            string fileTemp = string.Format(@"{0}\log{1}.txt", _dir, DateTime.Now.Ticks);
            try
            {
                System.IO.File.AppendAllText(fileTemp, string.Format("Test", Environment.NewLine));
            }
            catch { }

            if (File.Exists(fileTemp))
                File.Delete(fileTemp);
            else
                Morix.Log.InternalError(new Exception("Can't write logs to directory: " + _dir));

            _timer = new Timer();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = TimeSpan.FromHours(6).TotalMilliseconds;
            _timer.Start();
        }

        private static LogWriter GetLogWwriter(string fileName)
        {
            lock (_sync)
            {
                _logWriters.TryGetValue(fileName, out var writer);

                if (writer == null)
                {
                    writer = new LogWriter { FileName = fileName };
                    _logWriters[fileName] = writer;
                }

                return writer;
            }
        }

        private static void DeleteOldWriters()
        {
            var dt = DateTime.Now.AddMinutes(30);
            lock (_sync)
            {
                var _files = new HashSet<string>();

                foreach (var logWriter in _logWriters.Values)
                {
                    if (logWriter.LastWriteDateTime < dt)
                        _files.Add(logWriter.FileName);
                }

                foreach (var file in _files)
                    _logWriters.Remove(file);
            }
        }

        private static void DeleteOldFiles()
        {
            try
            {
                string[] files = Directory.GetFiles(_dir);
                DateTime dt = DateTime.Today.AddDays(-NoDays);

                int fileDeleted = 0;
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < dt)
                    {
                        fi.Delete();
                        fileDeleted++;

                        Debug("File Deleted: " + file);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalError(ex);
            }
        }
               
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            DeleteOldWriters();
            DeleteOldFiles();
            _timer.Start();
        }

        public static void Error(Exception ex)
        {
            try
            {
                var lw = GetLogWwriter("Error");
                lw.Write(ex.ToString());
            }
            catch (Exception ex1)
            {
                InternalError(ex1);
            }
        }

        public static void Error(Exception ex, string note)
        {

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Note: " + note);
                sb.Append(ex.ToString());

                var lw = GetLogWwriter("Error");
                lw.Write(sb.ToString());
            }
            catch (Exception ex1)
            {
                InternalError(ex1);
            }
        }

        public static void Debug(string content)
        {
            try
            {
                var lw = GetLogWwriter("Debug");
                lw.Write(content);
            }
            catch (Exception ex1)
            {
                InternalError(ex1);
            }
        }

        /// <summary>
        /// Log to custom filename
        /// </summary>
        /// <param name="filename">Filename where logs will be writen. Example 'mylog' and file will be create mylog.txt</param>
        /// <param name="content">Content to be write to file</param>
        /// <param name="sameLine">SameLine or in different line. Basically Write or WriteLine</param>
        public static void Custom(string filename, string content, bool sameLine = false)
        {
            try
            {
                var lw = GetLogWwriter(filename);
                lw.Write(content, sameLine);
            }
            catch (Exception ex1)
            {
                InternalError(ex1);
            }
        }

        private static void InternalError(Exception ex)
        {
            try
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Morix.Log";
                    eventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                }
            }
            catch
            {

            }
        }
    }
}