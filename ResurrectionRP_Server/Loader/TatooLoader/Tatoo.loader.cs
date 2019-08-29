using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AltV.Net;

namespace ResurrectionRP_Server.Loader.TatooLoader
{
    class TatooLoader
    {
        private static string basePath = $"tattoos{Path.DirectorySeparatorChar}";

        public static List<Tatoo> TorsoTatooList = new List<Tatoo>();
        public static List<Tatoo> HeadTatooList = new List<Tatoo>();
        public static List<Tatoo> LeftArmTatooList = new List<Tatoo>();
        public static List<Tatoo> RightArmTatooList = new List<Tatoo>();
        public static List<Tatoo> LeftLegTatooList = new List<Tatoo>();
        public static List<Tatoo> RightLegTatooList = new List<Tatoo>();

        public static Task LoadAllTatoo()
        {
            Alt.Server.LogInfo("[TatooManager] Loading all tatoo...");

            string[] files = Directory.GetFiles(MakePath(""), "*.json");
            foreach (var file in files)
            {
                try
                {
                    Tatoo[] tatoolist = Get(Path.GetFileName(file));
                    if (tatoolist == null) continue;

                    foreach (Tatoo tatoo in tatoolist)
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

        private static Tatoo[] Get(string filename)
        {
            string path = MakePath(filename);
            if (!File.Exists(path))
            {
                Alt.Server.LogError("[TatooManager] Could not find '" + path + "'");
                return null;
            }

            try
            {
                Tatoo[] tatooManifest = JsonConvert.DeserializeObject<Tatoo[]>(File.ReadAllText(path));
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
