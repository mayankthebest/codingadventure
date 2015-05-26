using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Threading;

namespace MigrationFileCopy
{
    /// <summary>
    /// Helper class
    /// </summary>
    static class Helper
    {
        static BackgroundWorker worker = null;
        static string _serviceName = string.Empty;

        /// <summary>
        /// Restarts the job scheduler.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public static void RestartJobScheduler(string serviceName)
        {
            _serviceName = serviceName;
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Starts the process.
        /// </summary>
        /// <param name="computerName">Name of the computer.</param>
        /// <param name="processName">Name of the process.</param>
        public static void StartProcess(string computerName, string processName)
        {
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoMoreWork;
            worker.RunWorkerAsync(new string[] { computerName, processName });
        }

        static void worker_DoMoreWork(object sender, DoWorkEventArgs e)
        {
            string computerName = (e.Argument as string[])[0];
            string processName = (e.Argument as string[])[1];
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = @"C:\PsTools\psexec.exe";
            info.Arguments = @"\\" + computerName + " -s -d -i 2 \"" + processName + "\"";
            info.CreateNoWindow = true;
            Process p = Process.Start(info);
        }

        /// <summary>
        /// Updates the log.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static object UpdateLog(object text)
        {
            MigrationTaskViewModel.AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = ((object[])text)[0].ToString() });
            return null;
        }

        /// <summary>
        /// Handles the DoWork event of the worker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (File.Exists("ComputerList.txt"))
            {
                var allComputers = File.ReadAllLines("ComputerList.txt");
                foreach (var computer in allComputers)
                {
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Stopping " + _serviceName + " service on " + computer });
                        ServiceController service = new ServiceController(_serviceName, computer);
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Service " + _serviceName + " stopped on " + computer });
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Starting " + _serviceName + " service on " + computer });
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Service " + _serviceName + " started on " + computer });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { ex.Message });
                    }
                }
            }
        }
    }
}
