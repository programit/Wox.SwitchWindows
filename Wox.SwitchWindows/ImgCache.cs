using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Wox.SwitchWindows
{
    public class ImgCache
    {
        private ConcurrentDictionary<string, string> imgCacheLookup = new ConcurrentDictionary<string, string>();
        private readonly string workingDir;
        public const string DefaultResult = "Images\\windowSwitcher.png";
        public ImgCache(string workingDir)
        {
            this.workingDir = workingDir;
        }

        public string GetOrAdd(string fileName)
        {
            if (imgCacheLookup.TryGetValue(fileName, out string imgPath) && File.Exists(imgPath))
            {
                return imgPath;
            }

            lock (this)
            {
                // Double Check lock
                if (imgCacheLookup.TryGetValue(fileName, out imgPath) && File.Exists(imgPath))
                {
                    return imgPath;
                }

                try
                {
                    Icon img = Icon.ExtractAssociatedIcon(fileName);
                    string fullPath = Path.Combine(this.workingDir, "Images", "QueryResults", Path.GetFileNameWithoutExtension(fileName)) + ".png";
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    using (FileStream fs = File.OpenWrite(fullPath))
                    {
                        img.ToBitmap().Save(fs, ImageFormat.Png);
                    }

                    this.imgCacheLookup[fileName] = fullPath;
                    return fullPath;
                }
                catch (Exception)
                {
                }

                this.imgCacheLookup[fileName] = DefaultResult;
                return DefaultResult;
            }
        }
    }
}
