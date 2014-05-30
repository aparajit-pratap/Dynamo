using System;
using System.IO;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Nodes
{
    public abstract class FileReaderBase : NodeModel
    {
        readonly FileSystemEventHandler handler;

        string path;
        protected string storedPath
        {
            get { return path; }
            set
            {
                if (value != null && !value.Equals(path))
                {
                    if (watch != null)
                        watch.FileChanged -= handler;

                    path = value;
                    watch = new FileWatch(path);
                    watch.FileChanged += handler;
                }
            }
        }

        FileWatch watch;

        protected FileReaderBase()
        {
            handler = watcher_FileChanged;
            InPortData.Add(new PortData("path", "Path to the file"));
        }

        void watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!dynSettings.Controller.Running)
                RequiresRecalc = true;
            else
            {
                //TODO: Refactor
                DisableReporting();
                RequiresRecalc = true;
                EnableReporting();
            }
        }
    }

    public class FileWatch : IDisposable
    {
        public bool Changed { get; private set; }

        private readonly FileSystemWatcher watcher;
        private readonly FileSystemEventHandler handler;

        public event FileSystemEventHandler FileChanged;

        //public static FileWatch FileWatcher(string fileName)
        //{
        //    FileWatch fw = new FileWatch(fileName);
        //    //fw.FileChanged += fileWatcherChanged;
        //    return fw;
        //}

        //public static bool FileWatcherChanged(FileWatch fileWatcher)
        //{
        //    return fileWatcher.Changed;
        //}

        //public static bool FileWatcherWait(FileWatch fileWatcher, int limit)
        //{
        //    var watcher = fileWatcher;
        //    double timeout = limit;

        //    timeout = timeout == 0 ? double.PositiveInfinity : timeout;

        //    int tick = 0;
        //    while (!watcher.Changed)
        //    {
        //        //if (dynSettings.Controller.RunCancelled)
        //        //    throw new Exception("Run Cancelled");

        //        System.Threading.Thread.Sleep(10);
        //        tick += 10;

        //        if (tick >= timeout)
        //        {
        //            throw new Exception("File watcher timeout!");
        //        }
        //    }

        //    return true;
        //}

        //public static FileWatch FileWatcherReset(FileWatch fileWatcher)
        //{
        //    fileWatcher.Reset();
        //    return fileWatcher;
        //}

        public FileWatch(string filePath)
        {
            Changed = false;

            var dir = Path.GetDirectoryName(filePath);

            if (string.IsNullOrEmpty(dir))
                dir = ".";

            var name = Path.GetFileName(filePath);

            watcher = new FileSystemWatcher(dir, name)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            handler = watcher_Changed;
            watcher.Changed += handler;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Changed = true;
            if (FileChanged != null)
                FileChanged(this, e);
        }

        public void Reset()
        {
            Changed = false;
        }

        #region IDisposable Members

        public void Dispose()
        {
            watcher.Changed -= handler;
            watcher.Dispose();
        }

        #endregion
    }
}
