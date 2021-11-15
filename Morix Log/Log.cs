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

        private static int _versioningNo = 0;
        private static readonly long _maxSize = 100000000;
        private static DateTime _date = DateTime.Now.Date;

        private static readonly Timer _timer;

        private static string _dir = "";
        private static int _noDays = 30;

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
            _timer.Interval = TimeSpan.FromHours(12).TotalMilliseconds;
            _timer.Start();
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
            DeleteOldFiles();
            _timer.Start();
        }

        public static void Error(Exception ex)
        {
            lock (_sync)
            {
                try
                {
                    if (_date != DateTime.Now.Date)
                    {
                        _versioningNo = 0;
                        _date = DateTime.Now.Date;
                    }
                    string file = string.Format(@"{0}\Error-{1:yyyy-MM-dd}-{2}.txt", _dir, _date, _versioningNo);
                    FileInfo fi = new FileInfo(file);

                    while (fi.Exists)
                    {
                        if (fi.Length > _maxSize)
                        {
                            _versioningNo++;
                            file = string.Format(@"{0}\Error-{1:yyyy-MM-dd}-{2}.txt", _dir, _date, _versioningNo);
                            fi = new FileInfo(file);
                            if (fi.Exists)
                                continue;
                        }
                        else
                            break;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("----------" + DateTime.Now.ToString("HH:mm:ss.fff") + "----------");
                    sb.AppendLine(ex.ToString());

                    using (StreamWriter sw = File.AppendText(file))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
                catch (Exception ex1)
                {
                    InternalError(ex1);
                }
            }
        }

        public static void Error(Exception ex, string note)
        {
            lock (_sync)
            {
                try
                {
                    if (_date != DateTime.Now.Date)
                    {
                        _versioningNo = 0;
                        _date = DateTime.Now.Date;
                    }
                    string file = string.Format(@"{0}\Error-{1:yyyy-MM-dd}-{2}.txt", _dir, _date, _versioningNo);
                    FileInfo fi = new FileInfo(file);

                    while (fi.Exists)
                    {
                        if (fi.Length > _maxSize)
                        {
                            _versioningNo++;
                            file = string.Format(@"{0}\Error-{1:yyyy-MM-dd}-{2}.txt", _dir, _date, _versioningNo);
                            fi = new FileInfo(file);
                            if (fi.Exists)
                                continue;
                        }
                        else
                            break;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("----------" + DateTime.Now.ToString("HH:mm:ss.fff") + "----------");
                    sb.AppendLine("Note: " + note);
                    sb.AppendLine(ex.ToString());

                    using (StreamWriter sw = File.AppendText(file))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
                catch (Exception ex1)
                {
                    InternalError(ex1);
                }
            }
        }

        public static void Debug(string content)
        {
            lock (_sync)
            {
                try
                {
                    if (_date != DateTime.Now.Date)
                    {
                        _versioningNo = 0;
                        _date = DateTime.Now.Date;
                    }
                    string file = string.Format(@"{0}\Debug-{1:yyyy-MM-dd}-{2}.txt", _dir, _date, _versioningNo);
                    FileInfo fi = new FileInfo(file);

                    while (fi.Exists)
                    {
                        if (fi.Length > _maxSize)
                        {
                            _versioningNo++;
                            file = string.Format(@"{0}\Debug-{1:yyyy-MM-dd}-{2}.txt", _dir, _date, _versioningNo);
                            fi = new FileInfo(file);
                            if (fi.Exists)
                                continue;
                        }
                        else
                            break;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("----------" + DateTime.Now.ToString("HH:mm:ss.fff") + "----------");
                    sb.AppendLine(content);

                    using (StreamWriter sw = File.AppendText(file))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    InternalError(ex);
                }
            }
        }

        public static void Custom(string filename, string content, bool sameLine = false)
        {
            lock (_sync)
            {
                try
                {
                    if (_date != DateTime.Now.Date)
                    {
                        _versioningNo = 0;
                        _date = DateTime.Now.Date;
                    }
                    string file = string.Format(@"{0}\{1}-{2:yyyy-MM-dd}-{3}.txt", _dir, filename, _date, _versioningNo);
                    FileInfo fi = new FileInfo(file);

                    while (fi.Exists)
                    {
                        if (fi.Length > _maxSize)
                        {
                            _versioningNo++;
                            file = string.Format(@"{0}\{1}-{2:yyyy-MM-dd}-{3}.txt", _dir, filename, _date, _versioningNo);
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
                        sb.AppendLine(DateTime.Now.ToString("HH:mm:ss.fff") + ":" + content);
                    }
                    else
                    {
                        sb.AppendLine("----------" + DateTime.Now.ToString("HH:mm:ss.fff") + "----------");
                        sb.AppendLine(content);
                    }

                    using (StreamWriter sw = File.AppendText(file))
                    {
                        sw.WriteLine(sb.ToString());
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    InternalError(ex);
                }
            }
        }

        public static void WriteCallingMethod()
        {
            try
            {
                var frame = new System.Diagnostics.StackFrame(1);
                var method = frame.GetMethod();
                Custom("Method", "Executed " + method.DeclaringType.Name + "." + method.Name, true);
            }
            catch(Exception ex)
            {
                InternalError(ex);
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