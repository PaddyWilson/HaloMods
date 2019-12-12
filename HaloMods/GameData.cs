using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloMods
{
    public struct SwapData
    {
        public string VanillaFilePath;
        public string VanillaFileName;

        public string ModdedFilePath;
        public string ModdedFileName;
    }

    public class GameData
    {
        public string MapLocationVanilla;
        public string MapLocationVanillaBackup;
        public string MapLocationModded;

        public Dictionary<string, string> VanillaMaps = new Dictionary<string, string>();
        public Dictionary<string, string> VanillaBackupMaps = new Dictionary<string, string>();
        public Dictionary<string, string> ModdedMaps = new Dictionary<string, string>();

        public Dictionary<string, SwapData> SwapData = new Dictionary<string, SwapData>();

        public GameData(string MapLocationVanilla, string MapLocationVanillaBackup, string MapLocationModded)
        {
            this.MapLocationModded = MapLocationModded;
            this.MapLocationVanillaBackup = MapLocationVanillaBackup;
            this.MapLocationVanilla = MapLocationVanilla;

            VanillaMaps = FileUtil.GetDicOfFiles(MapLocationVanilla);
            VanillaBackupMaps = FileUtil.GetDicOfFiles(MapLocationVanillaBackup);
            ModdedMaps = FileUtil.GetDicOfFiles(MapLocationModded);
        }


        public void ReloadMaps()
        {
            //VanillaMaps = FileUtil.GetDicOfFiles(MapLocationVanilla);
            VanillaBackupMaps = FileUtil.GetDicOfFiles(MapLocationVanillaBackup);
            ModdedMaps = FileUtil.GetDicOfFiles(MapLocationModded);
        }
    }
}
