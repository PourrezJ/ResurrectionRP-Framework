using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities;
using ResurrectionRP_Server.Entities.Vehicles;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Utils;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using ResurrectionRP_Server.Streamer.Data;
using ResurrectionRP_Server.Colshape;
using System;

namespace ResurrectionRP_Server.DrivingSchool
{
    public class DrivingSchool
    {
        #region Fields and properties
        public byte Id { get; }
        public string SchoolName;
        private Vector3 EntryPosition;
        private List<Location> VehicleSpawnLocation;
        private LicenseType LicenseType;
        private int Price;        
        private VehicleModel VehicleModel;

        private Marker EntryMarker;
        private IColshape EntryColshape;
        private TextLabel EntryLabel;
        private Entities.Blips.Blips Blip;
        private ConcurrentDictionary<ulong, Exam> ConcernedPlayers = new ConcurrentDictionary<ulong, Exam>();

        public List<Ride> RidePoints;

        #endregion

        #region Constructor
        public DrivingSchool(byte id, Vector3 pos, List<Location> spawnVeh, Models.LicenseType licenseType, int price, List<Ride> circuit, VehicleModel vehicleSchool)
        {
            Id = id;
            EntryPosition = pos;
            VehicleSpawnLocation = spawnVeh;
            LicenseType = licenseType;
            Price = price;
            RidePoints = circuit;
            VehicleModel = vehicleSchool;

            switch (LicenseType)
            {
                case LicenseType.Car:
                    SchoolName = "Auto-École";
                    break;

                case LicenseType.Air:
                    SchoolName = "École de pilotage";
                    break;

                case LicenseType.Bike:
                    SchoolName = "Moto-École";
                    break;

                case LicenseType.Boat:
                    SchoolName = "Bateau-École";
                    break;
            }

            EntryMarker = Marker.CreateMarker(MarkerType.VerticalCylinder, EntryPosition , new Vector3(1f, 1f, 1f), Color.FromArgb(150, 5, 168, 0));
            EntryColshape = ColshapeManager.CreateCylinderColshape(EntryPosition, 2, 3);
            EntryColshape.OnPlayerEnterColshape += EntryColshape_OnPlayerEnterColshape;
            EntryColshape.OnPlayerLeaveColshape += EntryColshape_OnPlayerLeaveColshape;
            EntryLabel = TextLabel.CreateTextLabel($"[~o~Auto Ecole~w~]\nPassez votre permis ici", EntryPosition + new Vector3(0,0,1), Color.White, 2);

            var i = 0;
            RidePoints.ForEach((Ride p) => {
                p.Colshape = ColshapeManager.CreateCylinderColshape(p.Position, 5, 10);
                p.Colshape.SetData("DrivingSchool_ID", i++);
                p.Colshape.OnPlayerEnterColshape += Colshape_OnPlayerEnterColshape;
            });

            Alt.OnClient("DrivingSchool_Avert", (IPlayer client) =>
            {
                if (!client.Exists)
                    return;
                if (!ConcernedPlayers.ContainsKey(client.SocialClubId))
                    return;
                ConcernedPlayers[client.SocialClubId].Avert++;
                client.DisplayHelp($"Vous allez bien trop vite, ralentissez ({RidePoints[ ConcernedPlayers[client.SocialClubId].CurrentCheckpoint ].Speed})", 10000);
            });

            Alt.OnPlayerConnect += OnPlayerConnect;
            Alt.OnPlayerDisconnect += OnPlayerDisconnect;
            Blip = Entities.Blips.BlipsManager.CreateBlip(SchoolName, EntryPosition, 69, 535, 0.5f);
        }

        private void OnPlayerDisconnect(IPlayer player, string reason)
        {
        }

        private void OnPlayerConnect(IPlayer player, string reason)
        {
            if (!ConcernedPlayers.ContainsKey(player.SocialClubId))
                return;

            var exam = ConcernedPlayers[player.SocialClubId];
            exam.Player = player;

            if (exam.Vehicle == null)
                exam.End();

            player.SetWaypoint(exam.Vehicle.Position);
        }

        private void Colshape_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (!ConcernedPlayers.ContainsKey(client.SocialClubId))
                return;
            colshape.GetData("DrivingSchool_ID", out int result);
            if ( result != ConcernedPlayers[client.SocialClubId].CurrentCheckpoint)
                return;
            ConcernedPlayers[client.SocialClubId].NextTraj();

            if (ConcernedPlayers[client.SocialClubId].CurrentCheckpoint == RidePoints.Count)
                End(client, ConcernedPlayers[client.SocialClubId].Avert);
        }

        private void EntryColshape_OnPlayerLeaveColshape(IColshape colshape, IPlayer client) =>
            MenuManager.CloseMenu(client);
        

        private void EntryColshape_OnPlayerEnterColshape(IColshape colshape, IPlayer client)
        {
            if (!client.Exists || client.IsInVehicle)
                return;
            OpenMenuDrivingSchool(client);
        }
        #endregion

        #region Public methods

        public bool ClientIsInExam(IPlayer client) =>
            ConcernedPlayers.ContainsKey(client.SocialClubId);

        public void End(IPlayer client, int advert)
        {
            if (ConcernedPlayers.TryRemove(client.SocialClubId, out Exam exam))
            {
                exam.End();

                if (advert >= 5)
                    //await client.SendNotificationPicture($"~r~ Vous avez échoué votre examen avec {advert} fautes.", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                    client.SendNotificationError($"~r~ Vous avez échoué votre examen avec {advert} fautes.");
                else
                {
                    //await client.SendNotificationPicture($"~g~ Vous réussi votre examen avec {advert} faute(s).", Utils.Enums.CharPicture.CHAR_ANDREAS, false, 0, "Auto-école", "Examinateur");
                    client.GetPlayerHandler()?.Licenses.Add(new Models.License(LicenseType));
                    client.SendNotificationSuccess($"~g~ Vous réussi votre examen avec {advert} faute(s).");
                }
            }
        }

        public bool AddConcernedPlayer(IPlayer client, Exam exm)
        {
            if (ConcernedPlayers.ContainsKey(client.SocialClubId))
                return true;
            if (ConcernedPlayers.TryAdd(client.SocialClubId, exm))
                return true;
            Alt.Server.LogError("DrivingSchool | Error when trying to add a player from concerned | " + client.GetPlayerHandler().PID);
            return false;
        }
        public bool RemoveConcernedPlayer(IPlayer client, bool AnticipateEnd = false)
        {
            if (!ConcernedPlayers.ContainsKey(client.SocialClubId))
                return true;
            if (ConcernedPlayers.TryRemove(client.SocialClubId, out Exam voided))
            {
                if(AnticipateEnd)
                    voided.End();
                return true;
            }
            Alt.Server.LogError("DrivingSchool | Error when trying to remove a player from concerned | " + client.GetPlayerHandler().PID);
            return false;
        }

        public void CancelExam(IPlayer client) =>
            RemoveConcernedPlayer(client, true);

        public void StartExam(IPlayer client)
        {
            // Spawn Check

            foreach(var spawn in VehicleSpawnLocation)
            {
                if (!VehiclesManager.IsVehicleInSpawn(spawn))
                {
                    var veh = VehiclesManager.SpawnVehicle(client.GetSocialClub(), (uint)VehicleModel, spawn.Pos, spawn.Rot, plate: "SCHOOL", spawnVeh: true, locked: false);
                    Exam Examitem = new Exam(client, veh, this);
                    //Examitem.endExam = End();
                    ConcernedPlayers.GetOrAdd(client.SocialClubId, Examitem);
                    client.GetPlayerHandler()?.AddKey(veh, "Auto école");
                    //await client.EmitAsync("BeginDrivingExamen", veh.Vehicle.Id, Circuit, ID);
                    return;
                }
            }
        }
        #endregion

        #region Menu

        public void OpenMenuDrivingSchool(IPlayer client)
        {

            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph != null) 
            {
                Menu drivingschoolmenu = new Menu("ID_DrivingShoolMenu", SchoolName, "", Globals.MENU_POSX, Globals.MENU_POSY, Globals.MENU_ANCHOR, backCloseMenu: true);
                drivingschoolmenu.ItemSelectCallback = DrivingMenuCallBack;

                if (ClientIsInExam(client))
                {
                    client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_ANTONIA, SchoolName, "Secrétaire", "Vous êtes déjà en examen! Vous avez le droit d'abandonner.");
                    drivingschoolmenu.Add(new MenuItem("Abandonner l'examen", "Les gens vous jugent, vous étiez si près du but !", "ID_Cancel", true));
                    client.Emit("DriveSchool_CreateCP"); // pour annuler le CP & les checkers 
                }
                else
                {
                    if (!ph.HasLicense(LicenseType) || GameMode.IsDebug)
                        switch (LicenseType)
                        {
                            case LicenseType.Car:
                                drivingschoolmenu.Add(new MenuItem("Permis Voiture", $"Passer le permis voiture pour la somme de ~r~${Price} ~w~prélevée au début de l'examen.", "ID_Car", true, rightLabel: $"${Price}"));
                                break;
                            case LicenseType.Boat:
                                drivingschoolmenu.Add(new MenuItem("Permis Bateau", $"Passer le permis bateau pour la somme de ~r~${Price} ~w~prélevée au début de l'examen.", "ID_Boat", true, rightLabel: $"${Price}"));
                                break;
                            case LicenseType.Bike:
                                drivingschoolmenu.Add(new MenuItem("Permis Moto", $"Passer le permis moto pour la somme de ~r~${Price} ~w~prélevée au début de l'examen.", "ID_Bike", true, rightLabel: $"${Price}"));
                                break;
                            case LicenseType.Air:
                                drivingschoolmenu.Add(new MenuItem("Permis Avion", $"Passer le permis avion pour la somme de ~r~${Price} ~w~prélevée au début de l'examen.", "ID_Air", true, rightLabel: $"${Price}"));
                                break;
                        }
                    else
                        client.DisplayHelp("Vous avez déjà votre license ici, inutile de revenir.", 10000);
                }

                drivingschoolmenu.OpenMenu(client);
            }
        }

        private void DrivingMenuCallBack(IPlayer client, Menu menu, IMenuItem menuItem, int itemIndex)
        {
            Entities.Players.PlayerHandler ph = client.GetPlayerHandler();

            if (ph == null)
                return;

            switch(menuItem.Id)
            {
                case "ID_Cancel":
                    if (!ClientIsInExam(client))
                        break;
                    CancelExam(client);
                    client.DisplayHelp("Vous avez annulé votre examen...", 10000);
                    break;
                case "ID_Car":
                    /*
                    if(VehiclesManager.IsVehicleInSpawn(VehicleSpawnLocation, 4))
                    {
                        client.DisplayHelp("Un véhicule bloque la sortie du véhicule!", 10000);
                        break;
                    }*/
                    if (ph.HasBankMoney(Price, "Paiement License " + SchoolName))
                        StartExam(client);
                    else
                        client.DisplayHelp("Vous n'avez pas assez d'argent en banque pour ça.", 10000);
                    break;
                case "ID_Bike":
                case "ID_Boat":
                case "ID_Air":
                    client.SendNotificationPicture(Utils.Enums.CharPicture.CHAR_AMANDA, SchoolName, "Pas possible", "Le gouvernement n'a pas encore mis en place cela! ");
                    MenuManager.CloseMenu(client);
                    return;
            }
            MenuManager.CloseMenu(client);
        }
        #endregion
    }
}
