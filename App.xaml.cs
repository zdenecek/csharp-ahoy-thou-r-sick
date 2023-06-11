using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AhoCorasick
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();


            string needle = e.Args[0];
            string path = e.Args[1];
            int crawlerNum = int.Parse(e.Args[2]);
            int workerNum = int.Parse(e.Args[3]);
            int queSize = int.Parse(e.Args[4]);

            if (workerNum <= 0)
            {
                Console.Error.WriteLine("Invalid worker count");
                return;
            }

            if (crawlerNum != 1)
            {
                Console.Error.WriteLine("Invalid crawler count. Only 1 supported");
                return;
            }

            if (queSize <= 0)
            {
                Console.Error.WriteLine("Invalid que size");
                return;
            }



            var searcher = new FileSearcher(workerNum, queSize);

            Func<string> getStatus = () => $"Files searched: {searcher.ProcessedFilesCount}, Files found: {searcher.FoundFiles.Count}";

            searcher.OnFileFound += file =>
            {
                wnd.AddFile(file);
                wnd.SetStatus(getStatus());
            };

            searcher.OnSearchComplete += () =>
            {
                wnd.SetStatus($"Search complete! " + getStatus());
            };

            Thread worker = new Thread(() => searcher.Search(path, needle));
            worker.Start();

            wnd.Show();
        }
    }
}
