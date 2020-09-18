using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Vishnu.Interchange;

namespace TextFileLogger
{
    /// <summary>
    /// Loggt Vishnu-Ereignisse in ein Logfile.
    /// </summary>
    /// <remarks>
    /// File: TextFileLogger.cs
    /// Autor: Erik Nagel
    ///
    /// 27.07.2013 Erik Nagel: erstellt
    /// 11.08.2018 Erik Nagel: Der Message-TimeStamp wird jetzt mit maximaler Präzision ausgegeben.
    /// </remarks>
    public class TextFileLogger : INodeLogger
    {
        #region INodeLogger implementaion

        /// <summary>
        /// Übernahme von diversen Logging-Informationen.
        /// </summary>
        /// <param name="loggerParameters">Spezifische Aufrufparameter oder null.</param>
        /// <param name="treeParameters">Für den gesamten Tree gültige Parameter oder null.</param>
        /// <param name="treeEvent">Klasse mit Informationen über das Ereignis.</param>
        /// <param name="additionalEventArgs">Enthält z.B. beim Event 'Exception' die zugehörige Exception.</param>
        public void Log(object loggerParameters, TreeParameters treeParameters, TreeEvent treeEvent, object additionalEventArgs)
        {
            string logPath = null;
            if (loggerParameters != null)
            {
                logPath = loggerParameters.ToString();
            }
            string indent = "        ";
            string addInfos = indent;
            if (treeEvent.Name.Contains("Exception"))
            {
                addInfos += (additionalEventArgs as Exception).Message;
            }
            if (treeEvent.Name.Contains("ProgressChanged"))
            {
                addInfos += String.Format("Fortschritt {0:d3}%", Convert.ToInt32((additionalEventArgs as object)));
            }
            // StringBuilder bigMessage = new StringBuilder(treeEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss,ms")
            string timestamp = System.String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:yyyy.MM.dd HH:mm:ss,ffffff}", new object[] { treeEvent.Timestamp });
            StringBuilder bigMessage = new StringBuilder(timestamp + " Event: " + treeEvent.Name);
            string IdName = treeEvent.NodeName + "|" + treeEvent.SenderId;
            bigMessage.Append(Environment.NewLine + indent + "Knoten: " + IdName);
            bigMessage.Append(", Logical: " + treeEvent.Logical);
            if (treeEvent.SenderId != treeEvent.SourceId)
            {
                bigMessage.Append(", Quelle: " + treeEvent.SourceId);
            }
            else
            {
                bigMessage.Append(", Quelle: " + treeEvent.SourceId);
            }
            bigMessage.Append(Environment.NewLine + indent + treeEvent.ReplaceWildcards("%MachineName%") + ", Thread: " + treeEvent.ThreadId.ToString());
            bigMessage.Append(", Tree: " + treeParameters.ToString());
            if (addInfos.Trim() != "")
            {
                bigMessage.Append(Environment.NewLine + addInfos);
            }
            if (!String.IsNullOrEmpty(treeEvent.NodePath))
            {
                bigMessage.Append(Environment.NewLine + indent + treeEvent.NodePath);
            }
            bigMessage.Append(", Status: " + treeEvent.State.ToString());
            string processDirectory = treeEvent.ReplaceWildcards("%WorkingDirectory%");
            if (logPath == null)
            {
                this._debugFile = treeEvent.ReplaceWildcards("%DebugFile%");
            }
            else
            {
                this._debugFile = logPath;
            }
            if (!Directory.Exists(Path.GetDirectoryName(this._debugFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this._debugFile));
            }
            bigMessage.Append(Environment.NewLine + indent + "WorkingDirectory: " + processDirectory);
            this.log(bigMessage.ToString());
        }

        #endregion INodeLogger implementaion

        private readonly object _locker;
        private delegate void AsyncDelegate(string message);
        private AsyncDelegate _asyncDelegate;
        private string _debugFile;

        /// <summary>
        /// Standard Konstruktor.
        /// </summary>
        public TextFileLogger()
        {
            _locker = new object();
            _asyncDelegate = new AsyncDelegate(AsyncLog);
        }

        private void AsyncLog(string message)
        {
            int maxTries = 5;
            lock (_locker)
            {
                StreamWriter SW;
                int i = 0;
                do
                {
                    SW = null;
                    try
                    {
                        SW = new StreamWriter(new FileStream(this._debugFile, FileMode.Append, FileAccess.Write), Encoding.Default);
                        SW.WriteLine(message);
                        i = maxTries;
                    }
                    catch (SystemException)
                    {
                        Thread.Sleep(10);
                    }
                    finally
                    {
                        if (SW != null)
                        {
                            try
                            {
                                SW.Close();
                            }
                            catch { }
                            SW.Dispose();
                        }
                    }
                } while (++i < maxTries);
            }
        }

        private void log(string message)
        {
            _asyncDelegate.BeginInvoke(message, null, null);
        }
    }
}
