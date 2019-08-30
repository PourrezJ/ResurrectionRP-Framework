using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AltV.Net;

namespace ResurrectionRP_Server.Loader.TattooLoader
{
    class TattooLoader
    {
        private static string basePath = $"tattoos{Path.DirectorySeparatorChar}";

        public static List<Tattoo> TorsoTattooList = new List<Tattoo>();
        public static List<Tattoo> HeadTattooList = new List<Tattoo>();
        public static List<Tattoo> LeftArmTattooList = new List<Tattoo>();
        public static List<Tattoo> RightArmTattooList = new List<Tattoo>();
        public static List<Tattoo> LeftLegTattooList = new List<Tattoo>();
        public static List<Tattoo> RightLegTattooList = new List<Tattoo>();

        public static Task LoadAllTattoo()
        {
            Alt.Server.LogInfo("[TattooManager] Loading all Tattoo...");

            string[] files = Directory.GetFiles(MakePath(""), "*.json");
            foreach (var file in files)
            {
                try
                {
                    Tattoo[] Tattoolist = Get(Path.GetFileName(file));
                    if (Tattoolist == null) continue;

                    foreach (Tattoo Tattoo in Tattoolist)
                    {
                        if (Tattoo != null)
                        {
                            switch (Tattoo.ZoneID)
                            {
                                case 0:
                                    TorsoTattooList.Add(Tattoo);
                                    break;

                                case 1:
                                    HeadTattooList.Add(Tattoo);
                                    break;

                                case 2:
                                    LeftArmTattooList.Add(Tattoo);
                                    break;

                                case 3:
                                    RightArmTattooList.Add(Tattoo);
                                    break;

                                case 4:
                                    LeftLegTattooList.Add(Tattoo);
                                    break;

                                case 5:
                                    RightLegTattooList.Add(Tattoo);
                                    break;

                                default:
                                    Alt.Server.LogError ("[TattooManager] ID unknown.");
                                    break;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("[TattooManager] ERROR: " + ex);
                    continue;
                }
            }

            Alt.Server.LogInfo ("[TattooManager] Loading completed!");
            return Task.CompletedTask;
        }

        private static Tattoo[] Get(string filename)
        {
            string path = MakePath(filename);
            if (!File.Exists(path))
            {
                Alt.Server.LogError("[TattooManager] Could not find '" + path + "'");
                return null;
            }

            try
            {
                Tattoo[] TattooManifest = JsonConvert.DeserializeObject<Tattoo[]>(File.ReadAllText(path));
                foreach (var Tattoo in TattooManifest)
                {
                    Tattoo.Collection = filename.Split('.')[0];
                }

                return TattooManifest;
            }
            catch (JsonReaderException e)
            {
                Alt.Server.LogError("[TattooManager] An error occured while reading '" + path + "': " + e.Message);
                return null;
            }
        }

        internal static string MakePath() => MakePath("");

        internal static string MakePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(basePath, relativePath));
        }
    }
}
