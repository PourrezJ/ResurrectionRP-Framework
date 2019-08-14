using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Models
{

    public class HairData
    {
        public int Hair;
        public int Color;
        public int HighlightColor;

        public HairData(byte hair, byte color, byte highlightcolor)
        {
            Hair = hair;
            Color = color;
            HighlightColor = highlightcolor;
        }
    }

    public class HeadBlend
    {
        public int ShapeFirst;
        public int ShapeSecond;
        public int ShapeThird;
        public int SkinFirst;
        public int SkinSecond;
        public int SkinThird;

        public float ShapeMix;
        public float SkinMix;
        public float ThirdMix;
    }

    public class HeadOverlay
    {
        public int Index;
        public float Opacity;
        public int Color;
        public int SecondaryColor;
    }



    public class PlayerCustomization
    {
        // Player
        public int Gender;

        // Parents
        public HeadBlend Parents;

        // Features
        public float[] Features = new float[20];

        // Appearance
        public HeadOverlay[] Appearance = new HeadOverlay[10];

        // Tatouages
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, int> Decorations = new Dictionary<int, int>();

        // Hair & Colors
        public HairData Hair;

        public int EyeColor;

        public PlayerCustomization()
        {
            Gender = 0;
            Parents = new HeadBlend();
            for (int i = 0; i < Features.Length; i++) Features[i] = 0;
            for (int i = 0; i < Appearance.Length; i++) Appearance[i] = new HeadOverlay();
            Hair = new HairData(0, 0, 0);
        }

        public void ApplyCharacter(IPlayer player)
        {
            if (!player.Exists)
                return;

             AltAsync.Do(() =>
            {
                try
                {
                    if (player.Model != ((Gender == 0) ? (uint)AltV.Net.Enums.PedModel.FreemodeMale01 : (uint)AltV.Net.Enums.PedModel.FreemodeFemale01))
                        player.Model = (Gender == 0) ? (uint)AltV.Net.Enums.PedModel.FreemodeMale01 : (uint)AltV.Net.Enums.PedModel.FreemodeFemale01;

                    //player.SetCloth(ClothSlot.Hair, new ClothData(Hair.Hair, 0, 0));
                    player.Emit("ComponentVariation", 2, Hair.Hair, 0, 0);
                    //player.SetHairColor(Hair.Color, Hair.HighlightColor);
                    player.Emit("HairVariation", Hair.Color, Hair.HighlightColor);
                    //player.SetEyeColor((uint)EyeColor);
                    player.Emit("EyeColorVariation", (uint)EyeColor);
                    //player.SetHeadBlend(new HeadBlendData(Parents.ShapeFirst, Parents.ShapeSecond, Parents.ShapeThird, Parents.SkinFirst, Parents.SkinSecond, Parents.SkinThird, Parents.ShapeMix, Parents.SkinMix, Parents.ThirdMix));
                    player.Emit("HeadVariation", Parents.ShapeFirst, Parents.ShapeSecond, Parents.ShapeThird, Parents.SkinFirst, Parents.SkinSecond, Parents.SkinThird, (float)Parents.ShapeMix, (float)Parents.SkinMix, (float)Parents.ThirdMix);
                    for (int i = 0; i < Features.Length; i++)
                        //player.SetFaceFeature(i, Features[i]);
                        player.Emit("FaceFeatureVariation", i, Features[i]);

                    for (int i = 0; i < Appearance.Length; i++)
                        //player.SetHeadOverlay(i, new HeadOverlayData(Appearance[i].Index, Appearance[i].Opacity, Appearance[i].Color, Appearance[i].SecondaryColor));
                        player.Emit("HeadOverlayVariation", Appearance[i].Index, Appearance[i].Opacity, Appearance[i].Color, Appearance[i].SecondaryColor);

                    if (Decorations != null && Decorations.Count > 0)
                    {
                        //player.SetDecorations(Decorations);
                        foreach (KeyValuePair<int, int> deco in Decorations)
                            player.Emit("DecorationVariation", deco.Key, deco.Value);
                    }
                }
                catch (Exception ex)
                {
                    Alt.Server.LogInfo("ApplyCharacter" +ex.Data);
                }
            });
        }
    }
}
