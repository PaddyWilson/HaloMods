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
        public string VanillaFilePath;
        public string VanillaFileName;

        public string NewFilePath;
        public string NewFileName;

        public string ModdedFilePath;
        public string ModdedFileName;

        public bool OtherFile = false;
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

            File.Delete(SwapData[key].VanillaFilePath);
            FileUtil.CreateHardLink(SwapData[key].VanillaFilePath, SwapData[key].ModdedFilePath);
        }

        public void SwapToModded()
        {
            foreach (var item in SwapData.Keys)
            {
                File.Delete(SwapData[item].VanillaFilePath);
                FileUtil.CreateHardLink(SwapData[item].VanillaFilePath, SwapData[item].ModdedFilePath);
            }
        }

        public void SwapToVanilla(string key)
        {
            if (!SwapData.ContainsKey(key))
                return;

            File.Delete(SwapData[key].VanillaFilePath);
            FileUtil.CreateHardLink(SwapData[key].VanillaFilePath, SwapData[key].NewFilePath);
        }

        public void SwapToVanilla()
        {
            foreach (var item in SwapData.Keys)
            {
                File.Delete(SwapData[item].VanillaFilePath);
                FileUtil.CreateHardLink(SwapData[item].VanillaFilePath, SwapData[item].NewFilePath);
            }

        }

        // Restores the original file and removes the item from the swap list
        public void RestoreOriginal(string key, bool deleteEntry = true)
        {
            if (!SwapData.ContainsKey(key))
                return;

            File.Delete(SwapData[key].VanillaFilePath);
            File.Move(SwapData[key].NewFilePath, SwapData[key].VanillaFilePath);
            if (deleteEntry)
                SwapData.Remove(key);
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
