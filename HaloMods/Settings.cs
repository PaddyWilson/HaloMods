using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloMods
{
	public static class Settings
	{
		public static string MCCLocation;
		public static string ModsLocation;

		public static string SettingsSaveFile = "settings.txt";
		public static string HaloReachSaveFile = "HaloReach.json";

		public static string exeLocation { get { return MCCLocation + @"\mcclauncher.exe"; } }
		public static string PakLocation { get { return MCCLocation + @"\mcc\content\paks\MCC-WindowsNoEditor.pak"; } }
		public static string StartupMovieLocation { get { return MCCLocation + @"\mcc\content\movies\FMS_logo_microsoft_7_1_.bk2"; } }

		#region HaloReach
		/// <summary>
		/// Location of the original maps		MCCLocation + @"\haloreach\maps"
		/// </summary>
		public static string HR_OriginalMapLocation { get { return MCCLocation + @"\haloreach\maps"; } }
		/// <summary>
		/// Location of Vanilla backup maps		ModsLocation + @"\Vanilla\Halo Reach\Maps"
		/// </summary>
		public static string HR_VanillaMapLocation { get { return ModsLocation + @"\Vanilla\Halo Reach\Maps"; } }
		/// <summary>
		/// Location of modded maps		ModsLocation + @"\Mods\Halo Reach\Maps"
		/// </summary>
		public static string HR_ModdedMapLocation { get { return ModsLocation + @"\Mods\Halo Reach\Maps"; } }
		#endregion

		public static bool ReadSettings()
		{
			if (File.Exists(SettingsSaveFile))
			{
				string[] settings = File.ReadAllLines(SettingsSaveFile);

				foreach (var item in settings)
				{
					if (item.StartsWith("#") || item.Length == 0)
						continue;

					string[] split = item.Split('=');

					if (split[0] == "install-folder")
						MCCLocation = split[1];
					else if (split[0] == "mods-folder")
						ModsLocation = split[1];
					else
						continue;
				}
				return true;
			}
			return false;
		}

		public static bool SaveSettings()
		{
			File.Delete(SettingsSaveFile);
			string output = 
				"install-folder=" + MCCLocation + "\r\n" +
				"mods-folder=" + ModsLocation;
			File.WriteAllText(SettingsSaveFile, output);
			return true;
		}

		public static bool CreateSettingsFile()
		{
			if (!File.Exists(SettingsSaveFile))
			{
				Console.WriteLine("Init setup running.");
				//make settings file
				File.WriteAllText(SettingsSaveFile, Properties.Resources.settings);
				return true;
			}

			return false;
		}
	}
}
