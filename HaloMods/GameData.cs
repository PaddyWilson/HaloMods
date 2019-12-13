using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HaloMods
{
    public class SwapData
    {
        //data of the original file
        public string VanillaFilePath;
        public string VanillaFileName;
        //data about where the original is now 
        public string NewFilePath;
        public string NewFileName;
        //data about modded file
        public string ModdedFilePath;
        public string ModdedFileName;

        //TODO: remove
        public bool OtherFile = false;
        public SwapData() { }

        public SwapData(string OriginalFile, string NewFile, string ModdedFile)
        {
            VanillaFilePath = OriginalFile;
            VanillaFileName = FileUtil.GetFileName(OriginalFile);
            NewFilePath = NewFile;
            NewFileName = FileUtil.GetFileName(NewFile);
            ModdedFilePath = ModdedFile;
            ModdedFileName = FileUtil.GetFileName(ModdedFile);
        }

        public bool MoveOriginalFile(bool DeleteIfExists = false)
        {
            //OG file already backed up
            if (File.Exists(NewFilePath) && !DeleteIfExists)
            {
                FileUtil.CreateHardLink(VanillaFilePath, NewFilePath);
                return true;
            }

            if (FileUtil.IsFileInUse(VanillaFilePath))
                return false;//can't backup 

            if (DeleteIfExists)
                File.Delete(NewFilePath);

            File.Move(VanillaFilePath, NewFilePath);
            FileUtil.CreateHardLink(VanillaFilePath, NewFilePath);

            return true;
        }

        public bool SwapToModded()
        {
            if (FileUtil.IsFileInUse(VanillaFilePath))
                return false;

            try
            {
                File.Delete(VanillaFilePath);
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;//cant delete file
            }

            FileUtil.CreateHardLink(VanillaFilePath, ModdedFilePath);
            return true;
        }

        public bool SwapToVanilla()
        {
            if (FileUtil.IsFileInUse(VanillaFilePath))
                return false;

            try
            {
                File.Delete(VanillaFilePath);
            }
            catch (System.UnauthorizedAccessException)
            {
                return false;//cant delete file
            }

            FileUtil.CreateHardLink(VanillaFilePath, NewFilePath);
            return true;
        }

        public bool RestoreOriginalFiles()
        {
            if (FileUtil.IsFileInUse(VanillaFilePath))
                return false;

            if (!File.Exists(NewFilePath))
                return false;

            if (File.Exists(VanillaFilePath))//Vanilla file may have already been delete somehow
                try
                {
                    File.Delete(VanillaFilePath);
                }
                catch (Exception)
                {
                    return false;
                }

            File.Move(NewFilePath, VanillaFilePath);
            return true;
        }
    }

    public class GameData
    {
        private string SaveFileName;// = "HaloReach.json";

        public string VanillaMapLocation;
        public string VanillaBackupMapLocation;
        public string ModdedMapLocation;

        public Dictionary<string, string> VanillaMaps = new Dictionary<string, string>();
        public Dictionary<string, string> VanillaBackupMaps = new Dictionary<string, string>();
        public Dictionary<string, string> ModdedMaps = new Dictionary<string, string>();

        public Dictionary<string, SwapData> SwapData { get; private set; }// = new Dictionary<string, SwapData>();

        public GameData(string SaveFileName, string VanillaMapLocation, string VanillaBackupMapLocation, string ModdedMapLocation)
        {
            SwapData = new Dictionary<string, SwapData>();

            this.SaveFileName = SaveFileName;
            this.ModdedMapLocation = ModdedMapLocation;
            this.VanillaBackupMapLocation = VanillaBackupMapLocation;
            this.VanillaMapLocation = VanillaMapLocation;

            VanillaMaps = FileUtil.GetDicOfFiles(VanillaMapLocation);
            VanillaBackupMaps = FileUtil.GetDicOfFiles(VanillaBackupMapLocation);
            ModdedMaps = FileUtil.GetDicOfFiles(ModdedMapLocation);
        }

        public void ReloadMaps()
        {
            //VanillaMaps = FileUtil.GetDicOfFiles(MapLocationVanilla);
            VanillaBackupMaps = FileUtil.GetDicOfFiles(VanillaBackupMapLocation);
            ModdedMaps = FileUtil.GetDicOfFiles(ModdedMapLocation);
        }

        public void SaveSwapData()
        {
            string data = JsonConvert.SerializeObject(SwapData);
            File.WriteAllText(SaveFileName, data);
        }
        public void LoadSwapData()
        {
            if (File.Exists(SaveFileName))
            {
                string data = File.ReadAllText(SaveFileName);
                SwapData = JsonConvert.DeserializeObject<Dictionary<string, SwapData>>(data);
            }
        }

        public void SwapToModded(string key)
        {
            if (!SwapData.ContainsKey(key))
                return;

            SwapData[key].SwapToModded();
        }

        public int SwapToModded()
        {
            int count = 0;
            foreach (var item in SwapData)
            {
                if (item.Value.SwapToModded())
                    count++;
                //try
                //{
                //    File.Delete(SwapData[item].VanillaFilePath);
                //    FileUtil.CreateHardLink(SwapData[item].VanillaFilePath, SwapData[item].ModdedFilePath);
                //}
                //catch (Exception) { }
            }
            return count;
        }

        public void SwapToVanilla(string key)
        {
            if (!SwapData.ContainsKey(key))
                return;

            SwapData[key].SwapToVanilla();
        }

        public int SwapToVanilla()
        {
            int count = 0;
            foreach (var item in SwapData)
            {
                if (item.Value.SwapToVanilla())
                    count++;
                //try
                //{
                //    File.Delete(SwapData[item].VanillaFilePath);
                //    FileUtil.CreateHardLink(SwapData[item].VanillaFilePath, SwapData[item].NewFilePath);
                //}
                //catch (Exception) { }
            }
            return count;
        }

        // Restores the original file and removes the item from the swap list
        public void RestoreOriginal(string key, bool deleteEntry = true)
        {
            if (!SwapData.ContainsKey(key))
                return;

            SwapData[key].RestoreOriginalFiles();

            //if (File.Exists(SwapData[key].VanillaFilePath))
            //    File.Delete(SwapData[key].VanillaFilePath);
            //if (File.Exists(SwapData[key].NewFilePath))
            //    File.Move(SwapData[key].NewFilePath, SwapData[key].VanillaFilePath);

            if (deleteEntry)
                SwapData.Remove(key);
        }

        public int RestoreAllOrigianlFiles()
        {
            List<string> done = new List<string>();
            int count = 0;

            foreach (var item in SwapData)
            {
                if (item.Value.RestoreOriginalFiles())
                {
                    count++;
                    done.Add(item.Key);
                }
            }

            foreach (var item in done)
            {
                SwapData.Remove(item);
            }

            return count;
        }

        public bool AddMapSwapData(string key, string VanillaFileName, string ModdedFileName)
        {
            SwapData data = new SwapData();
            data.VanillaFileName = VanillaFileName;
            data.VanillaFilePath = VanillaMapLocation + "\\" + VanillaFileName;

            data.NewFileName = VanillaFileName;
            data.NewFilePath = VanillaBackupMapLocation + "\\" + VanillaFileName;

            data.ModdedFileName = ModdedFileName;
            data.ModdedFilePath = ModdedMapLocation + "\\" + ModdedFileName;

            return AddSwapData(key, data);
        }

        public bool AddSwapData(string key, string VanillaFilePath, string NewFilePath, string ModdedFilePath)
        {
            if (!File.Exists(VanillaFilePath) || !File.Exists(ModdedFilePath))
            {
                return false;
            }

            string[] temp = VanillaFilePath.Split('\\');
            string VanillaFileName = temp[temp.Length - 1];
            temp = ModdedFilePath.Split('\\');
            string ModdedFileName = temp[temp.Length - 1];

            SwapData data = new SwapData();
            data.VanillaFileName = VanillaFileName;
            data.VanillaFilePath = VanillaFilePath;

            data.NewFileName = VanillaFileName;
            data.NewFilePath = NewFilePath;

            data.ModdedFileName = ModdedFileName;
            data.ModdedFilePath = ModdedFilePath;

            data.OtherFile = true;

            return AddSwapData(key, data);
        }

        public bool AddSwapData(string key, SwapData data)
        {
            if (SwapData.ContainsKey(key))
                return false;

            //move og file
            if (!File.Exists(data.NewFilePath))
                File.Move(data.VanillaFilePath, data.NewFilePath);
            FileUtil.CreateHardLink(data.VanillaFilePath, data.NewFilePath);

            SwapData.Add(key, data);

            return true;
        }
    }
}
