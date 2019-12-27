using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//fsutil.exe hardlink list "D:\- Halo\Clean\haloreach\maps\forge_halo.map"
namespace HaloMods
{
	class Program
	{
		static void Main(string[] args)
		{
			//FileUtil.GetPathsOfHardLinkedFile(@"C:\Users\GGGGG\Desktop\HaloMCC Install\mcclauncher.exe");
			//FileUtil.OpenFileExplorer(@"C:\Users\GGGGG\Desktop\HaloMCC Install");

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new HaloMods());
			return;
		}
	}
}
