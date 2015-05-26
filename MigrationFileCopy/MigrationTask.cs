namespace MigrationFileCopy
{
    /// <summary>
    /// Migration Task class
    /// </summary>
    public class MigrationTask
    {
        /// <summary>
        /// Gets or sets the task title.
        /// </summary>
        /// <value>
        /// The task title.
        /// </value>
        public string TaskTitle { get; set; }
        /// <summary>
        /// Gets or sets the type of the task.
        /// </summary>
        /// <value>
        /// The type of the task.
        /// </value>
        public TaskType TaskType { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }
        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <value>
        /// The destination.
        /// </value>
        public string Destination { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether [overwrite destination file].
        /// </summary>
        /// <value>
        /// <c>true</c> if [overwrite destination file]; otherwise, <c>false</c>.
        /// </value>
        public bool OverwriteDestinationFile { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is folder copy.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is folder copy; otherwise, <c>false</c>.
        /// </value>
        public bool IsFolderCopy { get; set; }
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }
        /// <summary>
        /// Gets or sets the find what.
        /// </summary>
        /// <value>
        /// The find what.
        /// </value>
        public string FindWhat { get; set; }
        /// <summary>
        /// Gets or sets the replace with.
        /// </summary>
        /// <value>
        /// The replace with.
        /// </value>
        public string ReplaceWith { get; set; }
    }

    /// <summary>
    /// Task Type Enumerator
    /// </summary>
    public enum TaskType
    {
        /// <summary>
        /// No Task
        /// </summary>
        None,
        /// <summary>
        /// The copy file
        /// </summary>
        CopyFile,
        /// <summary>
        /// The delete file or folder
        /// </summary>
        DeleteFileOrFolder,
        /// <summary>
        /// The find replace
        /// </summary>
        FindReplace,
        /// <summary>
        /// The change key value
        /// </summary>
        ChangeKeyValue,
        /// <summary>
        /// The restart job service
        /// </summary>
        RestartJobService,
        /// <summary>
        /// The remote computer
        /// </summary>
        RemoteComputer,
        /// <summary>
        /// The start process
        /// </summary>
        StartProcess
    }
}
