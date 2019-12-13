using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaloMods
{
    public struct HexEditData
    {
        public long Position;
        public byte Bytes;
    }

    public class FileUtil
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(
          string lpFileName,
          string lpExistingFileName,
          IntPtr lpSecurityAttributes
         );

        public static bool CreateHardLink(string ToFile, string FromFile)
        {
            return CreateHardLink(ToFile, FromFile, IntPtr.Zero);
        }

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

        public static Dictionary<string, string> GetDicOfFiles(string Dir)
        {
            Dictionary<string, string> FilesNames = new Dictionary<string, string>();
            string[] TempFiles = Directory.GetFiles(Dir);
            foreach (var item in TempFiles)
            {
                FilesNames.Add(item.Remove(0, Dir.Length + 1), item);
            }
            return FilesNames;
        }

        public static bool Exists(string FileName)
        {
            return File.Exists(FileName);
        }

        public static bool HexEdit(string FileName, List<HexEditData> Data)
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
            catch (FileNotFoundException)
            {
                Console.WriteLine("ERROR Can't hexedit file. File not found. \"{0}\"", FileName);
                return false;
            }
            return true;
        }

        public static string OpenDirectoryDiag()
        {
            string path = "";
            Thread t = new Thread((ThreadStart)(() =>
            {
                FolderBrowserDialog folder = new FolderBrowserDialog();

                if (folder.ShowDialog() == DialogResult.OK)
                {
                    path = folder.SelectedPath;
                }
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return path;
        }

        public static string OpenFileDiag()
        {
            string path = "";
            Thread t = new Thread((ThreadStart)(() =>
            {
                System.Windows.Forms.OpenFileDialog folder = new System.Windows.Forms.OpenFileDialog() { };

                if (folder.ShowDialog() == DialogResult.OK)
                {
                    path = folder.FileName;
                }
            }));

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return path;
        }

        public static bool IsFileInUse(string FileName)
        {
            FileInfo file = new FileInfo(FileName);
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        public static string GetFileName(string File)
        {
            if (File == "" || File == null)
                return "";

            string[] temp = File.Split('\\');
            return temp[temp.Length - 1];
        }
    }
}
