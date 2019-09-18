
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace ResurrectionRP_Server.Loader
{

    public struct ClothManifest
    {
        [JsonProperty("Overlay")]
        public byte Overlay;

        [JsonProperty("DrawablesList")]
        public IDictionary<int, ClothDrawable> DrawablesList;
    }

    public struct ClothDrawable
    {
        [JsonProperty("Name")]
        public string Categorie;

        [JsonProperty("Price")]
        public int Price;

        [JsonProperty("Torso")]
        public int[] Torso;

        [JsonProperty("UnderShirt")]
        public int[] UnderShirt;

        [JsonProperty("Variations")]
        public IDictionary<int, ClothVariation> Variations;
    }

    public struct ClothVariation
    {
        [JsonProperty("GXT")]
        public string Gxt { get; set; }
    }

    public class ClothingLoader
    {
        public static Dictionary<byte, ClothManifest> ClothingsMaleList
            = new Dictionary<byte, ClothManifest>();

        public static ClothManifest ClothingsMaleTopsList
            = new ClothManifest();

        public static Dictionary<byte, ClothManifest> ClothingsFemaleList
            = new Dictionary<byte, ClothManifest>();

        public static ClothManifest ClothingsFemaleTopsList
            = new ClothManifest();

        public static ClothManifest MasksList
            = new ClothManifest();

        public static Dictionary<byte, ClothManifest> PropsMaleList
            = new Dictionary<byte, ClothManifest>();

        public static Dictionary<byte, ClothManifest> PropsFemaleList
            = new Dictionary<byte, ClothManifest>();

        private static string basePath = $"clothings{Path.DirectorySeparatorChar}";

        public static Task LoadAllCloth()
        {
            Alt.Server.LogInfo("[ClothingLoader] Loading all cloths file...");

            #region Cloths
            var maskFile = Path.GetFileName("masks.json");

            if (maskFile != null)
            {
                var cloths = Get(maskFile);

                if (cloths == null)
                    return Task.CompletedTask;

                MasksList = cloths.Value;
            }

            foreach (var file in Directory.GetFiles(MakePath(@"male/cloths"), "*.json"))
            {
                try
                {
                    var filename = Path.GetFileName(file);

                    if (Path.GetFileName(file) == "male_tops.json")
                    {
                        var cloths = Get(@"male/cloths/" + filename);

                        if (cloths == null)
                            continue;

                        ClothingsMaleTopsList = cloths.Value;
                    }
                    else
                    {
                        var cloths = Get(@"male/cloths/" + filename);

                        if (cloths == null)
                            continue;

                        ClothingsMaleList.Add(cloths.Value.Overlay, cloths.Value);
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"[ClothingLoader] ERROR FILE {file} : " + ex);
                    continue;
                }
            }

            foreach (var file in Directory.GetFiles(MakePath(@"female/cloths"), "*.json"))
            {
                try
                {
                    var filename = Path.GetFileName(file);

                    if (Path.GetFileName(file) == "female_tops.json")
                    {
                        var cloths = Get(@"female/cloths/" + filename);
                        if (cloths == null)
                            continue;

                        ClothingsFemaleTopsList = cloths.Value;
                    }
                    else
                    {
                        var cloths = Get(@"female/cloths/" + Path.GetFileName(file));
                        if (cloths == null)
                            continue;

                        ClothingsFemaleList.Add(cloths.Value.Overlay, cloths.Value);
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogInfo($"[ClothingLoader] ERROR FILE {file} : " + ex);
                    continue;
                }
            }
            #endregion

            Alt.Server.LogInfo("[ClothingLoader] Loading all props file...");

            #region Props
            foreach (var file in Directory.GetFiles(MakePath(@"male/props"), "*.json"))
            {
                try
                {
                    var cloths = Get(@"male/props/" + Path.GetFileName(@"male/props" + file));

                    if (cloths == null)
                        continue;

                    PropsMaleList.Add(cloths.Value.Overlay, cloths.Value);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"[ClothingLoader] ERROR FILE {file} : " + ex);
                    continue;
                }
            }

            foreach (var file in Directory.GetFiles(MakePath(@"female/props"), "*.json"))
            {
                try
                {
                    var cloths = Get(@"female/props/" + Path.GetFileName(file));

                    if (cloths == null)
                        continue;

                    PropsFemaleList.Add(cloths.Value.Overlay, cloths.Value);
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError($"[ClothingLoader] ERROR FILE {file} : " + ex);
                    continue;
                }
            }
            #endregion

            Alt.Server.LogInfo("[ClothingLoader] Loading completed!");
            return Task.CompletedTask;
        }

        private static ClothManifest? Get(string filename)
        {
            string path = MakePath(filename);
            if (!File.Exists(path))
            {
                Alt.Server.LogError("[ClothingLoader] Could not find '" + path + "'");
                return null;
            }

            return JsonConvert.DeserializeObject<ClothManifest>(File.ReadAllText(path));
        }
        internal static string MakePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(basePath, relativePath));
        }

        public static ClothManifest? FindTops(IPlayer player)
        {
            if (player.Model == Alt.Hash("mp_m_freemode_01"))
            {
                return ClothingsMaleTopsList;
            }
            else if (player.Model == Alt.Hash("mp_f_freemode_01"))
            {
                return ClothingsFemaleTopsList;
            }

            return null;
        }

        public static ClothManifest? FindCloths(IPlayer player, byte compomentID)
        {
            if (compomentID == 1)
            {
                return MasksList;
            }

            if (player.Model == Alt.Hash("mp_m_freemode_01"))
            {
                if (ClothingsMaleList.TryGetValue(compomentID, out ClothManifest value))
                {
                    return value;
                }
            }
            else if (player.Model == Alt.Hash("mp_f_freemode_01"))
            {
                if (ClothingsFemaleList.TryGetValue(compomentID, out ClothManifest value))
                {
                    return value;
                }
            }

            return null;
        }

        public static ClothManifest? FindProps(IPlayer player, byte compomentID)
        {
            if (player.Model == Alt.Hash("mp_m_freemode_01"))
            {
                if (PropsMaleList.TryGetValue(compomentID, out ClothManifest value))
                {
                    return value;
                }
            }
            else if (player.Model == Alt.Hash("mp_f_freemode_01"))
            {
                if (PropsFemaleList.TryGetValue(compomentID, out ClothManifest value))
                {
                    return value;
                }
            }

            return null;
        }

        /*
        public static void test()
        {
            string[] files = Directory.GetFiles(MakePath("female\\cloths\\"), "*.json");
            foreach (var file in files)
            {
                try
                {
                    string path = MakePath("female\\cloths\\" + Path.GetFileName(file));

                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName != "female_tops")
                        continue;

                    var manifest = JsonConvert.DeserializeObject<ClothManifest>(File.ReadAllText(path));

                    for(int i = 0; i < manifest.DrawablesList.Count; i++)
                    {
                        var manif = manifest.DrawablesList[i];
                        manif.Torso = new int[8] { 14, 28, 39, 50, 61, 72, 83, 94 };
                        manifest.DrawablesList[i] = manif;
                    }

                    string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

                    //write string to file
                    var aaaa = Path.GetDirectoryName(MakePath("female\\cloths\\female_tops.json"));
                    File.WriteAllText(Path.GetFullPath(Path.Combine(basePath, "female\\cloths\\new\\female_tops.json")), json);

                }
                catch (Exception ex)
                {
                    MP.Logger.Error("[ClothingLoader] ERROR: " + ex);
                    continue;
                }
            }
        }

        /*
        public static async Task NameUpdate(IPlayer player)
        {
            MP.Logger.Info("[ClothingLoader] Modify all cloths files...");

            string[] files = Directory.GetFiles(MakePath(""), "*.json");
            foreach (var file in files)
            {
                try
                {
                    var cloths = Get(Path.GetFileName(file));
                    if (cloths == null)
                        continue;

                    for (int i = 0; i < cloths.Count; i++)
                    {
                        var data = cloths[i];
                        
                        for (int a = 0; a < data.Variations.Count; a++)
                        {
                            await player.CallAsync("ClothClientNameUpdate", 6, i, a, data.Variations[a].Gxt, Path.GetFileNameWithoutExtension(file));
                            await Task.Delay(75);
                        }
                    }

                    await Save(Path.GetFileNameWithoutExtension(file));

                }
                catch (Exception ex)
                {
                    MP.Logger.Error("[ClothingLoader] ERROR: " + ex);
                    continue;
                }
            }
        }

        private static Task ClothNameUpdate(object sender, PlayerRemoteEventEventArgs arg)
        {
            var compoment = (int)arguments[0];
            var drawable = (int)arguments[1];
            var variations = (int)arguments[2];
            var gtx = (string)arguments[3];
            var file = (string)arguments[4];

            ClothingsList[file][drawable].Variations[variations].Gxt = gtx;



            return Task.CompletedTask;
        }

        public static async Task Save(string file)
        {
            var clothData = ClothingLoader.FindCloths(file);

            var manifest = new NewClothManifest();

            manifest.Overlay = 6;

            Dictionary<int, NewClothDrawable> drawables = new Dictionary<int, NewClothDrawable>(); 

            for (int i = 0; i < clothData.Count; i++)
            {
                var data = clothData[i];

                var drawable = new NewClothDrawable();

                drawable.Price = data.Price;


                Dictionary<int, NewClothVariation> variations = new Dictionary<int, NewClothVariation>();

                foreach(var varia in data.Variations)
                {
                    variations.Add(varia.Key, new NewClothVariation() { Gxt = varia.Value.Gxt });
                }


                drawable.Variations = variations;

                drawables.Add(i, drawable);
            }

            manifest.DrawablesList = drawables;



            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

            //write string to file
            var aaaa = Path.GetDirectoryName(MakePath(""));
            File.WriteAllText($"{aaaa}\\new\\{file}.json", json);
        }*/

        /*
        public void MakeJson(string filename)
        {
            try
            {
                Dictionary<int, NewClothManifest> test = new Dictionary<int, NewClothManifest>();
                var clothManifest = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, ClothManifest>>>(File.ReadAllText(path));

                for (int a = 0; a < clothManifest.Count; a++)
                {
                    try
                    {
                        var bite = clothManifest[a];

                        var babouche = new NewClothManifest();

                        int price = 0;

                        for (int b = 0; b < bite.Count; b++)
                        {
                            var pouette = bite[b];
                            babouche.Variations.Add(b, new ClothVariation() { Gxt = pouette.Gxt });

                            price = pouette.Price;
                        }
                        babouche.Price = price;
                        test.Add(a, babouche);
                    }
                    catch (Exception ex)
                    {
                        MP.Logger.Error($"Error {filename}", ex);
                    }
                }

                string json = JsonConvert.SerializeObject(test);

                //write string to file
                var aaaa = Path.GetDirectoryName(path);
                File.WriteAllText($"{aaaa}\\new\\{filename}", json);

                return clothManifest;


                return null;
            }
            catch (JsonReaderException e)
            {
                MP.Logger.Error("[ClothingLoader] An error occured while reading '" + path + "': " + e.Message);
                return null;
            }
        }*/
    }
}
