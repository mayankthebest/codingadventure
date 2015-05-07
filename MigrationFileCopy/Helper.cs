using System;
using System.ComponentModel;
using System.Configuration;
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
        /// <summary>
        /// Restarts the job scheduler.
        /// </summary>
        public static void RestartJobScheduler()
        {
            worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
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
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Stopping service on " + computer });
                        ServiceController service = new ServiceController(ConfigurationManager.AppSettings["ServiceName"], computer);
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Service stopped on " + computer });
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Starting service on " + computer });
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(UpdateLog), new object[] { "Service started on " + computer });
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
