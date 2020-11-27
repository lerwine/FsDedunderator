using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace FsDedunderator
{
    public class CrawlJob
    {
        public static Regex TrueFalseRegex = new Regex(@"^\s*(?:(t(?:rue)|?y(?:es)?|0*[1-9]\d*)|f(?:alse)?|no?|0+)?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FileInfo OutputFile { get; set; }

        public Collection<DirectoryInfo> SourceDirectories { get; private set; }

        public string Name { get; set; }

        public bool Overwrite { get; private set; }

        public bool IgnoreNotFound { get; private set; }

        public CrawlResult Result { get; private set; }

        private CrawlJob()
        {
            Result = new CrawlResult();
            SourceDirectories = new Collection<DirectoryInfo>();
            Overwrite = IgnoreNotFound = false;
        }

        internal static CrawlJob Load(string path)
        {
            CrawlJob result = new CrawlJob();
            StreamReader streamReader;
            try
            {

                if (string.IsNullOrWhiteSpace(path))
                {
                    result.Result.ErrorMessage = "Crawl job settings file path is empty.";
                    result.Result.ExitCode = ExitCodes.InvalidSettingsFilePath;
                    return result;
                }
                if (Directory.Exists(path))
                {
                    result.Result.ErrorMessage = "Path '" + path + "' does not refer to a crawl job settings file.";
                    result.Result.ExitCode = ExitCodes.SettingsPathNotAFile;
                    return result;
                }
                if (File.Exists(path))
                    streamReader = new StreamReader(path);
                else
                {
                    result.Result.ErrorMessage = "Crawl job settings file '" + path + "' not found.";
                    result.Result.ExitCode = ExitCodes.SettingsPathNotFound;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.Result.ErrorMessage = "Error while trying to access crawl job settings file '" + path + "': " + exc.ToString();
                result.Result.ExitCode = ExitCodes.SettingsAccessError;
                return result;
            }
            XmlDocument document = new XmlDocument();
            try
            {
                using (streamReader)
                {
                    using (XmlReader reader = XmlReader.Create(streamReader))
                    {
                        document.Load(reader);
                    }
                }
            }
            catch (Exception exc)
            {
                result.Result.ErrorMessage = "Error while trying to parse XML in crawl job settings file '" + path + "': " + exc.ToString();
                result.Result.ExitCode = ExitCodes.InvalidSettingsFileFormat;
                return result;
            }
            XmlElement crawlJobElement = (XmlElement)document.SelectSingleNode("/CrawlJob");
            if (null == crawlJobElement)
            {
                result.Result.ErrorMessage = "Error while trying to parse XML in crawl job settings file '" + path + "': Expected document element not found.";
                result.Result.ExitCode = ExitCodes.InvalidSettingsFileFormat;
                return result;
            }

            XmlAttribute attr = (XmlAttribute)crawlJobElement.SelectSingleNode("@name");
            if (null != attr)
                result.Name = attr.Value.Trim();

            attr = (XmlAttribute)crawlJobElement.SelectSingleNode("@output");
            if (null == attr || string.IsNullOrWhiteSpace(attr.Value))
            {
                result.Result.ErrorMessage = "Error parsing 'output' attribute value '" + attr.Value + "' in '" + path + "': No path specified.";
                result.Result.ExitCode = ExitCodes.OutputPathNotSpecified;
                return result;
            }
            try
            {
                if (Directory.Exists(attr.Value))
                {
                    result.Result.ErrorMessage = "Error validating 'output' attribute value '" + attr.Value + "' in '" + path + "': Path does not refer to a file.";
                    result.Result.ExitCode = ExitCodes.OutputPathNotAFile;
                    return result;
                }
                if ((result.OutputFile = new FileInfo(attr.Value)).Exists)
                {
                    if (!result.Overwrite)
                    {
                        result.OutputFile = null;
                        result.Result.ErrorMessage = "Error validating 'output' attribute value '" + attr.Value + "' in '" + path + "': File already exists and 'overwrite' settings attribute was not set to true.";
                        result.Result.ExitCode = ExitCodes.OutputDirectoryNotFound;
                        return result;
                    }
                }
                else if (null == result.OutputFile.Directory || !result.OutputFile.Exists)
                {
                    result.OutputFile = null;
                    result.Result.ErrorMessage = "Error validating 'output' attribute value '" + attr.Value + "' in '" + path + "': Parent directory does not exist.";
                    result.Result.ExitCode = ExitCodes.OutputDirectoryNotFound;
                    return result;
                }
            }
            catch (Exception exc)
            {
                result.OutputFile = null;
                result.Result.ErrorMessage = "Error validating 'output' attribute value '" + attr.Value + "' in '" + path + "': " + exc.ToString();
                result.Result.ExitCode = ExitCodes.InvalidSettingsFileFormat;
                return result;
            }

            result.Result.JobName = result.Name;

            attr = (XmlAttribute)crawlJobElement.SelectSingleNode("@overwrite");
            if (null == attr)
                result.Overwrite = false;
            else
            {
                Match match = TrueFalseRegex.Match(attr.Value);
                if (match.Success)
                    result.Overwrite = match.Groups[1].Success;
                else
                {
                    result.Result.ErrorMessage = "Error parsing 'overwrite' attribute value '" + attr.Value + "' in '" + path + "': Not a valid bool value";
                    result.Result.ExitCode = ExitCodes.InvalidOverwriteAttribute;
                    return result;
                }
            }
            attr = (XmlAttribute)crawlJobElement.SelectSingleNode("@ignoreNotFound");
            if (null == attr)
                result.IgnoreNotFound = false;
            else
            {
                Match match = TrueFalseRegex.Match(attr.Value);
                if (match.Success)
                    result.IgnoreNotFound = match.Groups[1].Success;
                else
                {
                    result.Result.ErrorMessage = "Error parsing 'ignoreNotFound' attribute value '" + attr.Value + "' in '" + path + "': Not a valid bool value";
                    result.Result.ExitCode = ExitCodes.InvalidIgnoreNotFoundAttribute;
                    return result;
                }
            }

            foreach (XmlElement element in crawlJobElement.SelectNodes("path"))
            {
                string p = (element.IsEmpty) ? "" : element.InnerText;
                if (string.IsNullOrWhiteSpace(p))
                {
                    result.Result.ErrorMessage = "Error validating 'path' element value in '" + path + "': No path specified.";
                    result.Result.ExitCode = ExitCodes.OutputDirectoryNotFound;
                    return result;
                }
                try
                {
                    if (File.Exists(p))
                    {
                        result.Result.ErrorMessage = "Error validating 'path' element value '" + p + "' in '" + path + "': Path does not refer to a subdirectory.";
                        result.Result.ExitCode = ExitCodes.SourcePathNotADirectory;
                        return result;
                    }
                    if (!Directory.Exists(p))
                    {
                        String s = p.Trim();
                        if (s.Length == p.Length)
                            p = "";
                        else
                        {
                            if (File.Exists(p))
                            {
                                result.Result.ErrorMessage = "Error validating 'path' element value '" + p + "' in '" + path + "': Path does not refer to a subdirectory.";
                                result.Result.ExitCode = ExitCodes.SourcePathNotADirectory;
                                return result;
                            }
                            if (!Directory.Exists(p))
                                p = "";
                        }
                    }
                    if (p.Length > 0)
                        result.SourceDirectories.Add(new DirectoryInfo(p));
                    else if (!result.IgnoreNotFound)
                    {
                        result.Result.ErrorMessage = "Error validating 'path' element value '" + element.InnerText.Trim() + "' in '" + path + "': Subdirectory not found.";
                        result.Result.ExitCode = ExitCodes.SourcePathNotFound;
                        return result;
                    }
                }
                catch (Exception exc)
                {
                    result.Result.ErrorMessage = "Error validating 'path' element value '" + p + "' in '" + path + "': " + exc.ToString();
                    result.Result.ExitCode = ExitCodes.InvalidSettingsFileFormat;
                    return result;
                }
            }
            result.Result.ExitCode = ExitCodes.Success;
            return result;
        }
    }
}