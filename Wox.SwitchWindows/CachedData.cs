using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wox.SwitchWindows
{
    public readonly struct CachedData
    {
        public readonly string Lowered;
        public readonly string Original;
        public readonly IntPtr Handle;
        public readonly string ImagePath;

        public CachedData(string lowered, string original, IntPtr handle, string imagePath)
        {
            Lowered = lowered;
            Original = original;
            Handle = handle;
            ImagePath = imagePath;
        }
    }
}
