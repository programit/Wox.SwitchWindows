using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Wox.Plugin;

namespace Wox.SwitchWindows
{
    public class WindowSwitcher : IPlugin
    {
        private List<CachedData> processes = new List<CachedData>(0);
        private DateTime rebuildTime = DateTime.UtcNow;
        private ImgCache imgCache;
        public void Init(PluginInitContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}

            this.imgCache = new ImgCache(context.CurrentPluginMetadata.PluginDirectory);
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    Build();
                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            });
        }

        private void Build()
        {
            this.rebuildTime = DateTime.UtcNow.AddSeconds(10);
            Process[] processlist = Process.GetProcesses();
            List<CachedData> results = new List<CachedData>(32);
            foreach (Process process in processlist)
            {
                try
                {
                    if (!string.IsNullOrEmpty(process.MainWindowTitle))
                    {
                        string imgPath = ImgCache.DefaultResult;
                        if (process.MainModule?.FileName != null)
                        {
                            imgPath = this.imgCache.GetOrAdd(process?.MainModule?.FileName);
                        }

                        results.Add(new CachedData(process.MainWindowTitle.ToLower(), process.MainWindowTitle, process.MainWindowHandle, imgPath));
                    }
                }
                catch (Exception) { }
            }

            this.processes = results;
        }

        public List<Result> Query(Query query)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Process[] processlist = Process.GetProcesses();
            List<Result> results = new List<Result>(32);

            if (DateTime.UtcNow > rebuildTime)
            {
                Task.Factory.StartNew(Build);
            }

            foreach (var item in this.processes)
            {
                if (item.Lowered.Contains(query.Search.ToLower()))
                {
                    results.Add(new Result()
                    {
                        Title = item.Original,
                        Action = ctx => SwitchToWindow(ctx, item.Handle),
                        IcoPath = item.ImagePath
                    });
                }
            }

            return results;
        }

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd);

        private bool SwitchToWindow(ActionContext ctx, IntPtr handle)
        {
            SwitchToThisWindow(handle);
            return true;
        }
    }
}