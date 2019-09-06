using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using AltV.Net.Async;
using AltV.Net;
using ResurrectionRP_Server.Entities.Players.Data;

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
        public List<Decoration> Decorations = new List<Decoration>();

        // Hair & Colors
        public HairData Hair;

        public int EyeColor;

        public PlayerCustomization()
        {
            Gender = 0;
            Parents = new HeadBlend();

            for (int i = 0; i < Features.Length; i++)
                Features[i] = 0;
            for (int i = 0; i < Appearance.Length; i++)
                Appearance[i] = new HeadOverlay();

            Hair = new HairData(0, 0, 0);
        }

        public void ApplyCharacter(IPlayer player)
        {
            if (!player.Exists)
                return;

            try
            {
                if (player.Model != ((Gender == 0) ? (uint)AltV.Net.Enums.PedModel.FreemodeMale01 : (uint)AltV.Net.Enums.PedModel.FreemodeFemale01))
                    player.Model = (Gender == 0) ? (uint)AltV.Net.Enums.PedModel.FreemodeMale01 : (uint)AltV.Net.Enums.PedModel.FreemodeFemale01;

                player.Emit("HeadVariation", Parents.ShapeFirst, Parents.ShapeSecond, Parents.ShapeThird, Parents.SkinFirst, Parents.SkinSecond, Parents.SkinThird, Parents.ShapeMix, Parents.SkinMix, Parents.ThirdMix);

                for (int i = 0; i < Features.Length; i++)
                    player.Emit("FaceFeatureVariation", i, Features[i]);

                for (int i = 0; i < Appearance.Length; i++)
                {
                    player.Emit("HeadOverlayVariation", Appearance[i].Index, Appearance[i].Opacity, Appearance[i].Color, Appearance[i].SecondaryColor, i);
                }

                foreach (Decoration decoration in Decorations)
                    player.Emit("DecorationVariation", decoration.Collection, decoration.Overlay);

                player.Emit("EyeColorVariation", (uint)EyeColor);
                player.Emit("ComponentVariation", 2, Hair.Hair, 0, 0);
                player.Emit("HairVariation", Hair.Color, Hair.HighlightColor);
            }
            catch (Exception ex)
            {
                Alt.Server.LogInfo("ApplyCharacter" + ex.Data);
            }
        }

        public bool HasDecoration(int overlay)
        {
            if (Decorations == null)
                return false;

            foreach (Decoration decoration in Decorations)
            {
                if (decoration.Overlay == overlay)
                    return true;
            }

            return false;
        }
    }
}

