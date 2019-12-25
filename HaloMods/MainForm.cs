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
        string MCCLocation = @"C:\Users\GGGGG\Desktop\HaloMCC Install";
        string ModsLocation = @"C:\Users\GGGGG\Desktop\HaloMCC Install\MODS";

        string OriginalMCCpakFile = @"\mcc\content\paks\MCC-WindowsNoEditor.pak";
        SwapData ForgeFileSwap;// = new SwapData();

        string StartupMovieFile = @"\mcc\content\movies\FMS_logo_microsoft_7_1_.bk2";

        string txtFileSettings = "settings.txt";
        string HaloReachFile = "HaloReach.json";

        GameData HaloReach;// = new GameData();

        Dictionary<string, SwapData> TempSwap = new Dictionary<string, SwapData>();

        public HaloMods()
        {
            InitializeComponent();

            if (!File.Exists(txtFileSettings))
            {
                Console.WriteLine("Init setup running.");
                //make settings file
                File.WriteAllText(txtFileSettings, Properties.Resources.settings);
            }

            if (File.Exists(txtFileSettings))
            {
                string[] settings = File.ReadAllLines(txtFileSettings);

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
            }

            txtMCCLocation.Text = MCCLocation;
            txtModsLocation.Text = ModsLocation;

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
            bool installed = CheckInstallMCCLocation();

            if (!installed)
            {
                LogLine("MCC not installed at");
                LogLine(MCCLocation);
                LogLine("please change it in settings.");

                //disable all buttons
                DisableButtonControls(this);
                return;
            }
            else
            {
                LogLine("MCC is installed. " + MCCLocation);

            }
            EnableButtonControls(this);
            LogLine("Mods is installed. " + ModsLocation);

            //create mods folders
            Directory.CreateDirectory(ModsLocation + "\\Mods\\Halo Reach\\Maps");
            Directory.CreateDirectory(ModsLocation + "\\Vanilla\\Halo Reach\\Maps");

            //general tab setup
            //start up video
            if (File.Exists(MCCLocation + StartupMovieFile))
                btnStartupVideo.Text = "Disable Startup Video";
            else
                btnStartupVideo.Text = "Enable Startup Video";

            //enable forge
            ForgeFileSwap = new SwapData(MCCLocation + OriginalMCCpakFile,
                ModsLocation + "\\Vanilla\\" + FileUtil.GetFileName(OriginalMCCpakFile),
                ModsLocation + "\\Mods\\" + FileUtil.GetFileName(OriginalMCCpakFile));

            if (File.Exists(ForgeFileSwap.ModdedFilePath) && File.Exists(ForgeFileSwap.NewFilePath))
                btnForge.Enabled = false;//forge has already been setup

            //halo reach tab setup
            HaloReach = new GameData("HaloReach.json", MCCLocation + "\\haloreach\\maps", ModsLocation + "\\Vanilla\\Halo Reach\\Maps", ModsLocation + "\\Mods\\Halo Reach\\Maps");
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
                LogLine("MCC cannot be located to swap files.");
                btnVanilla.Enabled = true;
                return;
            }

            //swap pak file
            if (File.Exists(ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak"))
            {
                LogLine("Swaping MCC-WindowsNoEditor.pak to vanilla.");

                if (FileUtil.IsFileInUse(MCCLocation + OriginalMCCpakFile))
                {
                    LogLine("Can't swap pak files while game is running.");
                }
                else
                {
                    File.Delete(MCCLocation + OriginalMCCpakFile);
                    FileUtil.CreateHardLink(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");
                }
            }

            LogLine("Switching " + HaloReach.SwapData.Count + " items to vanilla.");
            int count = HaloReach.SwapToVanilla();
            if (count < HaloReach.SwapData.Count)
                LogLine("Swaped " + count + " to vanilla files. Not all files swaped. If the game is running close it.");

            //foreach (var item in HaloReach.SwapData.Keys)
            //{
            //    //swap other files
            //    //if (item.OtherFile)
            //    //{
            //    //    File.Delete(item.VanillaFilePath);
            //    //    FileUtil.CreateHardLink(item.VanillaFilePath, ModsLocation + "\\Vanilla\\Halo Reach\\" + item.VanillaFileName);
            //    //}
            //    //else
            //    //{
            //    //    File.Delete(item.VanillaFilePath);
            //    //    FileUtil.CreateHardLink(item.VanillaFilePath, item.ModdedFilePath);
            //    //}
            //}

            LogLine("Swaped to Vanilla");
            btnVanilla.Enabled = true;
        }

        private void btnModded_Click(object sender, EventArgs e)
        {
            btnModded.Enabled = false;

            if (!CheckInstallMCCLocation())
            {
                LogLine("MCC cannot be located to swap files.");
                btnModded.Enabled = true;
                return;
            }

            //swap pak file
            if (File.Exists(ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak"))
            {
                LogLine("Swaping MCC-WindowsNoEditor.pak to modded.");
                try
                {
                    File.Delete(MCCLocation + OriginalMCCpakFile);
                    FileUtil.CreateHardLink(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak");
                }
                catch (Exception)
                {
                    LogLine("Can't swap pak files while game is running.");
                }
            }

            LogLine("Switching " + HaloReach.SwapData.Count + " items to modded.");
            int count = HaloReach.SwapToModded();
            if (count < HaloReach.SwapData.Count)
                LogLine("Swaped " + count + " to modded files. Not all files swaped. If the game is running close it.");

            LogLine("Swaped to Modded.");
            btnModded.Enabled = true;
        }

        #endregion

        #region Settings

        private void SaveSettings()
        {
            File.Delete(txtFileSettings);
            string output = "install-folder=" + MCCLocation + "\r\n" +
                "mods-folder=" + ModsLocation;
            File.WriteAllText(txtFileSettings, output);
        }

        private void btnMCCLocate_Click(object sender, EventArgs e)
        {
            string path = FileUtil.OpenDirectoryDiag();

            if (path == "")
                return;//no file selected

            MCCLocation = path;
            txtMCCLocation.Text = MCCLocation;

            //check if mcc is install in folder
            //if (!CheckInstallMCCLocation())
            //{
            //    LogLine("MCC not installed here");
            //    MessageBox.Show("MCC not installed here", "MCC not installed here");
            //}
            SaveSettings();
            Setup();
        }

        private void btnModsLocate_Click(object sender, EventArgs e)
        {
            string path = FileUtil.OpenDirectoryDiag();

            if (path == "")
                return;//no file selected

            //check if mods is install in folder
            ModsLocation = path;
            txtModsLocation.Text = ModsLocation;

            //create mods folders
            Directory.CreateDirectory(ModsLocation + "\\Mods\\Halo Reach");
            Directory.CreateDirectory(ModsLocation + "\\Vanilla\\Halo Reach");

            SaveSettings();
            Setup();
        }

        #endregion

        #region GeneralTab
        private void btnStartupVideo_Click(object sender, EventArgs e)
        {
            if (!CheckInstallMCCLocation())
            {
                Log("MCC not installed here. Not removing startup video.");
                //MessageBox.Show("MCC not installed here", "MCC not installed here. Not removing startup video.");
                return;
            }

            string og = MCCLocation + StartupMovieFile;
            string rn = MCCLocation + StartupMovieFile + ".bk";

            //justs renames the video file
            if (File.Exists(og))
            {
                Log("Disabling start up video");
                File.Move(og, rn);
                btnStartupVideo.Text = "Enable Startup Video";
            }
            else if (File.Exists(rn))
            {
                Log("Enabling start up video");
                File.Move(rn, og);
                btnStartupVideo.Text = "Disable Startup Video";
            }

            LogLine(". Done");
        }

        private void btnForge_Click(object sender, EventArgs e)
        {
            btnForge.Enabled = false;

            if (!CheckInstallMCCLocation())
            {
                LogLine("Can't file \"MCC-WindowsNoEditor.pak\", is the MCC install location correct.");
                btnForge.Enabled = true;
                return;
            }

            LogLine("Creating Backup of \"MCC-WindowsNoEditor.pak\"");
            if (!ForgeFileSwap.MoveOriginalFile())
            {
                LogLine("Can't backup \"MCC-WindowsNoEditor.pak\", if the game is running close it.");
                btnForge.Enabled = true;
                return;
            }
            //File.Move(ForgeFileSwap.VanillaFilePath, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");

            LogLine("Creating copy to mod");
            File.Copy(ForgeFileSwap.NewFilePath, ForgeFileSwap.ModdedFilePath);

            LogLine("Modding copy");
            List<HexEditData> enableForge = new List<HexEditData>() {
            new HexEditData() { Position = 0x1E302110, Bytes = 0x27},
            new HexEditData() { Position = 0x1E2F52D0, Bytes = 0x27}};

            //mod the copy to enable forge
            FileUtil.HexEdit(ForgeFileSwap.ModdedFilePath, enableForge);

            //FileUtil.CreateHardLink(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");

            LogLine("Forge can now be enabled by pressing the \"Modded\" button.");
            //btnForge.Enabled = true;
        }

        #endregion

        #region HaloReachTab
        private void btnHRSwapModded_Click(object sender, EventArgs e)
        {
            btnHRSwapModded.Enabled = false;

            if (lstHRVanillaMaps.Items.Count == 0)
            {
                LogLine("No vanilla maps found.");
                btnHRSwapModded.Enabled = true;
                return;
            }
            else if (lstHRModdedMaps.Items.Count == 0)
            {
                LogLine("No modded maps found.");
                btnHRSwapModded.Enabled = true;
                return;
            }
            else if (lstHRModdedMaps.SelectedItem == null || lstHRVanillaMaps.SelectedItem == null)
            {
                LogLine("Please select a map from each list.");
                btnHRSwapModded.Enabled = true;
                return;
            }

            string vanMap = lstHRVanillaMaps.SelectedItem.ToString();
            string modMap = lstHRModdedMaps.SelectedItem.ToString();
            //check if vanilla map is backed up
            if (!File.Exists(HaloReach.VanillaBackupMapLocation + "\\" + vanMap))
            {
                //create backup
                Log("Backing up vanilla map \"" + vanMap + "\"");
                File.Copy(HaloReach.VanillaMapLocation + "\\" + vanMap, HaloReach.VanillaBackupMapLocation + "\\" + vanMap);
                LogLine(". Done");
            }

            File.Delete(MCCLocation + "\\haloreach\\maps\\" + vanMap);

            FileUtil.CreateHardLink(HaloReach.VanillaMaps[vanMap], HaloReach.ModdedMaps[modMap]);
            LogLine(string.Format("Swaped \"{0}\" to \"{1}\"", vanMap, modMap));

            btnHRSwapModded.Enabled = true;
        }
        private void btnHRSwapVanilla_Click(object sender, EventArgs e)
        {
            btnHRSwapVanilla.Enabled = false;

            if (lstHRVanillaMaps.Items.Count == 0)
            {
                LogLine("No vanilla maps found.");
                btnHRSwapVanilla.Enabled = true;
                return;
            }
            else if (lstHRVanillaMaps.SelectedItem == null)
            {
                LogLine("Please select a map from the vanilla list.");
                btnHRSwapVanilla.Enabled = true;
                return;
            }

            string vanMap = lstHRVanillaMaps.SelectedItem.ToString();

            //check if vanilla map is backed up
            if (!File.Exists(HaloReach.VanillaBackupMapLocation + "\\" + vanMap))
            {
                //File has not been modded
                LogLine("Vanilla map \"" + vanMap + "\" has never been swaped.");
                btnHRSwapVanilla.Enabled = true;
                return;
            }

            //delete hard linked file from vanilla map folder
            File.Delete(HaloReach.VanillaMapLocation + "\\" + vanMap);
            HaloReach.ReloadMaps();
            //create hardlink to vanilla map in backup folder
            FileUtil.CreateHardLink(HaloReach.VanillaMaps[vanMap], HaloReach.VanillaBackupMaps[vanMap]);
            LogLine(string.Format("Swaping \"{0}\" back to original", vanMap));

            btnHRSwapVanilla.Enabled = true;
        }

        private void btnHRLoadQuick_Click(object sender, EventArgs e)
        {
            btnHRLoadQuick.Enabled = false;

            if (lstHRVanillaMaps.Items.Count == 0)
            {
                LogLine("No vanilla maps found.");
                btnHRLoadQuick.Enabled = true;
                return;
            }
            else if (lstHRModdedMaps.Items.Count == 0)
            {
                LogLine("No modded maps found.");
                btnHRLoadQuick.Enabled = true;
                return;
            }
            else if (lstHRModdedMaps.SelectedItem == null || lstHRVanillaMaps.SelectedItem == null)
            {
                LogLine("Please select a map from each list.");
                btnHRLoadQuick.Enabled = true;
                return;
            }

            string vanMap = lstHRVanillaMaps.SelectedItem.ToString();
            string modMap = lstHRModdedMaps.SelectedItem.ToString();
            //check if vanilla map is backed up
            if (!File.Exists(ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap))
            {
                //create backup
                Log("Backing up vanilla map \"" + vanMap + "\"");
                File.Move(MCCLocation + "\\haloreach\\maps\\" + vanMap, ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap);
                LogLine(". Done");
            }

            if (HaloReach.SwapData.ContainsKey(vanMap))
            {
                //remove old entry
                HaloReach.SwapData.Remove(vanMap);
                LogLine("Overwrote existing swap.");
            }

            //add new entry and swap maps
            HaloReach.AddMapSwapData(vanMap, vanMap, modMap);
            HaloReach.ReloadMaps();
            HaloReach.SaveSwapData();

            UpdateSwapList();
            LogLine("Added \"" + vanMap + "\" -> \"" + modMap + "\" to swap list.");

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
                LogLine("No selected item to delete from swap list.");
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
                LogLine("Moved vanilla file back of original location");
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

            LogLine("Moved vanilla file back of original location");
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
                LogLine("Please select both files.");
                return;
            }
            else if (ogFile == modFile)
            {
                LogLine("The files are the same.");
                return;
            }
            else if (!File.Exists(ogFile) || !File.Exists(modFile))
            {
                LogLine("Can't find one of the files.");
                return;
            }
            else if (ogFile[0] != modFile[0])
            {
                LogLine("Files have to be on the same drive.");
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
            if (!File.Exists(ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename))
            {
                Log("Backing up \"" + ogFilename + "\"");
                File.Move(ogFile, ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename);
                LogLine(". Done");
            }
            else
            {
                LogLine("Backup file already exist, delete the backup to try again. Located at \"" + ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename + "\"");
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
            LogLine("Files swaped.");
            btnHRMod.Enabled = true;
        }
        private void btnHROtherQuickSwap_Click(object sender, EventArgs e)
        {
            string ogFile = txtHROriginalFile.Text;
            string modFile = txtHRModdedFile.Text;

            if (ogFile == "" || modFile == "")
            {
                LogLine("Please select both files.");
                return;
            }
            else if (ogFile == modFile)
            {
                LogLine("The files are the same.");
                return;
            }
            else if (!File.Exists(ogFile) || !File.Exists(modFile))
            {
                LogLine("Can't find one of the files.");
                return;
            }
            else if (ogFile[0] != modFile[0])
            {
                LogLine("Files have to be on the same drive.");
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
            swap.NewFilePath = ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename;
            swap.OtherFile = true;

            //backup Vanilla file
            if (!File.Exists(ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename))
            {
                Log("Backing up \"" + ogFilename + "\"");
                File.Move(ogFile, ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename);
                LogLine(". Done");
            }
            else
            {
                LogLine("Backup file already exist, restore the backup to it original location and try again. Located at \"" + ModsLocation + "\\Vanilla\\Halo Reach\\" + ogFilename + "\"");
                btnHRMod.Enabled = true;
                return;
            }

            HaloReach.AddSwapData(ogFilename, swap);

            //HaloReach.SwapData.Add(ogFilename, swap);

            //bool t = FileUtil.CreateHardLink(ogFile, modFile);
            HaloReach.SaveSwapData();

            UpdateSwapList();

            LogLine("Files swaped.");
            btnHRMod.Enabled = true;
        }

        private void btnDeleteTempSwap_Click(object sender, EventArgs e)
        {
            int index = lstTempSwap.SelectedIndex;
            if (index == -1)
            {
                LogLine("Please select an item from the temp swap list.");
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
                    File.Move(ModsLocation + "\\Vanilla\\Halo Reach\\" + TempSwap[item.Key].VanillaFileName, TempSwap[item.Key].VanillaFilePath);
                    LogLine("Moved vanilla file back of original location");
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
            return File.Exists(MCCLocation + "\\mcclauncher.exe");
        }

        public void Log(string message)
        {
            txtLog.AppendText(message);
        }
        public void LogLine(string message)
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
                File.Move(ModsLocation + "\\Vanilla\\Halo Reach\\" + item.Value.VanillaFileName, item.Value.VanillaFilePath);
                LogLine("Moved vanilla file back of original location");
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
            LogLine("Restoring install to vanilla.");
            //restore startup video
            string og = MCCLocation + StartupMovieFile;
            string rn = MCCLocation + StartupMovieFile + ".bk";

            //justs renames the video file
            if (File.Exists(rn))
            {
                Log("Enabling start up video");
                File.Move(rn, og);
                btnStartupVideo.Text = "Disable Startup Video";
            }

            //restore MMC.pak
            if (File.Exists(ForgeFileSwap.NewFilePath))
            {
                LogLine("Restoring \"MCC-WindowsNoEditor.pak\".");
                if (ForgeFileSwap.RestoreOriginalFiles())
                {
                    if (File.Exists(ForgeFileSwap.ModdedFilePath))
                        File.Delete(ForgeFileSwap.ModdedFilePath);
                }
                else
                {
                    LogLine("Can't restore \"MCC-WindowsNoEditor.pak\" back to original state. The game is running or the backup file is missing.");
                }
            }

            //restore maps
            LogLine("Restoring " + HaloReach.VanillaBackupMaps.Count + " Map Files.");
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

            LogLine("Restoring Temp Files.");
            foreach (var item in TempSwap)
            {
                item.Value.RestoreOriginalFiles();
            }

            Setup();

            LogLine("Restoring is finished.");
            btnResetEveryThing.Enabled = true;
        }

        private void label7_Click(object sender, EventArgs e)
        {
            LogLine("Why did you click this?");
        }

        private void btnOpenModsFolder_Click(object sender, EventArgs e)
        {
            FileUtil.OpenFileExplorer(ModsLocation + "\\Mods\\Halo Reach\\Maps");
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
