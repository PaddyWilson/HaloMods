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
            HaloReach = new GameData(MCCLocation + "\\haloreach\\maps", ModsLocation +"\\Vanilla\\Halo Reach\\Maps", ModsLocation + "\\Mods\\Halo Reach\\Maps");

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
                FileUtil.CreateHardLink(ModsLocation + "\\Vanilla\\MCC-WindowsNoEditor.pak", MCCLocation + OriginalMCCpakFile);
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
                FileUtil.CreateHardLink(ModsLocation + "\\Mods\\MCC-WindowsNoEditor.pak", MCCLocation + OriginalMCCpakFile);
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

        private void btnHRSwapModded_Click(object sender, EventArgs e)
        {
            btnHRSwapModded.Enabled = false;

            LogLine(lstHRVanillaMaps.SelectedItem.ToString());
        }
    }
}
