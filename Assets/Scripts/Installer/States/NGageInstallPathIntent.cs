using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDEngine.Installer.States
{
    public class NGageInstallPathIntent
    {
        public string Path;

        public NGageInstallPathIntent(string path)
        {
            Path = path;
        }
    }
}
