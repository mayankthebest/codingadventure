using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MigrationFileCopy
{
    /// <summary>
    /// View Model for tool
    /// </summary>
    public class MigrationTaskViewModel : INotifyPropertyChanged
    {
        #region Private Variables
        private ObservableCollection<MigrationTask> allTasks = new ObservableCollection<MigrationTask>();
        private static ObservableCollection<LogEntry> allLogEntries = new ObservableCollection<LogEntry>();
        private MigrationTask activeTask = null;
        private string taskTitle;
        private string projectTitle = "Migration Copy Tool";
        private object userControl;
        private TaskType migrationTaskType = TaskType.None;
        private bool isRunning = false;
        private string sourceSelectedPath;
        private string destinationSelectedPath;
        private string fileSelectedPath;
        private bool overwriteDestinationFile = false;
        private bool isFolderCopy = false;
        private string fileName;
        private string findWhat;
        private string replaceWith;
        private string computerList;
        private string projectSavePath;
        private bool canTasksRun;
        private ICommand onTaskAddedCommand;
        private ICommand onBrowseClickedCommand;
        private ICommand onCreateTaskCommand;
        private ICommand onContentChangedCommand;
        private ICommand onOpenProjectCommand;
        private ICommand onSaveProjectCommand;
        private ICommand onRemoveTaskCommand;
        private ICommand onRunAllTasksCommand;
        #endregion

        #region Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<MigrationTask> AllTasks
        {
            get { return allTasks; }
            set { allTasks = value; }
        }
        public static ObservableCollection<LogEntry> AllLogEntries
        {
            get { return allLogEntries; }
            set { allLogEntries = value; }
        }
        public MigrationTask ActiveTask
        {
            get { return activeTask; }
            set
            {
                activeTask = value;
                NotifyPropertyChanged("ActiveTask");
            }
        }
        public string ProjectTitle
        {
            get { return projectTitle; }
            set
            {
                projectTitle = value;
                NotifyPropertyChanged("ProjectTitle");
            }
        }
        public string TaskTitle
        {
            get { return taskTitle; }
            set
            {
                taskTitle = value;
                NotifyPropertyChanged("TaskTitle");
            }
        }
        public object UserControl
        {
            get { return userControl; }
            set
            {
                userControl = value;
                NotifyPropertyChanged("UserControl");
            }
        }
        public TaskType MigrationTaskType
        {
            get { return migrationTaskType; }
            set
            {
                migrationTaskType = value;
                NotifyPropertyChanged("TaskType");
            }
        }
        public string SourceSelectedPath
        {
            get { return sourceSelectedPath; }
            set
            {
                sourceSelectedPath = value;
                NotifyPropertyChanged("SourceSelectedPath");
            }
        }
        public string DestinationSelectedPath
        {
            get { return destinationSelectedPath; }
            set
            {
                destinationSelectedPath = value;
                NotifyPropertyChanged("DestinationSelectedPath");
            }
        }
        public string FileSelectedPath
        {
            get { return fileSelectedPath; }
            set
            {
                fileSelectedPath = value;
                NotifyPropertyChanged("FileSelectedPath");
            }
        }
        public bool OverwriteDestinationFile
        {
            get { return overwriteDestinationFile; }
            set
            {
                overwriteDestinationFile = value;
                NotifyPropertyChanged("OverwriteDestinationFile");
            }
        }
        public bool IsFolderCopy
        {
            get { return isFolderCopy; }
            set
            {
                isFolderCopy = value;
                NotifyPropertyChanged("IsFolderCopy");
            }
        }
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }
        public string FindWhat
        {
            get { return findWhat; }
            set
            {
                findWhat = value;
                NotifyPropertyChanged("FindWhat");
            }
        }
        public string ReplaceWith
        {
            get { return replaceWith; }
            set
            {
                replaceWith = value;
                NotifyPropertyChanged("ReplaceWith");
            }
        }
        public string ComputerList
        {
            get
            {
                return computerList;
            }
            set
            {
                computerList = value;
                NotifyPropertyChanged("ComputerList");
            }
        }
        public bool CanTasksRun
        {
            get { return canTasksRun; }
            set
            {
                canTasksRun = value;
                NotifyPropertyChanged("CanTasksRun");
            }
        }
        public ICommand OnBrowseClickedCommand
        {
            get
            {
                if (onBrowseClickedCommand == null)
                {
                    onBrowseClickedCommand = new RelayCommand(BrowseFile);
                }

                return onBrowseClickedCommand;
            }
        }
        public ICommand OnSchedulerTaskAddedCommand
        {
            get
            {
                if (onTaskAddedCommand == null)
                {
                    onTaskAddedCommand = new RelayCommand(SchedulerTaskAdded);
                }

                return onTaskAddedCommand;
            }
        }
        public ICommand OnCreateTaskCommand
        {
            get
            {
                if (onCreateTaskCommand == null)
                {
                    onCreateTaskCommand = new RelayCommand(CreateTask);
                }

                return onCreateTaskCommand;
            }
        }
        public ICommand OnContentChangedCommand
        {
            get
            {
                if (onContentChangedCommand == null)
                {
                    onContentChangedCommand = new RelayCommand(ChangeContent);
                }

                return onContentChangedCommand;
            }
        }
        public ICommand OnOpenProjectCommand
        {
            get
            {
                if (onOpenProjectCommand == null)
                {
                    onOpenProjectCommand = new RelayCommand(BrowseFile);
                }

                return onOpenProjectCommand;
            }
        }
        public ICommand OnSaveProjectCommand
        {
            get
            {
                if (onSaveProjectCommand == null)
                {
                    onSaveProjectCommand = new RelayCommand(SaveProject);
                }

                return onSaveProjectCommand;
            }
        }
        public ICommand OnRemoveTaskCommand
        {
            get
            {
                if (onRemoveTaskCommand == null)
                {
                    onRemoveTaskCommand = new RelayCommand(RemoveTask);
                }

                return onRemoveTaskCommand;
            }
        }
        public ICommand OnRunAllTasksCommand
        {
            get
            {
                if (onRunAllTasksCommand == null)
                {
                    onRunAllTasksCommand = new RelayCommand(RunAllTasks);
                }

                return onRunAllTasksCommand;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Runs all tasks.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void RunAllTasks(object obj)
        {
            if (File.Exists("ComputerList.txt"))
            {
                computerList = string.Join(Environment.NewLine, File.ReadAllLines("ComputerList.txt"));
            }
            else
                return;
            string relativePath = string.Empty;
            AllLogEntries.Clear();
            UserControl = new LogViewerControl();
            isRunning = true;
            foreach (var task in allTasks)
            {
                switch (task.TaskType)
                {
                    case TaskType.None:
                        break;
                    case TaskType.CopyFile:
                        try
                        {
                            relativePath = GetRelativePath(task.Destination);
                            string folderPath = string.Empty;
                            if (string.IsNullOrEmpty(relativePath))
                                continue;
                            if (task.IsFolderCopy)
                            {
                                folderPath = Path.GetDirectoryName(task.Source);
                            }

                            if (!File.Exists(task.Source))
                            {
                                AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = task.Source + " not found." });
                                continue;
                            }

                            foreach (var computer in computerList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(folderPath))
                                    {
                                        if (!Directory.Exists(folderPath))
                                        {
                                            AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = folderPath + " not found." });
                                            continue;
                                        }

                                        foreach (string file in Directory.GetFiles(folderPath))
                                        {
                                            CopyFile(relativePath, task, computer, file);
                                        }
                                    }

                                    CopyFile(relativePath, task, computer, task.Source);
                                }
                                catch (Exception ex)
                                {
                                    AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = ex.Message });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + ex.StackTrace);
                        }

                        break;
                    case TaskType.FindReplace:
                        relativePath = GetRelativePath(task.FileName);
                        if (string.IsNullOrEmpty(relativePath))
                            continue;
                        foreach (var computer in computerList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            try
                            {
                                if (File.Exists("\\\\" + computer + "\\" + relativePath))
                                {
                                    string allText = File.ReadAllText("\\\\" + computer + "\\" + relativePath);
                                    string changeText = allText.Replace(task.FindWhat, task.ReplaceWith);
                                    File.WriteAllText("\\\\" + computer + "\\" + relativePath, changeText);
                                    AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = string.Format("Replaced {0} to {1} in {2}", task.FindWhat, task.ReplaceWith, "\\\\" + computer + "\\" + relativePath) });
                                }
                            }
                            catch (Exception ex)
                            {
                                AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = ex.Message });
                            }
                        }
                        break;
                    case TaskType.ChangeKeyValue:
                        break;
                    case TaskType.RestartJobService:
                        Helper.RestartJobScheduler();
                        break;
                    case TaskType.RemoteComputer:
                        break;
                    default:
                        break;
                }
            }

            isRunning = false;
        }
        /// <summary>
        /// Copies the file.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="task">The task.</param>
        /// <param name="computer">The computer.</param>
        /// <param name="file">The file.</param>
        private void CopyFile(string relativePath, MigrationTask task, string computer, string file)
        {
            string fileName = Path.GetFileName(file);
            if (!task.OverwriteDestinationFile)
            {
                string originalFileName = Path.GetFileNameWithoutExtension(file);
                fileName = fileName.Replace(originalFileName, originalFileName + ".old");
                string newNamePath = relativePath + "\\" + fileName;
                if (File.Exists("\\\\" + computer + "\\" + relativePath + "\\" + Path.GetFileName(file)))
                {
                    if (File.Exists("\\\\" + computer + "\\" + newNamePath))
                        File.Delete("\\\\" + computer + "\\" + newNamePath);
                    File.Move("\\\\" + computer + "\\" + relativePath + "\\" + Path.GetFileName(file), "\\\\" + computer + "\\" + newNamePath);
                    AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = "Renamed original file " + "\\\\" + computer + "\\" + relativePath + "\\" + Path.GetFileName(file) });
                }
            }

            File.Delete("\\\\" + computer + "\\" + relativePath + "\\" + Path.GetFileName(file));
            File.Copy(task.Source, "\\\\" + computer + "\\" + relativePath + "\\" + Path.GetFileName(file), true);
            AllLogEntries.Add(new LogEntry() { DateTime = DateTime.Now.ToString(), Message = string.Format("Copied {0} to {1}", task.Source, "\\\\" + computer + "\\" + relativePath + "\\" + Path.GetFileName(file)) });
        }
        /// <summary>
        /// Gets the relative path.
        /// </summary>
        /// <param name="originalPath">The original path.</param>
        /// <returns></returns>
        private string GetRelativePath(string originalPath)
        {
            string pathRoot = Path.GetPathRoot(originalPath);
            if (string.IsNullOrEmpty(pathRoot))
                return string.Empty;
            return originalPath.Replace(pathRoot, pathRoot.Replace(':', '$'));
        }
        /// <summary>
        /// Removes the task.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void RemoveTask(object obj)
        {
            if (activeTask != null)
            {
                AllTasks.Remove(activeTask);
                CheckRunStatus();
                ProjectTitle = "Migration Copy Tool *";
            }
        }
        /// <summary>
        /// Saves the project.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void SaveProject(object obj)
        {
            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MigrationTask>));
            serializer.Serialize(xmlWriter, allTasks);
            string xmlResult = stringWriter.ToString();
            if (string.IsNullOrEmpty(projectSavePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "xml";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.Filter = "XML Files|*.xml";
                if (saveFileDialog.ShowDialog() == true)
                {
                    projectSavePath = saveFileDialog.FileName;
                }
            }

            File.WriteAllText(projectSavePath, xmlResult);
            ProjectTitle = "Migration Copy Tool";
        }
        /// <summary>
        /// Changes the content.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ChangeContent(object obj)
        {
            if (isRunning)
                return;
            destinationSelectedPath = null;
            fileName = null;
            findWhat = null;
            isFolderCopy = false;
            overwriteDestinationFile = false;
            replaceWith = null;
            sourceSelectedPath = null;
            taskTitle = null;
            switch (obj.ToString())
            {
                default:
                    break;
                case "Copy File":
                    UserControl = new CopyFileControl();
                    MigrationTaskType = TaskType.CopyFile;
                    break;
                case "Find & Replace":
                    UserControl = new FindAndReplace();
                    MigrationTaskType = TaskType.FindReplace;
                    break;
                case "Remote Computers":
                    UserControl = new ComputerList();
                    if (File.Exists("ComputerList.txt"))
                    {
                        ComputerList = string.Join(Environment.NewLine, File.ReadAllLines("ComputerList.txt"));
                    }
                    MigrationTaskType = TaskType.RemoteComputer;
                    break;
                case "About":
                    MessageBox.Show("\u00A9 Mayank Kumar. \r\n All Rights Reserved.");
                    break;
                case "Cancel":
                    userControl = null;
                    break;
            }
        }
        /// <summary>
        /// Creates the task.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void CreateTask(object obj)
        {
            MigrationTask task = new MigrationTask();
            switch (MigrationTaskType)
            {
                case TaskType.CopyFile:
                case TaskType.FindReplace:
                case TaskType.ChangeKeyValue:
                    task.Destination = destinationSelectedPath;
                    task.FileName = fileName;
                    task.FindWhat = findWhat;
                    task.IsFolderCopy = isFolderCopy;
                    task.OverwriteDestinationFile = overwriteDestinationFile;
                    task.ReplaceWith = replaceWith;
                    task.Source = sourceSelectedPath;
                    task.TaskTitle = taskTitle;
                    task.TaskType = migrationTaskType;
                    allTasks.Add(task);
                    ProjectTitle = "Migration Copy Tool *";
                    CheckRunStatus();
                    break;
                case TaskType.RemoteComputer:
                    File.WriteAllLines("ComputerList.txt", computerList.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                    break;
            }


        }
        /// <summary>
        /// Browses the file.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void BrowseFile(object obj)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fileDialog.Multiselect = false;
            if (obj.ToString().Equals("OpenFile"))
            {
                fileDialog.Filter = "XML Files|*.xml";
            }

            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;
                if (obj.ToString().Equals("OpenFile"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<MigrationTask>));
                    StreamReader reader = new StreamReader(filePath);
                    var tasks = (ObservableCollection<MigrationTask>)serializer.Deserialize(reader);
                    reader.Close();
                    allTasks.Clear();
                    foreach (var task in tasks)
                    {
                        AllTasks.Add(task);
                    }
                    CheckRunStatus();
                    ProjectTitle = "Migration Copy Tool";
                    projectSavePath = filePath;
                }
            }
        }
        /// <summary>
        /// Checks the run status.
        /// </summary>
        private void CheckRunStatus()
        {
            if (AllTasks.Count > 0)
                CanTasksRun = true;
            else
                CanTasksRun = false;
        }
        /// <summary>
        /// Schedulers the task added.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void SchedulerTaskAdded(object param)
        {
            MigrationTask task = new MigrationTask();
            task.TaskTitle = "Restart Scheduler Service";
            task.TaskType = TaskType.RestartJobService;
            allTasks.Add(task);
            ProjectTitle = "Migration Copy Tool *";
            CheckRunStatus();
        }
        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion
    }
}
