using System;
using System.Collections.Generic;
using System.Text;
using AltV.Net;
using AltV.Net.Elements.Entities;
using System.Numerics;
using System.Threading.Tasks;
using AltV.Net.Async;
using Newtonsoft.Json;

namespace ResurrectionRP_Server.Jobs
{
    public class DustManManager
    {
        public IPlayer DustManClient;
        public TrashZone Zone;
        public IVehicle Truck;
        public Vector3 DepotZone;
        public IColShape depotColShape;
        public bool depotInProgress = false;
        public System.Timers.Timer timer = null;
        public int ProgressState = 0;

        public  DustManManager(IPlayer client, TrashZone zone,Vector3 pos )
        {
            DustManClient = client;
            Zone = zone;
            Truck = client.Vehicle;
            DepotZone = pos;
            Task.Run(async () =>
            {
                await Init();      
            });
        }

        public async Task Init()
        {
            AltAsync.OnClient("Jobs_Dustman_Depot", OnDepot);
            DustManClient.SendNotificationSuccess($"Vous devez vous rendre dans la zone de ~g~{Zone.NameZone}~w~.");
            if (GameMode.IsDebug)
            DustManClient.SetWaypoint(Zone.ZonePosition, true);
            await DustManClient.EmitAsync("Jobs_Dustman", JsonConvert.SerializeObject(Zone.ZonePosition), JsonConvert.SerializeObject(Zone.TrashList));
        }

        public Task OnDepot(IPlayer client, object[] args)
        {
            if (client.Id != DustManClient.Id)
                return Task.CompletedTask;

            depotColShape = Alt.CreateColShapeCircle(DepotZone, 8);
            AltAsync.OnColShape += OnEnterColShape;
            client.SetWaypoint(DepotZone);
            return Task.CompletedTask;
        }

        public Task OnEnterColShape(IColShape colShape, IEntity entity, bool state)
        {
            if (!entity.Exists || entity.Type != BaseObjectType.Player)
                return Task.CompletedTask;

            IPlayer client = entity as IPlayer;
            if (state && depotInProgress == false)
            {
                depotInProgress = true;
                client.SendNotificationSuccess("Génial ! Restez ici pour vider votre camion !");
                client.DisplayHelp("Déchargement de la remorque en cours, veuillez patenter! ", 30000);
                timer = Utils.Util.SetInterval(() =>
                {
                    if (!depotInProgress)
                        timer.Stop();
                    switch (ProgressState)
                    {
                        case 0:
                            ProgressState++;
                            break;
                        case 1:
                            client.DisplayHelp("On décharge ! Attendez encore un peu ! ", 30000);
                            ProgressState++;
                            break;
                        case 2:
                            client.DisplayHelp("On y est presque, un p'tit peu plus, et vous êtes libre ! ", 30000);
                            ProgressState++;
                            break;
                        default:
                            client.DisplayHelp("~g~C'est bon! ~w~Tu es libre ! ", 10000);
                            ProgressState = 0;
                            timer.Stop();
                            timer = null;
                            DustManClient.EmitLocked("DustMan_Callback");
                            break;
                    }
                }, 30000);
            }
            else if (depotInProgress)
            {
                depotInProgress = false;
                client.SendNotificationError("Vous aviez pour mission de rester ici, revenez !");
            }

            return Task.CompletedTask;
        }
    }
}
