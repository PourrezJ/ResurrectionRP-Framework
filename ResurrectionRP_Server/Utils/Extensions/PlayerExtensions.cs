using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using MongoDB.Driver;
using ResurrectionRP_Server.Entities.Players;
using ResurrectionRP_Server.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace ResurrectionRP_Server
{
    public static class PlayerExtensions
    {
        public static string GetSocialClub(this IPlayer player)
        {
            if (player.GetData("SocialClub", out string data))
                return data;

            return "";
        }

        public static void SetCloth(this IPlayer client, ClothSlot level, int drawable, int texture, int palette)
        {
            client.EmitLocked("ComponentVariation", (int)level, drawable, texture, palette);
        }

        public static void SetProp(this IPlayer client, PropSlot slot, PropData item)
        {
            client.EmitLocked("PropVariation", (int)slot, item.Drawable, item.Texture);
        }

        public static void SetWaypoint(this IPlayer client, Vector3 pos, bool overrideOld = true) => 
            client.EmitLocked("SetWaypoint", pos.X, pos.Y, overrideOld);

        public static void DisplayHelp(this IPlayer client, string text, int timeMs) =>
            client.EmitLocked("Display_Help", text, timeMs);

        public static void DisplaySubtitle(this IPlayer client, string text, int timeMs) =>
            client.EmitLocked("Display_subtitle", text, timeMs);

        public static PlayerHandler GetPlayerHandler(this IPlayer client)
        {
            if (client == null || !client.Exists)
                return null;

            if (PlayerHandler.PlayerHandlerList.TryGetValue(client, out PlayerHandler value))
                return value;

            return null;
        }

        public static bool IsDead(this IPlayer player)
        {
            var ph = player.GetPlayerHandler();

            if (ph == null)
                return false;

            return ph.PlayerSync.IsDead;
        }
        
        public static void SendNotification(this IPlayer client, string text)
        {
            if (text == "")
                return;

            client.EmitLocked("notify", "Notification", text, 7000);
        }

        public static void SendNotificationError(this IPlayer client, string text)
        {
            client.EmitLocked("alertNotify", "Erreur", text, 7000);
        }

        public static void SendNotificationSuccess(this IPlayer client, string text)
        {
            client.EmitLocked("successNotify", "Succès", text, 7000);
        }

        public static void SendNotificationPicture(this IPlayer client, Utils.Enums.CharPicture img, string sender, string subject, string message) =>
            client.EmitLocked("SetNotificationMessage", img.ToString(), sender, subject, message);

        public static List<IVehicle> GetVehiclesInRange(this IPlayer client, int Range)
        {
            // BUG v752 : La liste des véhicules renvoie des véhicules supprimés
            // var vehs = Alt.GetAllVehicles();
            var vehs = Entities.Vehicles.VehiclesManager.GetAllVehiclesInGame();

            List<IVehicle> endup = new List<IVehicle>();
            var position = client.GetPosition();
            Vector3 osition = new Vector3(position.X, position.Y, position.Z);
            foreach (IVehicle veh in vehs)
            {
                if (!veh.Exists)
                    continue;
                var vehpos = veh.GetPosition();
                if (osition.DistanceTo2D(new Vector3(vehpos.X, vehpos.Y, vehpos.Z)) <= Range)
                {
                    endup.Add(veh);
                }
            }
            return endup;
        }

        public static List<IPlayer> GetPlayersInRange(this IPlayer client, float Range)
        {
            List<IPlayer> endup = new List<IPlayer>();
            try
            {
                var players = PlayerManager.GetPlayersList();

                Position playerPos = Position.Zero;
                client.GetPositionLocked(ref playerPos);

                Vector3 osition = new Vector3(playerPos.X, playerPos.Y, playerPos.Z);
                foreach (PlayerHandler player in players)
                {
                    if (!player.Client.Exists)
                        continue;

                    Position pos = Position.Zero;

                    player.Client.GetPositionLocked(ref pos);
                    if (osition.DistanceTo2D(new Vector3(pos.X, pos.Y, pos.Z)) <= Range)
                    {
                        endup.Add(player.Client);
                    }
                }
            }
            catch(Exception ex)
            {
                Alt.Server.LogError(ex.ToString());
            }

            return endup;
        }

        public static List<PlayerHandler> GetPlayersHandlerInRange(this IPlayer client, float Range)
        {
            var vehs = Alt.GetAllPlayers();
            List<Entities.Players.PlayerHandler> endup = new List<PlayerHandler>();
            var position = client.GetPosition();
            Vector3 osition = new Vector3(position.X, position.Y, position.Z);
            foreach (IPlayer veh in vehs)
            {
                if (!veh.Exists)
                    continue;
                var vehpos = veh.GetPosition();
                if (osition.DistanceTo2D(new Vector3(vehpos.X, vehpos.Y, vehpos.Z)) <= Range)
                {
                    endup.Add(veh.GetPlayerHandler());
                }
            }
            return endup;
        }

        public static IPlayer GetNearestPlayer(this IPlayer client)
        {
            ICollection<IPlayer> players = Alt.GetAllPlayers();
            IPlayer nearestPlayer = null;

            foreach (IPlayer player in players)
            {
                if (player == client || !player.Exists || player.Dimension != player.Dimension)
                    continue;

                if (nearestPlayer == null || client.Position.Distance(player.Position) <= client.Position.Distance(nearestPlayer.Position))
                    nearestPlayer = player;
            }

            return nearestPlayer;
        }

        public static List<IPlayer> GetNearestPlayers(this IPlayer client, float range, bool withoutme = true, int dimension = GameMode.GlobalDimension)
        {
            ICollection<IPlayer> players = Alt.GetAllPlayers();
            List<IPlayer> nearestPlayers = new List<IPlayer>();

            foreach (IPlayer player in players)
            {
                if (!player.Exists || player.Dimension != dimension)
                    continue;

                if (client.Position.Distance(player.Position) <= range)
                    nearestPlayers.Add(player);
            }

            if (withoutme)
                nearestPlayers.Remove(client);

            return nearestPlayers;
        }

        public static IVehicle GetNearestVehicle(this IPlayer client)
        {
            // BUG v752 : La liste des véhicules renvoie des véhicules supprimés
            // var vehs = Alt.GetAllVehicles();
            var vehs = Entities.Vehicles.VehiclesManager.GetAllVehiclesInGame();

            IVehicle endup = null;
            var position = client.GetPosition();
            Vector3 osition = new Vector3(position.X, position.Y, position.Z);
            foreach (IVehicle veh in vehs)
            {
                if (!veh.Exists)
                    continue;
                if (endup == null && veh != client.Vehicle)
                    endup = veh;
                var vehpos = veh.GetPosition();
                var enduppos = endup.GetPosition();
                if (osition.DistanceTo2D(new Vector3(vehpos.X, vehpos.Y, vehpos.Z)) <= osition.DistanceTo2D(new Vector3(enduppos.X, enduppos.Y, enduppos.Z)) && veh != client.Vehicle)
                {
                    endup = veh;
                }
            }
            return endup;
        }
        public static Entities.Vehicles.VehicleHandler GetNearestVehicleHandler(this IPlayer client) =>
            client.GetNearestVehicle()?.GetVehicleHandler();

        public static void SetRotation(this IPlayer client, Vector3 rotate)
        {
            Rotation rotating = new Rotation(rotate.X, rotate.Y, rotate.Z);
            client.Rotation = rotating;
        }

        public static bool HasData(this IPlayer client, string Data)
        {
            return client.GetData(Data, out string result);
        }

        public static void ResetData(this IPlayer client, string Data)
        {
            client.SetData(Data, null);
        }

        public async static Task PlaySoundFrontEndFix(this IPlayer client, int id, string dict, string anim)
        {
            await Task.CompletedTask;
        }

        public async static Task PlaySoundFromEntity(this IPlayer client, IPlayer initiator, int id, string dict, string anim)
        {
            await Task.CompletedTask;
        }

        public async static Task PlaySoundFromEntity(this IPlayer client, IVehicle initiator, int id, string dict, string anim)
        {
            await Task.CompletedTask;
        }

        public async static Task PlaySoundFromEntity(this IPlayer client, IEntity initiator, int id, string dict, string anim)
        {
            await Task.CompletedTask;
        }

        public static Task SetDecorationAsync(this IPlayer client, uint collection, uint overlay)
        {
            // TODO
            return Task.CompletedTask;
        }

        public static Task RemoveDecorationAsync(this IPlayer client, uint collection, uint overlay)
        {
            // TODO
            return Task.CompletedTask;
        }

        public static void SetDecoration(this IPlayer client, uint collection, uint overlay)
        {
            // TODO
        }

        public static void ClearDecorations(this IPlayer client)
        {
            // TODO
        }

        public static Task ClearDecorationsAsync(this IPlayer client)
        {
            // TODO
            return Task.CompletedTask;
        }

        public static async Task SetInvisibleAsync(this IPlayer client, bool invisible)
            => await client.SetSyncedMetaDataAsync("SetInvisible", invisible);

        public static async Task<bool> IsInvisibleAsync(this IPlayer client) 
            => await client.GetSyncedMetaDataAsync<bool>("SetInvisible");



        public static async Task SetInvincibleAsync(this IPlayer client, bool value)
            => await client.SetSyncedMetaDataAsync("SetInvincible", value);

        public static async Task<bool> IsInvinsibleAsync(this IPlayer client)
            => await client.GetSyncedMetaDataAsync<bool>("SetInvincible");


        public static Task SetHeadOverlayAsync(this IPlayer client, int overlayId, Business.Barber.HeadOverlayData overlayData)
        {
            client.EmitLocked("HeadOverlayVariation", overlayData.Index, overlayData.Opacity, overlayData.ColorId, overlayData.SecondaryColorId, overlayId);
            return Task.CompletedTask;
        }

        public static void SetHeadOverlay(this IPlayer client, int overlayId, Business.Barber.HeadOverlayData overlayData)
        {
            client.EmitLocked("HeadOverlayVariation", overlayData.Index, overlayData.Opacity, overlayData.ColorId, overlayData.SecondaryColorId, overlayId);
        }

        public static void SetHairColor(this IPlayer client, uint color, uint hightlightColor)
        {
            client.EmitLocked("HairVariation", color, hightlightColor);
        }

        public static Task SetHairColorAsync(this IPlayer client,  uint color, uint hightlightColor)
        {
            client.EmitLocked("HairVariation", color, hightlightColor);
            return Task.CompletedTask;
        }

        public static void Freeze(this IPlayer client, bool state)
        {
            // TODO
        }

        public static void CreateBlip(this IPlayer client, uint sprite, Vector3 position, string name = "", float scale = 1, int color = 0, int alpha = 255, bool shortRange = false) 
            => client.EmitLocked("CreateBlip", sprite, position.ConvertToVector3Serialized(), name, scale, color, alpha, shortRange);

        public static void PlayAnimation(this IPlayer client, string animDict, string animName, float blendInSpeed = 8f, float blendOutSpeed = -8f, int duration = -1, Utils.Enums.AnimationFlags flags = 0, float playbackRate = 0f)
        {
            var animsync = new AnimationsSync()
            {
                AnimName = animName,
                AnimDict = animDict,
                BlendInSpeed = blendInSpeed,
                BlendOutSpeed = blendOutSpeed,
                Duraction = duration,
                Flag = (int)flags,
                PlaybackRate = playbackRate
            };

            client.EmitLocked("PlayAnimation", JsonConvert.SerializeObject(animsync));
        }

        public static void StopAnimation(this IPlayer client)
        {
            client.EmitLocked("StopAnimation");
        }

        public static void RequestCollisionAtCoords(this IPlayer client, Vector3 pos)
        {
            client.EmitLocked("RequestCollisionAtCoords", pos.X, pos.Y, pos.Z);
        }

        public static void Resurrect(this IPlayer client)
            => client.EmitLocked("ResurrectPlayer");

        public static async Task<bool> PlayerHandlerExist(this IPlayer player)
        {
            if (!player.Exists)
                return false;
            try
            {
                player.GetData("SocialClub", out string social);
                return await Database.MongoDB.GetCollectionSafe<PlayerHandler>("players").Find(p => p.PID == social).AnyAsync();
            }
            catch (Exception ex)
            {
                // await player.SendNotificationError("Erreur avec votre compte, contactez un membre du staff.");
                Alt.Server.LogError("PlayerHandlerExist" + ex);
            }
            return false;
        }

        public static async Task Revive(this IPlayer client, ushort health = 200, Vector3? position = null)
        {
            Vector3 pos = position ?? await client.GetPositionAsync();
            await client.SpawnAsync(new Position(pos.X, pos.Y, pos.Z));
            await client.SetHealthAsync(health);
            client.Resurrect();

            if (GameMode.Instance.FactionManager.Onu != null && GameMode.Instance.FactionManager.Onu.ServicePlayerList?.Count > 0)
            {
                foreach (var medecin in GameMode.Instance.FactionManager.Onu?.GetEmployeeOnline())
                {
                    medecin.EmitLocked("ONU_BlesseEnd", client.Id);
                }
            }
        }

        public static bool HasVehicleKey(this IPlayer client, string plate)
            => client.GetPlayerHandler().ListVehicleKey.Exists(x => x.Plate == plate);

        public static void SetPlayerIntoVehicle(this IPlayer target, IVehicle client)
        {
            target.EmitLocked("SetPlayerIntoVehicle", client, -1);
        }
    }
}