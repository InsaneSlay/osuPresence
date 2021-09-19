using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Presence.Win
{
    public static class osu_
    {
        public static string GetOsuRunningDirectory()
        {
            var processes = Process.GetProcessesByName("osu!");
            if (processes.Length > 0)
            {
                string dir = processes[0].Modules[0].FileName;
                dir = dir.Remove(dir.LastIndexOf('\\'));
                return dir;
            }
            return string.Empty;
        }
    }
}
