using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FsDedunderator
{
    static class Program
    {
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            // Collection<CrawlJob> crawlJobCollection = new Collection<CrawlJob>();
            // ExitCodes? exitCode = null;
            // for (int i = 0; i < args.Length; i++)
            // {
            //     CrawlJob job = CrawlJob.Load(args[i]);
            //     string baseName = job.Name;
            //     int idx = i;
            //     if (null == baseName || (baseName = baseName.Trim()).Length == 0)
            //     {
            //         baseName = "CrawlJob";
            //     }
            //     else if (!crawlJobCollection.Any((j) => j.Name.Equals(job.Name, StringComparison.InvariantCultureIgnoreCase)))
            //         baseName = null;
            //     else
            //         idx = 0;
            //     if (null != baseName)
            //     {
            //         do
            //         {
            //             job.Name = baseName + " #" + (++idx).ToString();
            //         } while (crawlJobCollection.Any((j) => j.Name.Equals(job.Name, StringComparison.InvariantCultureIgnoreCase)));
            //     }
            //     if (job.Result.ExitCode != ExitCodes.Success)
            //     {
            //         if (!exitCode.HasValue)
            //             exitCode = job.Result.ExitCode;
            //         Console.Error.WriteLine(job.Name + ": " + job.Result.ErrorMessage);
            //         if (null != job.OutputFile)
            //             crawlJobCollection.Add(job);
            //     }
            //     else
            //         crawlJobCollection.Add(job);
            // }
            // if (crawlJobCollection.Count == 0)
            // {
            //     if (exitCode.HasValue)
            //         Environment.ExitCode = (int)exitCode.Value;
            //     else
            //         Console.WriteLine("Nothing to do...");
            //     return;
            // }


            // foreach (CrawlJob crawlJob in crawlJobCollection)
            // {
            //     CrawlResult crawlResult = crawlJob.Result;
            //     if (crawlResult.ExitCode == ExitCodes.Success) {
            //         for (int i = 0; i < crawlJob.SourceDirectories.Count; i++) {
            //             CrawlContext context = new CrawlContext(crawlJob.Name, null, i + 1, crawlJob.SourceDirectories.Count, crawlJob.SourceDirectories[i]);
            //             crawlResult.Nodes.Add(context.Run());
            //         }
            //         if (crawlResult.ExitCode != ExitCodes.Success) {
            //             if (!exitCode.HasValue)
            //                 exitCode = crawlResult.ExitCode;
            //             Console.Error.WriteLine(crawlJob.Name + ": " + crawlResult.ErrorMessage);
            //         }
            //     }
            //     if (!crawlJob.Overwrite)
            //     {
            //         crawlJob.OutputFile.Refresh();
            //         if (crawlJob.OutputFile.Exists)
            //         {
            //             Console.Error.WriteLine(crawlJob.Name + ": Cannot save results to file '" + crawlJob.OutputFile.FullName + "'. Path exists and the overwrite settings attribute is not set to true");
            //             continue;
            //         }
            //     }
            //     Console.Write("Saving results to '" + crawlJob.OutputFile.FullName + "'...");
            //     try
            //     {
            //         XmlSerializer ser = new XmlSerializer(crawlResult.GetType());
            //         ser.Serialize(Console.Out, crawlResult);
            //         Console.WriteLine("ok");
            //     }
            //     catch (Exception exc)
            //     {
            //         Console.Error.WriteLine("failed.");
            //         Console.Error.WriteLine(crawlJob.Name + ": Error saving '" + crawlJob.OutputFile.FullName + "': " + exc.ToString());
            //     }
            // }
        }
    }
}
