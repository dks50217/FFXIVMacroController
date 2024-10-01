using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroControllerApp.Model
{
    public class GithubVersionModel
    {
        public required string Version { get; set; }
        public required string DownloadURL { get; set; }
    }
}
