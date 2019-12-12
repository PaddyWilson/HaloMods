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

        string StartupMovieFile = @"\mcc\content\movies\FMS_logo_microsoft_7_1_.bk2";

        string OriginalMCCpakFile = @"\mcc\content\paks\MCC-WindowsNoEditor.pak";

        GameData HaloReach;// = new GameData();

        Dictionary<string, SwapData> TempSwap = new Dictionary<string, SwapData>();

        public HaloMods()
        {
            InitializeComponent();

            txtMCCLocation.Text = MCCLocation;
            txtModsLocation.Text = ModsLocation;

            bool installed = CheckInstallMCCLocation();

            if (!installed)
            {
                LogLine("MCC not installed at");
                LogLine(MCCLocation);
                LogLine("please change it in settings.");
                return;
            }

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
            if (File.Exists(ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak"))
                btnForge.Enabled = false;


            //halo reach tab setup
            HaloReach = new GameData(MCCLocation + "\\haloreach\\maps", ModsLocation + "\\Vanilla\\Halo Reach\\Maps", ModsLocation + "\\Mods\\Halo Reach\\Maps");

            //populate the vanilla list
            lstHRVanillaMaps.Items.Clear();
            foreach (var item in HaloReach.VanillaMaps)
                lstHRVanillaMaps.Items.Add(item.Key);
            //select first item
            if (lstHRVanillaMaps.Items.Count > 0)
                lstHRVanillaMaps.SelectedIndex = 0;

            //populate the mods list
            lstHRModdedMaps.Items.Clear();
            foreach (var item in HaloReach.ModdedMaps)
                lstHRModdedMaps.Items.Add(item.Key);
            //select first item
            if (lstHRModdedMaps.Items.Count > 0)
                lstHRModdedMaps.SelectedIndex = 0;

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
                FileUtil.CreateHardLink(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");
            }
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
                FileUtil.CreateHardLink(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak");
            }
            btnModded.Enabled = true;
        }

        #endregion

        #region Settings
        private void btnMCCLocate_Click(object sender, EventArgs e)
        {
            string path = FileUtil.OpenDirectoryDiag();

            if (path == "")
                return;//no file selected

            MCCLocation = path;
            txtMCCLocation.Text = MCCLocation;

            //check if mcc is install in folder
            if (!CheckInstallMCCLocation())
            {
                LogLine("MCC not installed here");
                //MessageBox.Show("MCC not installed here", "MCC not installed here");
            }
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

            //if(MCCLocation[0] != ModsLocation[0])
            //{
            //    MessageBox.Show("Please select Folders on the drive EG. C: or D: etc");
            //}
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

            if (!File.Exists(MCCLocation + OriginalMCCpakFile))
            {
                LogLine("Can't file \"MCC-WindowsNoEditor.pak\", is the MCC install location correct.");
                btnForge.Enabled = true;
                return;
            }

            LogLine("Creating Backup of \"MCC-WindowsNoEditor.pak\"");
            File.Move(MCCLocation + OriginalMCCpakFile, ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak");

            LogLine("Creating copy to mod");
            File.Copy(ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak", ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak");

            LogLine("Modding copy");
            List<HexEditData> enableForge = new List<HexEditData>() {
            new HexEditData() { Position = 0x1E302110, Bytes = 0x27},
            new HexEditData() { Position = 0x1E2F52D0, Bytes = 0x27}};

            FileStuff.HexEdit(ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak", enableForge);
            LogLine("Forge can now be enabled by pressing \"Modded\" button.");
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

            if (lstHRModdedMaps.Items.Count == 0)
            {
                LogLine("No modded maps found.");
                btnHRSwapModded.Enabled = true;
                return;
            }

            if (lstHRModdedMaps.SelectedItem == null || lstHRVanillaMaps.SelectedItem == null)
            {
                LogLine("Please select a map from each list.");
                btnHRSwapModded.Enabled = true;
                return;
            }

            string vanMap = lstHRVanillaMaps.SelectedItem.ToString();
            string modMap = lstHRModdedMaps.SelectedItem.ToString();
            //check if vanilla map is backed up
            if (!File.Exists(ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap))
            {
                //create backup
                Log("Backing up vanilla map \"" + vanMap + "\"");
                File.Copy(MCCLocation + "\\haloreach\\maps\\" + vanMap, ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap);
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

            if (lstHRVanillaMaps.SelectedItem == null)
            {
                LogLine("Please select a map from the vanilla list.");
                btnHRSwapVanilla.Enabled = true;
                return;
            }

            string vanMap = lstHRVanillaMaps.SelectedItem.ToString();

            //check if vanilla map is backed up
            if (!File.Exists(ModsLocation + "\\Vanilla\\Halo Reach\\maps\\" + vanMap))
            {
                //File has not been modded
                LogLine("Vanilla map \"" + vanMap + "\" has never been swaped.");
                btnHRSwapVanilla.Enabled = true;
                return;
            }

            //delete hard linked file from vanilla map folder
            File.Delete(MCCLocation + "\\haloreach\\maps\\" + vanMap);
            HaloReach.ReloadMaps();
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

            if (lstHRModdedMaps.Items.Count == 0)
            {
                LogLine("No modded maps found.");
                btnHRLoadQuick.Enabled = true;
                return;
            }

            if (lstHRModdedMaps.SelectedItem == null || lstHRVanillaMaps.SelectedItem == null)
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

            HaloReach.ReloadMaps();

            SwapData data = new SwapData();

            data.ModdedFileName = modMap;
            data.ModdedFilePath = HaloReach.ModdedMaps[modMap];
            data.VanillaFileName = vanMap;
            data.VanillaFilePath = HaloReach.VanillaBackupMaps[vanMap];

            if (!HaloReach.SwapData.ContainsKey(vanMap))
                HaloReach.SwapData.Add(vanMap, data);
            else
            {
                HaloReach.SwapData.Remove(vanMap);
                HaloReach.SwapData.Add(vanMap, data);
                LogLine("Overwrote existing swap.");
            }
            UpdateSwapList();
            //create hard link to backup file
            bool t = FileUtil.CreateHardLink(HaloReach.VanillaMaps[vanMap], HaloReach.VanillaBackupMaps[vanMap]);
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
            HaloReach.SwapData.Remove(swapName);
            UpdateSwapList();
        }

        private void btnHRDeleteAll_Click(object sender, EventArgs e)
        {
            HaloReach.SwapData.Clear();
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
            Log("Nothing");
        }

        private void btnDeleteTempSwap_Click(object sender, EventArgs e)
        {

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


    }
}
