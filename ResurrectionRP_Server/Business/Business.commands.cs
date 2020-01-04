using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using ResurrectionRP_Server.Business.Barber;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils.Enums;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Business
{
    public partial class Business
    {
        public static void AddCommands()
        {
            Chat.RegisterCmd("AddTatooStore", AddTatooStore);
            Chat.RegisterCmd("AddBarberStore", AddBarberStore);
            Chat.RegisterCmd("AddArmumerieStore", AddArmumerieStore);
            Chat.RegisterCmd("AddClothingStore", AddClothingStore);
            Chat.RegisterCmd("AddClothingStore2", AddClothingStore2);
            Chat.RegisterCmd("AddClothingStore3", AddClothingStore3);
            Chat.RegisterCmd("AddClothingStore4", AddClothingStore4);
            Chat.RegisterCmd("Addclothingluxe", Addclothingluxe);
            Chat.RegisterCmd("AddMaskShop", AddMaskShop);
            Chat.RegisterCmd("AddPawnShop", AddPawnShop);
            Chat.RegisterCmd("AddDigitalDeen", AddDigitalDeen);
            Chat.RegisterCmd("AddAccessory", AddAccessory);
        }

        public static async Task AddTatooStore(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var tatoo = new TattoosStore("Tatoueur", new Location(await client.GetPositionAsync(), await client.GetRotationAsync()), 75, 500, PedModel.Tattoo01AMO);
            await tatoo.Insert();
        }

        public static async Task AddBarberStore(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var barber = new BarberStore("Barbier", new Location(await client.GetPositionAsync(), await client.GetRotationAsync()), 71, 500, PedModel.FemBarberSFM);
            await barber.Insert();
        }

        public static async Task AddArmumerieStore(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var weaponStore = new WeaponsShop("Armurerie", new Location(await client.GetPositionAsync(), await client.GetRotationAsync()), 110, 10000, PedModel.Ammucity01SMY);
            await weaponStore.Insert();
        }

        public static async Task AddClothingStore(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            Location pnjloc = new Location(new Vector3(73.94672f, -1392.636f, 29.37613f), new Vector3(0, 0, 270.3475f));

            var clothingPos = new Vector3(72.56725f, -1398.15f, 29.37613f); // position du colshape pour changer de tenu

            var clothingStore = new ClothingStore("Boutique de vêtements", new Location(pnjloc.Pos, pnjloc.Rot), 73, 10000, clothingPos, PedModel.Poppymich);
            clothingStore.BannerStyle = Banner.LowFashion;
            clothingStore.Buyable = false;
            /*
            clothingStore.MenFeet = new int[] { 3, 5, 6, 16, 17, 27, 31, 35, 42, 43, 56, 59, 60, 62, 63, 75, 76, 84, 85, 86 };
            clothingStore.GirlFeet = new int[] { 3, 5, 11, 13, 16, 17, 24, 25, 26, 31, 32, 33, 39, 52, 54, 55, 57, 59, 62, 63, 64, 65, 66, 73, 74, 75, 76, 79 };

            clothingStore.MenLegs = new int[] { 2, 3, 5, 6, 14, 15, 16, 18, 21, 26, 27, 29, 31, 32, 33, 34, 40, 46, 54, 55, 56, 57, 59, 60, 61, 62, 64, 66, 67, 68, 69, 70, 73, 74, 77, 78, 79, 80, 81, 82, 83, 85, 91, 94, 98, 99, 100, 101, 104, 106, 107, 110 };
            clothingStore.GirlLegs = new int[] { 1, 2, 4, 10, 11, 14, 15, 16, 17, 26, 32, 33, 38, 39, 40, 41, 45, 48, 49, 56, 57, 58, 59, 61, 66, 68, 70, 71, 72, 78, 79, 80, 81, 82, 83, 84, 85, 88, 94, 98, 100, 101, 103, 104, 105, 111, 113, 114, 119 };

            clothingStore.MenTops = new int[] { 1,3,5,7,8,14,16,17,18,36,38,44,47,51,52,56,57,67,68,69,80,81,82,85,86,89,90,97,113,114,116,118,119,120,121,122,123,124,125,126,127,146,147,148,150,151,152,155,164,166,169,170,171,174,175,178,180,182,
            184,185,188,189,191,194,195,196,197,198,199,200,201,202,203,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,227,237,238,239,245,246,251,253};

            clothingStore.GirlTops = new int[] {3,5,15,16,17,18,19,23,30,32,33,36,37,39,44,45,49,56,61,62,63,73,74,75,77,79,101,102,106,108,117,118,120,121,122,123,135,144,145,146,147,148,149,152,158,159,162,165,166,167,168,169,170,176,177
                ,180,181,182,184,186,187,190,191,193,195,196,197,198,199,200,201,202,203,204,205,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,237,239,242,243,253,254};

            clothingStore.MenUnderShirt = new int[] { 0, 1, 2, 3, 4, 5, 8, 9, 10, 12, 13, 14, 16, 17, 18, 27, 30, 34 };
            clothingStore.GirlUnderShirt = new int[] { 0, 1, 11, 12, 13, 16, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 37, 38, 39 };
            */
            await clothingStore.Insert();
        }

        public static async Task AddClothingStore2(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            Location loc = new Location(new Vector3(5.872166f, 6511.335f, 31.87784f), new Vector3(0, 0, 40.79449f));

            var clothingPos = new Vector3(11.00836f, 6513.661f, 31.87785f); // position du colshape pour changer de tenu

            var clothingStore = new ClothingStore("Boutique de vêtements", new Location(loc.Pos, loc.Rot), 73, 10000, clothingPos, PedModel.Poppymich);
            clothingStore.BannerStyle = Banner.LowFashion;
            clothingStore.Buyable = false;
            /*
            clothingStore.MenFeet = new int[] { 3, 5, 6, 16, 17, 27, 31, 35, 42, 43, 56, 59, 60, 62, 63, 75, 76, 84, 85, 86 };
            clothingStore.GirlFeet = new int[] { 3, 5, 11, 13, 16, 17, 24, 25, 26, 31, 32, 33, 39, 52, 54, 55, 57, 59, 62, 63, 64, 65, 66, 73, 74, 75, 76, 79 };

            clothingStore.MenLegs = new int[] { 2, 3, 5, 6, 14, 15, 16, 18, 21, 26, 27, 29, 31, 32, 33, 34, 40, 46, 54, 55, 56, 57, 59, 60, 61, 62, 64, 66, 67, 68, 69, 70, 73, 74, 77, 78, 79, 80, 81, 82, 83, 85, 91, 94, 98, 99, 100, 101, 104, 106, 107, 110 };
            clothingStore.GirlLegs = new int[] { 1, 2, 4, 10, 11, 14, 15, 16, 17, 26, 32, 33, 38, 39, 40, 41, 45, 48, 49, 56, 57, 58, 59, 61, 66, 68, 70, 71, 72, 78, 79, 80, 81, 82, 83, 84, 85, 88, 94, 98, 100, 101, 103, 104, 105, 111, 113, 114, 119 };

            clothingStore.MenTops = new int[] { 1,3,5,7,8,14,16,17,18,36,38,44,47,51,52,56,57,67,68,69,80,81,82,85,86,89,90,97,113,114,116,118,119,120,121,122,123,124,125,126,127,146,147,148,150,151,152,155,164,166,169,170,171,174,175,178,180,182,
            184,185,188,189,191,194,195,196,197,198,199,200,201,202,203,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,227,237,238,239,245,246,251,253};

            clothingStore.GirlTops = new int[] {3,5,15,16,17,18,19,23,30,32,33,36,37,39,44,45,49,56,61,62,63,73,74,75,77,79,101,102,106,108,117,118,120,121,122,123,135,144,145,146,147,148,149,152,158,159,162,165,166,167,168,169,170,176,177
                ,180,181,182,184,186,187,190,191,193,195,196,197,198,199,200,201,202,203,204,205,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,237,239,242,243,253,254};

            clothingStore.MenUnderShirt = new int[] { 0, 1, 2, 3, 4, 5, 8, 9, 10, 12, 13, 14, 16, 17, 18, 27, 30, 34 };
            clothingStore.GirlUnderShirt = new int[] { 0, 1, 11, 12, 13, 16, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 37, 38, 39 };
            */
            await clothingStore.Insert();
        }

        public static async Task AddClothingStore3(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var clothingPos = new Vector3(123.0721f, -219.4073f, 54.55784f); // position du colshape pour changer de tenu

            var clothingStore = new ClothingStore("Boutique de vêtements", new Location(new Vector3(127.1998f, -223.4781f, 54.55784f), new Vector3(0, 0, 56.35518f)), 73, 10000, clothingPos, PedModel.Poppymich);
            clothingStore.BannerStyle = Banner.MidFashion;
            clothingStore.Buyable = false;
            /*
            clothingStore.MenFeet = new int[] { 1, 4, 7, 8, 9, 12, 14, 22, 23, 24, 25, 26, 29, 32, 34, 46, 50, 51, 52, 53, 54, 57, 61, 70, 71, 72, 73, 74, 77, 79, 80, 81, 82, 89 };
            clothingStore.GirlFeet = new int[] { 1, 4, 9, 10, 15, 21, 22, 23, 27, 28, 30, 35, 43, 44, 51, 53, 56, 60, 68, 69, 72, 77, 81, 83, 84, 85, 86 };

            clothingStore.MenLegs = new int[] { 0, 1, 4, 7, 8, 9, 12, 13, 17, 35, 37, 43, 45, 47, 63, 65, 71, 72, 75, 76, 86, 87, 88, 89, 90, 96, 102, 103, 105 };
            clothingStore.GirlLegs = new int[] { 0, 3, 7, 12, 18, 24, 25, 27, 28, 30, 31, 36, 64, 67, 73, 74, 75, 76, 77, 87, 89, 90, 91, 92, 93, 97, 107, 109, 110, 112 };

            clothingStore.MenTops = new int[] { 6, 9, 12, 13, 22, 23, 24, 26, 33, 34, 37, 39, 41, 42, 43, 45, 46, 61, 62, 63, 64, 79, 83, 84, 87, 88, 92, 95, 96, 107, 110, 112, 128, 133, 134, 138, 139, 140, 141, 143, 144, 145, 153, 154, 156, 157, 158, 159, 160, 161, 162, 163, 167, 168, 172, 173, 176, 179, 181, 187, 192, 204, 205, 223, 224, 225, 243, 249, 250, 262, 263, 264 };

            clothingStore.GirlTops = new int[] { 1, 2, 8, 9, 10, 11, 14, 27, 31, 35, 38, 40, 50, 51, 52, 53, 54, 55, 72, 76, 78, 80, 81, 86, 87, 88, 98, 104, 105, 110, 112, 113, 114, 115, 116, 119, 125, 126, 131, 136, 137, 140, 141, 150, 151, 153, 154, 155, 156, 157, 160, 161, 163, 164, 171, 172, 173, 174, 175, 178, 183, 189, 194, 206, 233, 234, 235, 244, 245, 247, 257, 258, 271, 272, 280, 281, 284, 286 };

            clothingStore.MenUnderShirt = new int[] { 0, 1, 2, 3, 4, 5, 8, 9, 10, 12, 13, 14, 16, 17, 18, 27, 30, 34 };
            clothingStore.GirlUnderShirt = new int[] { 0, 1, 11, 12, 13, 16, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 37, 38, 39 };
            */
            await clothingStore.Insert();
        }

        public static async Task AddClothingStore4(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var clothingPos = new Vector3(-3174.521f, 1044.078f, 20.86321f); // position du colshape pour changer de tenu

            var clothingStore = new ClothingStore("Boutique de vêtements", new Location(new Vector3(-3168.986f, 1044.027f, 20.86321f), new Vector3(0, 0, 65.07517f)), 73, 10000, clothingPos, PedModel.Poppymich);
            clothingStore.BannerStyle = Banner.MidFashion;
            clothingStore.Buyable = false;
            /*
            clothingStore.MenFeet = new int[] { 1, 4, 7, 8, 9, 12, 14, 22, 23, 24, 25, 26, 29, 32, 34, 46, 50, 51, 52, 53, 54, 57, 61, 70, 71, 72, 73, 74, 77, 79, 80, 81, 82, 89 };
            clothingStore.GirlFeet = new int[] { 1, 4, 9, 10, 15, 21, 22, 23, 27, 28, 30, 35, 43, 44, 51, 53, 56, 60, 68, 69, 72, 77, 81, 83, 84, 85, 86 };

            clothingStore.MenLegs = new int[] { 0, 1, 4, 7, 8, 9, 12, 13, 17, 35, 37, 43, 45, 47, 63, 65, 71, 72, 75, 76, 86, 87, 88, 89, 90, 96, 102, 103, 105 };
            clothingStore.GirlLegs = new int[] { 0, 3, 7, 12, 18, 24, 25, 27, 28, 30, 31, 36, 64, 67, 73, 74, 75, 76, 77, 87, 89, 90, 91, 92, 93, 97, 107, 109, 110, 112 };

            clothingStore.MenTops = new int[] { 6, 9, 12, 13, 22, 23, 24, 26, 33, 34, 37, 39, 41, 42, 43, 45, 46, 61, 62, 63, 64, 79, 83, 84, 87, 88, 92, 95, 96, 107, 110, 112, 128, 133, 134, 138, 139, 140, 141, 143, 144, 145, 153, 154, 156, 157, 158, 159, 160, 161, 162, 163, 167, 168, 172, 173, 176, 179, 181, 187, 192, 204, 205, 223, 224, 225, 243, 249, 250, 262, 263, 264 };

            clothingStore.GirlTops = new int[] { 1, 2, 8, 9, 10, 11, 14, 27, 31, 35, 38, 40, 50, 51, 52, 53, 54, 55, 72, 76, 78, 80, 81, 86, 87, 88, 98, 104, 105, 110, 112, 113, 114, 115, 116, 119, 125, 126, 131, 136, 137, 140, 141, 150, 151, 153, 154, 155, 156, 157, 160, 161, 163, 164, 171, 172, 173, 174, 175, 178, 183, 189, 194, 206, 233, 234, 235, 244, 245, 247, 257, 258, 271, 272, 280, 281, 284, 286 };

            clothingStore.MenUnderShirt = new int[] { 0, 1, 2, 3, 4, 5, 8, 9, 10, 12, 13, 14, 16, 17, 18, 27, 30, 34 };
            clothingStore.GirlUnderShirt = new int[] { 0, 1, 11, 12, 13, 16, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 37, 38, 39 };
            */
            await clothingStore.Insert();
        }

        public static async Task Addclothingluxe(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var clothingPos = new Vector3(-704.6177f, -151.9589f, 37.41514f); // position du colshape pour changer de tenu

            var clothingStore = new ClothingStore("Boutique de vêtements luxe", new Location(new Vector3(-708.8221f, -151.7659f, 37.41513f), new Vector3(0, 0, 135.085f)), 73, 10000, clothingPos, PedModel.AnitaCutscene);
            clothingStore.BannerStyle = Banner.HighFashion;
            clothingStore.Buyable = false;
            /*
            clothingStore.MenFeet = new int[] { 10, 15, 18, 19, 20, 21, 28, 30, 34, 36, 37, 38, 40, 41, 44, 45, 65, 66, 69, 88 };
            clothingStore.GirlFeet = new int[] { 0, 2, 6, 7, 8, 14, 18, 19, 20, 29, 35, 37, 38, 39, 41, 42, 45, 46, 92 };

            clothingStore.MenLegs = new int[] { 10, 19, 20, 22, 23, 24, 25, 28, 48, 49, 50, 51, 52, 53, 58 };
            clothingStore.GirlLegs = new int[] { 6, 8, 9, 19, 20, 22, 23, 34, 37, 43, 44, 47, 50, 51, 52, 53, 54, 55, 60, 62, 63, 65, 99, 102, 106, 108 };

            clothingStore.MenTops
                = new int[] { 4, 10, 11, 19, 20, 21, 25, 27, 28, 29, 30, 31, 32, 35, 40, 58, 59, 60, 70, 71, 72, 73, 74, 75, 76, 77, 78, 93, 94, 99, 100, 101, 102, 103, 104, 105, 106, 108, 109, 111, 115, 117, 131, 132, 135, 136, 137, 142, 183, 190, 192, 226, 235, 236, 240, 241, 242, 254 };

            clothingStore.GirlTops
                = new int[] { 6, 7, 13, 20, 21, 22, 24, 25, 26, 28, 29, 34, 57, 58, 64, 65, 66, 67, 68, 69, 70, 71, 83, 84, 85, 90, 91, 92, 93, 94, 95, 96, 97, 99, 100, 103, 107, 109, 111, 124, 128, 129, 130, 132, 133, 134, 138, 139, 142, 143, 185, 192, 236, 246, 248, 249, 250 };

            clothingStore.MenUnderShirt = new int[] { };
            clothingStore.GirlUnderShirt = new int[] { };
            */
            await clothingStore.Insert();
        }

        public static async Task AddMaskShop(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var clothingPos = new Vector3(-1336.856f, -1279.136f, 4.856103f); // position du colshape pour changer de tenu

            var clothingStore = new ClothingStore("Boutique de masques", new Location(await client.GetPositionAsync(), await client.GetRotationAsync()), 102, 10000, clothingPos, PedModel.JimmyDisanto);
            clothingStore.BannerStyle = Banner.MovieMasks;
            clothingStore.Buyable = false;

            //clothingStore.Mask = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 29, 30, 31, 32, 33, 34, 35 };

            await clothingStore.Insert();
        }

        public static async Task AddPawnShop(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var inventoryBox = InventoryBox.CreateInventoryBox("Pawnshop", new Location(new Vector3(2752.311f, 3489.616f, 56.24662f), new Vector3(0, 0, 330f)), new Inventory.Inventory(1000, 40), Alt.Hash("p_v_43_safe_s"));
            var pawnShop = new PawnShop("Pawn-Shop", new Location(await client.GetPositionAsync(), await client.GetRotationAsync()), 108, 10000, inventoryBox, PedModel.Strvend01SMY, "Armex72", true, false);
            await pawnShop.Insert();
        }

        public static async Task AddDigitalDeen(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var digitalDeen = new DigitalDeen("Digital Deen", new Location(await client.GetPositionAsync(), await client.GetRotationAsync()), 606, 750, PedModel.Stwhi02AMY);
            await digitalDeen.Insert();
        }

        public static async Task AddAccessory(IPlayer client, string[] arguments)
        {
            if (client.GetStaffRank() <= StaffRank.Player)
                return;

            var clothingPos = new Vector3(-622.5515f, -231.0927f, 38.05706f); // position du colshape pour changer de tenu

            var clothingStore = new PropsStore("Boutique d'accessoires", new Location(new Vector3(-616.9886f, -228.4723f, 38.057f), new Vector3(0, 0, 129.577f)), 171, 10000, clothingPos, PedModel.ShopMidSFY);
            //clothingStore.BannerStyle = Banner.a;
            clothingStore.Buyable = false;
            /*
            clothingStore.MenHats = new int[] { 2, 5, 6, 7, 12, 13, 14, 15, 20, 21, 25, 26, 27, 29, 44, 45, 58, 61, 63, 64, 83, 94, 95, 96, 102, 120 };
            clothingStore.GirlHats = new int[] { 6, 7, 12, 13, 14, 15, 20, 21, 22, 26, 27, 28, 29, 54, 58, 60, 61, 63, 82, 93, 94, 95, 101, 108, 109, 119 };

            clothingStore.MenGlasses = new int[] { 2, 3, 4, 5, 7, 8, 9, 10, 12, 13, 15, 16, 17, 18, 19, 20, 23, 24 };
            clothingStore.GirlGlasses = new int[] { 0, 1, 2, 3, 4, 7, 8, 11, 14, 15, 16, 17, 18, 19, 20, 21, 24, 25, 26 };

            clothingStore.MenEars = new int[] { 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 32, 35 };
            clothingStore.GirlEars = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };

            clothingStore.MenWatches = new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 };
            clothingStore.GirlWatches = new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

            clothingStore.MenBracelets = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            clothingStore.GirlBracelets = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            */
            //clothingStore.Accessories = new int[] { };

            await clothingStore.Insert();
        }
    }
}
