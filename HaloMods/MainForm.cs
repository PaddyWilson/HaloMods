using HaloMods.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaloMods
{
	public partial class HaloMods : Form
	{	
		SwapData ForgeFileSwap;

		GameData HaloReach;

		Dictionary<string, SwapData> TempSwap = new Dictionary<string, SwapData>();

		public HaloMods()
		{
			InitializeComponent();

			Settings.CreateSettingsFile();
			Settings.ReadSettings();

			Logger.LogHandle += LogEvent;
			Logger.LogLineHandle += LogLineEvent;

			txtMCCLocation.Text = Settings.MCCLocation;
			txtModsLocation.Text = Settings.ModsLocation;

			Setup();
		}

		private void DisableButtonControls(Control con)
		{
			foreach (Control c in con.Controls)
			{
				DisableButtonControls(c);
			}

			var btn = con as Button;
			if (btn != null)
			{
				if (btn.Text != "Locate")
					btn.Enabled = false;
			}
			//con.Enabled = false;
		}

		private void EnableButtonControls(Control con)
		{
			foreach (Control c in con.Controls)
			{
				EnableButtonControls(c);
			}
			var btn = con as Button;
			if (btn != null)
			{
				btn.Enabled = true;
			}
		}

		private void Setup()
		{
			//Logger.Logger.Logger.Logger.Log("things");

			bool installed = CheckInstallMCCLocation();

			if (!installed)
			{
				Logger.LogLine("MCC not installed at");
				Logger.LogLine(Settings.MCCLocation);
				Logger.LogLine("please change it in settings.");

				//disable all buttons
				DisableButtonControls(this);
				return;
			}
			else
			{
				Logger.LogLine("MCC is installed. " + Settings.MCCLocation);

			}
			EnableButtonControls(this);
			Logger.LogLine("Mods is installed. " + Settings.MCCLocation);

			//create mods folders
			Directory.CreateDirectory(Settings.HR_ModdedMapLocation);
			Directory.CreateDirectory(Settings.HR_VanillaMapLocation);

			//general tab setup
			//start up video
			if (File.Exists(Settings.StartupMovieLocation))
				btnStartupVideo.Text = "Disable Startup Video";
			else
				btnStartupVideo.Text = "Enable Startup Video";

			//enable forge swap
			ForgeFileSwap = new SwapData(Settings.PakLocation,
				Settings.ModsLocation + "\\Vanilla\\" + FileUtil.GetFileName(Settings.PakLocation),
				Settings.ModsLocation + "\\Mods\\" + FileUtil.GetFileName(Settings.PakLocation));

			if (File.Exists(ForgeFileSwap.ModdedFilePath) && File.Exists(ForgeFileSwap.NewFilePath))
				btnForge.Enabled = false;//forge has already been setup

			//halo reach tab setup
			HaloReach = new GameData("HaloReach.json", Settings.HR_OriginalMapLocation, Settings.HR_VanillaMapLocation, Settings.HR_ModdedMapLocation);
			HaloReach.LoadSwapData();

			//populate the vanilla list
			lstHRVanillaMaps.Items.Clear();
			foreach (var item in HaloReach.VanillaMaps)
				lstHRVanillaMaps.Items.Add(item.Key);
			//select first item
			if (HaloReach.VanillaMaps.Count > 0)
				lstHRVanillaMaps.SelectedIndex = 0;

			//populate the mods list
			lstHRModdedMaps.Items.Clear();
			foreach (var item in HaloReach.ModdedMaps)
				lstHRModdedMaps.Items.Add(item.Key);
			//select first item
			if (HaloReach.ModdedMaps.Count > 0)
				lstHRModdedMaps.SelectedIndex = 0;

			UpdateSwapList();
		}

		#region VanillaAndModdedButton

		private void btnVanilla_Click(object sender, EventArgs e)
		{
			btnVanilla.Enabled = false;

			if (!CheckInstallMCCLocation())
			{
				Logger.LogLine("MCC cannot be located to swap files.");
				btnVanilla.Enabled = true;
				return;
			}

			//swap pak file
			if (File.Exists(ForgeFileSwap.NewFilePath))
			{
				Logger.LogLine("Swaping MCC-WindowsNoEditor.pak to vanilla.");

				if (!ForgeFileSwap.SwapToVanilla())
				{
					Logger.LogLine("Can't swap pak files while game is running.");
				}
			}

			Logger.LogLine("Switching " + HaloReach.SwapData.Count + " items to vanilla.");
			int count = HaloReach.SwapToVanilla();
			if (count < HaloReach.SwapData.Count)
				Logger.LogLine("Swaped " + count + " to vanilla files. Not all files swaped. If the game is running close it.");


			Logger.LogLine("Swaped to Vanilla");
			btnVanilla.Enabled = true;
		}

		private void btnModded_Click(object sender, EventArgs e)
		{
			btnModded.Enabled = false;

			if (!CheckInstallMCCLocation())
			{
				Logger.LogLine("MCC cannot be located to swap files.");
				btnModded.Enabled = true;
				return;
			}

			//swap pak file
			if (File.Exists(ForgeFileSwap.ModdedFilePath))
			{
				Logger.LogLine("Swaping MCC-WindowsNoEditor.pak to modded.");
				if (!ForgeFileSwap.SwapToModded())
				{
					Logger.LogLine("Can't swap pak files while game is running.");
				}
			}

			Logger.LogLine("Switching " + HaloReach.SwapData.Count + " items to modded.");
			int count = HaloReach.SwapToModded();
			if (count < HaloReach.SwapData.Count)
				Logger.LogLine("Swaped " + count + " to modded files. Not all files swaped. If the game is running close it.");

			Logger.LogLine("Swaped to Modded.");
			btnModded.Enabled = true;
		}

		#endregion

		#region Settings

		private void btnMCCLocate_Click(object sender, EventArgs e)
		{
			string path = FileUtil.OpenDirectoryDiag();

			if (path == "")
				return;//no file selected

			Settings.MCCLocation = path;
			txtMCCLocation.Text = Settings.MCCLocation;

			Settings.SaveSettings();
			Setup();
		}

		private void btnModsLocate_Click(object sender, EventArgs e)
		{
			string path = FileUtil.OpenDirectoryDiag();

			if (path == "")
				return;//no file selected

			//check if mods is install in folder
			Settings.ModsLocation = path;
			txtModsLocation.Text = Settings.ModsLocation;

			Settings.SaveSettings();
			Setup();
		}

		#endregion

		#region GeneralTab
		private void btnStartupVideo_Click(object sender, EventArgs e)
		{
			if (!CheckInstallMCCLocation())
			{
				Logger.Log("MCC not installed here. Not removing startup video.");
				//MessageBox.Show("MCC not installed here", "MCC not installed here. Not removing startup video.");
				return;
			}

			string og = Settings.StartupMovieLocation;
			string rn = Settings.StartupMovieLocation + ".bk";

			//justs renames the video file
			if (File.Exists(og))
			{
				Logger.Log("Disabling start up video");
				File.Move(og, rn);
				btnStartupVideo.Text = "Enable Startup Video";
			}
			else if (File.Exists(rn))
			{
				Logger.Log("Enabling start up video");
				File.Move(rn, og);
				btnStartupVideo.Text = "Disable Startup Video";
			}

			Logger.LogLine(". Done");
		}

		private void btnForge_Click(object sender, EventArgs e)
		{
			btnForge.Enabled = false;

			if (!CheckInstallMCCLocation())
			{
				Logger.LogLine("Can't file \"MCC-WindowsNoEditor.pak\", is the MCC install location correct.");
				btnForge.Enabled = true;
				return;
			}

			Logger.LogLine("Creating Backup of \"MCC-WindowsNoEditor.pak\"");
			if (!ForgeFileSwap.MoveOriginalFile())
			{
				Logger.LogLine("Can't backup \"MCC-WindowsNoEditor.pak\", if the game is running close it.");
				btnForge.Enabled = true;
				return;
			}
			//File.Move(ForgeFileSwap.VanillaFilePath, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");

			Logger.LogLine("Creating copy to mod");
			File.Copy(ForgeFileSwap.NewFilePath, ForgeFileSwap.ModdedFilePath);

			Logger.LogLine("Modding copy");
			List<HexEditData> enableForge = new List<HexEditData>() {
			new HexEditData() { Position = 0x2FFC72D0, Bytes = 0x27},
			new HexEditData() { Position = 0x2FFD4110, Bytes = 0x27}};

			//mod the copy to enable forge
			FileUtil.HexEdit(ForgeFileSwap.ModdedFilePath, enableForge);

			//FileUtil.CreateHardLink(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");

			Logger.LogLine("Forge can now be enabled by pressing the \"Modded\" button.");
			//btnForge.Enabled = true;
		}

		#endregion

		#region HaloReachTab
		private void btnHRSwapModded_Click(object sender, EventArgs e)
		{
			btnHRSwapModded.Enabled = false;

			if (lstHRVanillaMaps.Items.Count == 0)
			{
				Logger.LogLine("No vanilla maps found.");
				btnHRSwapModded.Enabled = true;
				return;
			}
			else if (lstHRModdedMaps.Items.Count == 0)
			{
				Logger.LogLine("No modded maps found.");
				btnHRSwapModded.Enabled = true;
				return;
			}
			else if (lstHRModdedMaps.SelectedItem == null || lstHRVanillaMaps.SelectedItem == null)
			{
				Logger.LogLine("Please select a map from each list.");
				btnHRSwapModded.Enabled = true;
				return;
			}

			string vanMap = lstHRVanillaMaps.SelectedItem.ToString();
			string modMap = lstHRModdedMaps.SelectedItem.ToString();
			//check if vanilla map is backed up
			if (!File.Exists(HaloReach.VanillaBackupMapLocation + "\\" + vanMap))
			{
				//create backup
				Logger.Log("Backing up vanilla map \"" + vanMap + "\"");
				File.Copy(HaloReach.VanillaMapLocation + "\\" + vanMap, HaloReach.VanillaBackupMapLocation + "\\" + vanMap);
				Logger.LogLine(". Done");
			}

			File.Delete(Settings.MCCLocation + "\\haloreach\\maps\\" + vanMap);

			FileUtil.CreateHardLink(HaloReach.VanillaMaps[vanMap], HaloReach.ModdedMaps[modMap]);
			Logger.LogLine(string.Format("Swaped \"{0}\" to \"{1}\"", vanMap, modMap));

			btnHRSwapModded.Enabled = true;
		}
		private void btnHRSwapVanilla_Click(object sender, EventArgs e)
		{
			btnHRSwapVanilla.Enabled = false;

			if (lstHRVanillaMaps.Items.Count == 0)
			{
				Logger.LogLine("No vanilla maps found.");
				btnHRSwapVanilla.Enabled = true;
				return;
			}
			else if (lstHRVanillaMaps.SelectedItem == null)
			{
				Logger.LogLine("Please select a map from the vanilla list.");
				btnHRSwapVanilla.Enabled = true;
				return;
			}

			string vanMap = lstHRVanillaMaps.SelectedItem.ToString();

			//check if vanilla map is backed up
			if (!File.Exists(HaloReach.VanillaBackupMapLocation + "\\" + vanMap))
			{
				//File has not been modded
				Logger.LogLine("Vanilla map \"" + vanMap + "\" has never been swaped.");
				btnHRSwapVanilla.Enabled = true;
				return;
			}

			//delete hard linked file from vanilla map folder
			File.Delete(HaloReach.VanillaMapLocation + "\\" + vanMap);
			HaloReach.ReloadMaps();
			//create hardlink to vanilla map in backup folder
			FileUtil.CreateHardLink(HaloReach.VanillaMaps[vanMap], HaloReach.VanillaBackupMaps[vanMap]);
			Logger.LogLine(string.Format("Swaping \"{0}\" back to original", vanMap));

			btnHRSwapVanilla.Enabled = true;
		}

		private void btnHRLoadQuick_Click(object sender, EventArgs e)
		{
			btnHRLoadQuick.Enabled = false;

			if (lstHRVanillaMaps.Items.Count == 0)
			{
				Logger.LogLine("No vanilla maps found.");
				btnHRLoadQuick.Enabled = true;
				return;
			}
			else if (lstHRModdedMaps.Items.Count == 0)
			{
				Logger.LogLine("No modded maps found.");
				btnHRLoadQuick.Enabled = true;
				return;
			}
			else if (lstHRModdedMaps.SelectedItem == null || lstHRVanillaMaps.SelectedItem == null)
			{
				Logger.LogLine("Please select a map from each list.");
				btnHRLoadQuick.Enabled = true;
				return;
			}

			string vanMap = lstHRVanillaMaps.SelectedItem.ToString();
			string modMap = lstHRModdedMaps.SelectedItem.ToString();
			//check if vanilla map is backed up
			if (!File.Exists(Settings.ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap))
			{
				//create backup
				Logger.Log("Backing up vanilla map \"" + vanMap + "\"");
				File.Move(Settings.MCCLocation + "\\haloreach\\maps\\" + vanMap, Settings.ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap);
				Logger.LogLine(". Done");
			}

			if (HaloReach.SwapData.ContainsKey(vanMap))
			{
				//remove old entry
				HaloReach.SwapData.Remove(vanMap);
				Logger.LogLine("Overwrote existing swap.");
			}

			//add new entry and swap maps
			HaloReach.AddMapSwapData(vanMap, vanMap, modMap);
			HaloReach.ReloadMaps();
			HaloReach.SaveSwapData();

			UpdateSwapList();
			Logger.LogLine("Added \"" + vanMap + "\" -> \"" + modMap + "\" to swap list.");

			btnHRLoadQuick.Enabled = true;
		}

		#region DeleteButtons

		private void btnHRDeleteSwap_Click(object sender, EventArgs e)
		{
			if (lstHRSwapsToLoad.Items.Count == 0)
				return;

			int index = lstHRSwapsToLoad.SelectedIndex;

			if (index == -1)
			{
				Logger.LogLine("No selected item to delete from swap list.");
				return;
			}

			string swapName = "";
			int i = 0;
			foreach (var item in HaloReach.SwapData)
			{
				if (i == index)
				{
					swapName = item.Key;
					break;
				}
				i++;
			}

			if (!HaloReach.SwapData[swapName].OtherFile)
			{
				HaloReach.SwapToVanilla(swapName);
				HaloReach.SwapData.Remove(swapName);
			}
			else
			{
				HaloReach.RestoreOriginal(swapName);
				Logger.LogLine("Moved vanilla file back of original location");
			}
			HaloReach.SaveSwapData();
			UpdateSwapList();
		}

		private void btnHRDeleteAll_Click(object sender, EventArgs e)
		{
			List<SwapData> deletes = new List<SwapData>();
			foreach (var item in HaloReach.SwapData)
			{
				if (!HaloReach.SwapData[item.Key].OtherFile)
				{
					HaloReach.SwapToVanilla(item.Key);
				}
				else
				{
					HaloReach.RestoreAllOrigianlFiles();
				}
			}

			Logger.LogLine("Moved vanilla file back of original location");
			HaloReach.SwapData.Clear();
			HaloReach.SaveSwapData();
			UpdateSwapList();
		}


		#endregion

		#region OtherFileToMod
		private void btnHRSelectOriginal_Click(object sender, EventArgs e)
		{
			string file = FileUtil.OpenFileDiag();
			txtHROriginalFile.Text = file;
		}

		private void btnHRSelectModded_Click(object sender, EventArgs e)
		{
			string file = FileUtil.OpenFileDiag();
			txtHRModdedFile.Text = file;
		}

		private void btnHRMod_Click(object sender, EventArgs e)
		{
			string ogFile = txtHROriginalFile.Text;
			string modFile = txtHRModdedFile.Text;

			if (ogFile == "" || modFile == "")
			{
				Logger.LogLine("Please select both files.");
				return;
			}
			else if (ogFile == modFile)
			{
				Logger.LogLine("The files are the same.");
				return;
			}
			else if (!File.Exists(ogFile) || !File.Exists(modFile))
			{
				Logger.LogLine("Can't find one of the files.");
				return;
			}
			else if (ogFile[0] != modFile[0])
			{
				Logger.LogLine("Files have to be on the same drive.");
				return;
			}

			btnHRMod.Enabled = false;

			string[] temp = ogFile.Split('\\');
			string ogFilename = temp[temp.Length - 1];
			temp = modFile.Split('\\');
			string modFilename = temp[temp.Length - 1];

			SwapData swap = new SwapData();
			swap.ModdedFileName = modFilename;
			swap.ModdedFilePath = modFile;
			swap.VanillaFileName = ogFilename;
			swap.VanillaFilePath = ogFile;
			swap.OtherFile = true;

			//backup Vanilla file
			if (!File.Exists(Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename))
			{
				Logger.Log("Backing up \"" + ogFilename + "\"");
				File.Move(ogFile, Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename);
				Logger.LogLine(". Done");
			}
			else
			{
				Logger.LogLine("Backup file already exist, delete the backup to try again. Located at \"" + Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename + "\"");
				btnHRMod.Enabled = true;
				return;
			}

			TempSwap.Add(ogFilename, swap);

			bool t = FileUtil.CreateHardLink(ogFile, modFile);

			lstTempSwap.Items.Clear();
			foreach (var item in TempSwap)
			{
				lstTempSwap.Items.Add(ogFilename + " -> " + modFilename);
			}
			Logger.LogLine("Files swaped.");
			btnHRMod.Enabled = true;
		}
		private void btnHROtherQuickSwap_Click(object sender, EventArgs e)
		{
			string ogFile = txtHROriginalFile.Text;
			string modFile = txtHRModdedFile.Text;

			if (ogFile == "" || modFile == "")
			{
				Logger.LogLine("Please select both files.");
				return;
			}
			else if (ogFile == modFile)
			{
				Logger.LogLine("The files are the same.");
				return;
			}
			else if (!File.Exists(ogFile) || !File.Exists(modFile))
			{
				Logger.LogLine("Can't find one of the files.");
				return;
			}
			else if (ogFile[0] != modFile[0])
			{
				Logger.LogLine("Files have to be on the same drive.");
				return;
			}

			btnHRMod.Enabled = false;

			string[] temp = ogFile.Split('\\');
			string ogFilename = temp[temp.Length - 1];
			temp = modFile.Split('\\');
			string modFilename = temp[temp.Length - 1];

			SwapData swap = new SwapData();
			swap.ModdedFileName = modFilename;
			swap.ModdedFilePath = modFile;
			swap.VanillaFileName = ogFilename;
			swap.VanillaFilePath = ogFile;
			swap.NewFileName = ogFilename;
			swap.NewFilePath = Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename;
			swap.OtherFile = true;

			//backup Vanilla file
			if (!File.Exists(Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename))
			{
				Logger.Log("Backing up \"" + ogFilename + "\"");
				File.Move(ogFile, Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename);
				Logger.LogLine(". Done");
			}
			else
			{
				Logger.LogLine("Backup file already exist, restore the backup to it original location and try again. Located at \"" 
					+ Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename + "\"");
				btnHRMod.Enabled = true;
				return;
			}

			HaloReach.AddSwapData(ogFilename, swap);

			//HaloReach.SwapData.Add(ogFilename, swap);

			//bool t = FileUtil.CreateHardLink(ogFile, modFile);
			HaloReach.SaveSwapData();

			UpdateSwapList();

			Logger.LogLine("Files swaped.");
			btnHRMod.Enabled = true;
		}

		private void btnDeleteTempSwap_Click(object sender, EventArgs e)
		{
			int index = lstTempSwap.SelectedIndex;
			if (index == -1)
			{
				Logger.LogLine("Please select an item from the temp swap list.");
				return;
			}

			btnDeleteTempSwap.Enabled = false;

			int i = 0;
			string tempIndex = "";
			foreach (var item in TempSwap)
			{
				if (i == index)
				{
					tempIndex = item.Key;
					//restore original file
					//HaloReach.RestoreOriginal(item.Key);
					File.Delete(TempSwap[item.Key].VanillaFilePath);
					File.Move(Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + TempSwap[item.Key].VanillaFileName, TempSwap[item.Key].VanillaFilePath);
					Logger.LogLine("Moved vanilla file back of original location");
					break;
				}
				i++;
			}
			TempSwap.Remove(tempIndex);
			lstTempSwap.Items.Clear();
			foreach (var item in TempSwap)
			{
				lstTempSwap.Items.Add(item.Value.VanillaFilePath + " -> " + item.Value.ModdedFileName);
			}
			btnDeleteTempSwap.Enabled = true;
		}

		#endregion

		#endregion
		public bool CheckInstallMCCLocation()
		{
			return File.Exists(Settings.exeLocation);
		}

		public void LogEvent(string message)
		{
			txtLog.AppendText(message);
		}
		public void LogLineEvent(string message)
		{
			txtLog.AppendText(message + "\r\n");
		}

		private void UpdateSwapList()
		{
			lstHRSwapsToLoad.Items.Clear();
			foreach (var item in HaloReach.SwapData)
			{
				lstHRSwapsToLoad.Items.Add(item.Value.VanillaFileName + " -> " + item.Value.ModdedFileName);
			}
		}

		private void HaloMods_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach (var item in TempSwap)
			{
				//restore original file
				File.Delete(item.Value.VanillaFilePath);
				File.Move(Settings.ModsLocation + "\\Vanilla\\Halo Reach\\" + item.Value.VanillaFileName, item.Value.VanillaFilePath);
				Logger.LogLine("Moved vanilla file back of original location");
			}
			lstTempSwap.Items.Clear();
			foreach (var item in TempSwap)
			{
				lstTempSwap.Items.Add(item.Value.VanillaFilePath + " -> " + item.Value.ModdedFileName);
			}
		}

		private void btnResetEveryThing_Click(object sender, EventArgs e)
		{
			btnResetEveryThing.Enabled = false;
			Logger.LogLine("Restoring install to vanilla.");
			//restore startup video
			string og = Settings.StartupMovieLocation;
			string rn = Settings.StartupMovieLocation + ".bk";

			//justs renames the video file
			if (File.Exists(rn))
			{
				Logger.Log("Enabling start up video");
				File.Move(rn, og);
				btnStartupVideo.Text = "Disable Startup Video";
			}

			//restore MMC.pak
			if (File.Exists(ForgeFileSwap.NewFilePath))
			{
				Logger.LogLine("Restoring \"MCC-WindowsNoEditor.pak\".");
				if (ForgeFileSwap.RestoreOriginalFiles())
				{
					if (File.Exists(ForgeFileSwap.ModdedFilePath))
						File.Delete(ForgeFileSwap.ModdedFilePath);
				}
				else
				{
					Logger.LogLine("Can't restore \"MCC-WindowsNoEditor.pak\" back to original state. The game is running or the backup file is missing.");
				}
			}

			//restore maps
			Logger.LogLine("Restoring " + HaloReach.VanillaBackupMaps.Count + " Map Files.");
			HaloReach.ReloadMaps();
			foreach (var item in HaloReach.VanillaBackupMaps)
			{
				if (File.Exists(item.Value))
				{
					if (File.Exists(HaloReach.VanillaMapLocation + "\\" + item.Key))
						File.Delete(HaloReach.VanillaMapLocation + "\\" + item.Key);
					File.Move(item.Value, HaloReach.VanillaMapLocation + "\\" + item.Key);
					HaloReach.SwapData.Remove(item.Key);
				}
			}

			HaloReach.RestoreAllOrigianlFiles();
			HaloReach.ReloadMaps();
			HaloReach.SaveSwapData();

			Logger.LogLine("Restoring Temp Files.");
			foreach (var item in TempSwap)
			{
				item.Value.RestoreOriginalFiles();
			}

			Setup();

			Logger.LogLine("Restoring is finished.");
			btnResetEveryThing.Enabled = true;
		}

		private void label7_Click(object sender, EventArgs e)
		{
			Logger.LogLine("Why did you click this?");
		}

		private void btnOpenModsFolder_Click(object sender, EventArgs e)
		{
			FileUtil.OpenFileExplorer(Settings.HR_ModdedMapLocation);
		}

		private void btnReloadMods_Click(object sender, EventArgs e)
		{
			HaloReach.ReloadMaps();

			//populate the mods list
			lstHRModdedMaps.Items.Clear();
			foreach (var item in HaloReach.ModdedMaps)
				lstHRModdedMaps.Items.Add(item.Key);
			//select first item
			if (HaloReach.ModdedMaps.Count > 0)
				lstHRModdedMaps.SelectedIndex = 0;
		}
	}
}
