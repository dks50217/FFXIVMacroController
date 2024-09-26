using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroControllerWpfApp.Model
{
    public class GithubVersionModel
    {
        public required Version Version { get; set; }
        public required string DownloadURL { get; set; }
    }
}
