﻿using System;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HaloMods());

            string HaloInstallFolder = @"D:/- Halo/Clean/";
            string VanillaFolder = HaloInstallFolder + @"MODS/Vanilla Files/";
            string ModsFolder = HaloInstallFolder + @"MODS/Mods/";

            List<string> IgnoreFolders = new List<string>();
            List<string> IgnoreFiles = new List<string>();
            List<string> FilesToMod = new List<string>();

            bool CreateBackups = true;
            bool IsModded = false;
            string txtFileSettings = "settings.txt";

            bool EnableForge = true;
            bool RemoveStartUpVideo = true;

            //read settings
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
                        HaloInstallFolder = split[1];
                    else if (split[0] == "vanilla-folder")
                        VanillaFolder = split[1];
                    else if (split[0] == "mods-folder")
                        ModsFolder = split[1];
                    else if (split[0] == "ignore-folder")
                        IgnoreFolders.Add(split[1]);
                    else if (split[0] == "ignore-file")
                        IgnoreFiles.Add(split[1]);
                    else if (split[0] == "create-backups")
                        CreateBackups = bool.Parse(split[1]);
                    else if (split[0] == "EnableForge")
                        EnableForge = bool.Parse(split[1]);
                    else if (split[0] == "RemoveStartUpVideo")
                        RemoveStartUpVideo = bool.Parse(split[1]);
                    else
                        continue;
                }

                VanillaFolder = HaloInstallFolder + VanillaFolder;
                ModsFolder = HaloInstallFolder + ModsFolder;
                for (int i = 0; i < IgnoreFolders.Count; i++)
                {
                    IgnoreFolders[i] = HaloInstallFolder + IgnoreFolders[i];
                }
            }

            //check if the install folder is valid
            if (!File.Exists(HaloInstallFolder + "mcclauncher.exe"))
            {
                Console.WriteLine("MCC is not installed in this directory. \r\n" + HaloInstallFolder);
                Console.WriteLine("Please change it in the settings file and rerun program.");
                Console.ReadKey();
                return;
            }

            //create folders to use
            Directory.CreateDirectory(VanillaFolder);
            Directory.CreateDirectory(ModsFolder);

            if (args.Length == 0)//get user input
            {

                Console.WriteLine("Enter 1 for Vanilla or 2 for Modded");
                ConsoleKeyInfo Input = Console.ReadKey();
                Console.WriteLine("");
                if (Input.KeyChar == '1')
                    IsModded = false;
                else if (Input.KeyChar == '2')
                    IsModded = true;
                else
                {
                    Console.WriteLine("Invalid input. Settings up Vanilla");
                    IsModded = false;
                }
            }
            else//has args passed in
            {
                if (args[0] == "1")
                    IsModded = false;
                else if (args[0] == "2")
                    IsModded = true;
                else
                {
                    Console.WriteLine("Invalid Arguments. Please only use 1 for Vanilla or 2 for Modded. Setting up Vanilla");
                    IsModded = false;
                }
            }

            //Console.WriteLine("Moding pak");
            //List<HexEditData> enableForge = new List<HexEditData>() {
            //new HexEditData() { Position = 0x1E302110, Bytes = 0x27},
            //new HexEditData() { Position = 0x1E2F52D0, Bytes = 0x27}};

            ////string loco = @"D:\- Halo\Clean\MODS\Mods\MCC-WindowsNoEditor.pak";
            //string loco = @"C:\Users\GGGGG\Desktop\Halo\swap.py";
            //FileStuff.HexEdit(loco, enableForge);
            //Console.WriteLine("Moding pak");
            ////

            Console.WriteLine("Reading files. Ignoring {0} folders and {1} files", IgnoreFolders.Count, IgnoreFiles.Count);
            //Get Dictionary of all files and their paths
            Dictionary<string, string> Files = FileStuff.GetListOfAllFiles(HaloInstallFolder, IgnoreFolders.ToArray(), IgnoreFiles.ToArray());
            Console.WriteLine("Files Found - " + Files.Count);

            string FileLocation = VanillaFolder;
            if (IsModded)
                FileLocation = ModsFolder;

            FilesToMod = FileStuff.GetListOfFiles(FileLocation);

            //create backups            
            if (CreateBackups)
            {
                Console.WriteLine("Creating Backups");
                foreach (var item in FilesToMod)
                {
                    //check if backed up
                    if (File.Exists(VanillaFolder + item)) continue;
                    Console.WriteLine("Backing up \"{0}\"", item);
                    //if not make backup
                    if (Files.ContainsKey(item))
                        File.Copy(Files[item], VanillaFolder + item, true);
                }
            }

            Console.WriteLine("Swaping " + FilesToMod.Count + " Files To " + (IsModded ? "Modded" : "Vanilla"));
            foreach (var item in FilesToMod)
            {
                //check if file exits
                if (!File.Exists(FileLocation + item))
                {
                    Console.WriteLine("ERROR:" + item + " does not exist");
                    continue;
                }
                if (!Files.ContainsKey(item))
                {
                    Console.WriteLine("ERROR:" + item + " not in list of files");
                    continue;
                }

                Console.WriteLine(FileLocation + item);
                try
                {
                    File.Delete(Files[item]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR DELETING FILE \"{0}\" - {1}", Files[item], e.Message);
                    continue;
                    //throw;
                }

                FileStuff.CreateHardLink(Files[item], FileLocation + item, IntPtr.Zero);
            }
            Console.WriteLine("Done. Press any key to continue...");
            Console.ReadKey();
        }
    }

    public class FileStuff
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(
          string lpFileName,
          string lpExistingFileName,
          IntPtr lpSecurityAttributes
         );

        public static Dictionary<string, string> GetListOfAllFiles(string Dir, string[] IgnoreFolders, string[] IgnoreFiles)
        {
            List<string> DirsToSearch = new List<string>();
            DirsToSearch.Add(Dir);

            Dictionary<string, string> Files = new Dictionary<string, string>();

            while (DirsToSearch.Count != 0)
            {
                string CurrentFolder = DirsToSearch[0];
                if (IgnoreFolders.Contains(CurrentFolder))
                {
                    DirsToSearch.Remove(CurrentFolder);
                    continue;
                }

                //Console.WriteLine(CurrentFolder);
                //find files
                string[] FilesTemp = Directory.GetFiles(CurrentFolder);

                foreach (var item in FilesTemp)
                {
                    try
                    {
                        //ignore files with these names
                        if (IgnoreFiles.Contains(item.Remove(0, CurrentFolder.Length)))
                            continue;

                        Files.Add(item.Remove(0, CurrentFolder.Length), item);
                    }
                    catch (Exception)
                    {
                        // i dont care if this is considered bad

                        Console.WriteLine("Dupe Files \"" + item + "\" - \"" + item.Remove(0, CurrentFolder.Length) + "\" - " + Files[item.Remove(0, CurrentFolder.Length)]);
                        //throw;
                    }
                }

                string[] dirs = Directory.GetDirectories(CurrentFolder);
                foreach (var item in dirs)
                {
                    DirsToSearch.Add(item + "/");
                }
                //DirsToSearch.AddRange(dirs);
                DirsToSearch.Remove(CurrentFolder);
            }
            return Files;
        }//end GetListOfFiles

        public static List<string> GetListOfFiles(string Dir)
        {
            List<string> FilesNames = new List<string>();
            string[] TempFiles = Directory.GetFiles(Dir);
            foreach (var item in TempFiles)
            {
                FilesNames.Add(item.Remove(0, Dir.Length));
            }
            return FilesNames;
        }


        public static void HexEdit(string FileName, List<HexEditData> Data)
        {
            try
            {
                using (var stream = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    foreach (var item in Data)
                    {
                        stream.Position = item.Position;
                        stream.WriteByte(item.Bytes);
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("ERROR Can't hexedit file. File not found. \"{0}\"", FileName);
                //throw;
            }

        }
    }

    public struct HexEditData
    {
        public long Position;
        public byte Bytes;
    }
}
