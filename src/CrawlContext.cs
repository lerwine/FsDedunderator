using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FsDedunderator
{
    public class CrawlContext
    {
        private readonly string jobName;
        private readonly string basePath;
        private readonly int position;
        private readonly int total;
        private DirectoryInfo directoryInfo;
        private DateTime emitMessageAfter = DateTime.MinValue;

        public CrawlContext(String jobName, CrawlContext parentContext, int position, int total, DirectoryInfo directoryInfo)
        {
            this.jobName = jobName;
            basePath = (null == parentContext) ? "" : ((parentContext.basePath.Length == 0) ? directoryInfo.Name : Path.Combine(parentContext.basePath, directoryInfo.Name));
            this.directoryInfo = directoryInfo;
            this.position = position;
            this.total = total;
        }

        internal ResultDirectory Run() {
            ResultDirectory resultDirectory = new ResultDirectory(directoryInfo.Name);
            DirectoryInfo[] childDirectories;
            
            if (DateTime.Now.CompareTo(emitMessageAfter) >= 0) {
                Console.WriteLine(jobName + ": Reading " + directoryInfo.Parent.FullName + " =>");
                Console.WriteLine("\t" + directoryInfo.Name + ": " + position.ToString() + " of " + total.ToString());
                emitMessageAfter = DateTime.Now.AddSeconds(1);
            }
            try
            {
                childDirectories = directoryInfo.GetDirectories();
            }
            catch (Exception exc)
            {
                resultDirectory.ErrorMessage = "Error enumerating child subdirectories: " + exc.ToString();
                childDirectories = new DirectoryInfo[0];
            }
            FileInfo[] files;
            try
            {
                files = directoryInfo.GetFiles();
            }
            catch (Exception exc)
            {
                if (null == resultDirectory.ErrorMessage)
                    resultDirectory.ErrorMessage = "Error enumerating files: " + exc.ToString();
                else
                    resultDirectory.ErrorMessage = "Error enumerating subdirectory contents: " + exc.ToString();
                files = new FileInfo[0];
            }
            if (null != resultDirectory.ErrorMessage)
                Console.Error.WriteLine(jobName + " [.\\" + basePath + "]: " + resultDirectory.ErrorMessage);
            Collection<CrawlContext> contexts = new Collection<CrawlContext>();
            for (int i = 0; i < childDirectories.Length; i++) {
                contexts.Add(new CrawlContext(jobName, this, i + 1, childDirectories.Length, childDirectories[i]));
            }
            foreach (FileInfo f in files)
            {
                resultDirectory.ChildNodes.Add(new ResultFile(f));
            }
            foreach (CrawlContext c in contexts) {
                c.emitMessageAfter = emitMessageAfter;
                resultDirectory.ChildNodes.Add(c.Run());
                emitMessageAfter = c.emitMessageAfter;
            }
            return resultDirectory;
        }
    }
}