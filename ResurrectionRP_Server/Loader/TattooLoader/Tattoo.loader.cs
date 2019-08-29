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
            Alt.Server.LogInfo("[TatooManager] Loading all tatoo...");

            string[] files = Directory.GetFiles(MakePath(""), "*.json");
            foreach (var file in files)
            {
                try
                {
                    Tattoo[] tatoolist = Get(Path.GetFileName(file));
                    if (tatoolist == null) continue;

                    foreach (Tattoo tatoo in tatoolist)
                    {
                        if (tatoo != null)
                        {
                            switch (tatoo.ZoneID)
                            {
                                case 0:
                                    TorsoTatooList.Add(tatoo);
                                    break;

                                case 1:
                                    HeadTatooList.Add(tatoo);
                                    break;

                                case 2:
                                    LeftArmTatooList.Add(tatoo);
                                    break;

                                case 3:
                                    RightArmTatooList.Add(tatoo);
                                    break;

                                case 4:
                                    LeftLegTatooList.Add(tatoo);
                                    break;

                                case 5:
                                    RightLegTatooList.Add(tatoo);
                                    break;

                                default:
                                    Alt.Server.LogError ("[TatooManager] ID unknown.");
                                    break;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("[TatooManager] ERROR: " + ex);
                    continue;
                }
            }

            Alt.Server.LogInfo ("[TatooManager] Loading completed!");
            return Task.CompletedTask;
        }

        private static Tattoo[] Get(string filename)
        {
            string path = MakePath(filename);
            if (!File.Exists(path))
            {
                Alt.Server.LogError("[TatooManager] Could not find '" + path + "'");
                return null;
            }

            try
            {
                Tattoo[] tatooManifest = JsonConvert.DeserializeObject<Tattoo[]>(File.ReadAllText(path));
                foreach (var tatoo in tatooManifest)
                {
                    tatoo.Collection = filename.Split('.')[0];
                }

                return tatooManifest;
            }
            catch (JsonReaderException e)
            {
                Alt.Server.LogError("[TatooManager] An error occured while reading '" + path + "': " + e.Message);
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
