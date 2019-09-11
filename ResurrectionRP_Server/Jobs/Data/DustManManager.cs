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
            this.DustManClient = client;
            this.Zone = zone;
            this.Truck = client.Vehicle;
            this.DepotZone = pos;
            Task.Run(async () =>
            {
                await Init();      
            });
        }

        public async Task Init()
        {
            Alt.OnClient("Jobs_Dustman_Depot", this.OnDepot);
            this.DustManClient.SendNotificationSuccess($"Vous devez vous rendre dans la zone de ~g~{this.Zone.NameZone}~w~.");
            if (GameMode.Instance.IsDebug)
            this.DustManClient.SetWaypoint(this.Zone.ZonePosition, true);
            await this.DustManClient.EmitAsync("Jobs_Dustman", JsonConvert.SerializeObject(this.Zone.ZonePosition), JsonConvert.SerializeObject(this.Zone.TrashList));
        }

        public void OnDepot(IPlayer client, object[] args)
        {
            if (client.Id != this.DustManClient.Id)
                return;
            this.depotColShape = Alt.CreateColShapeCircle(this.DepotZone, 8);
            Alt.OnColShape += this.OnEnterColShape;
            client.SetWaypoint(this.DepotZone);
        }

        public void OnEnterColShape(IColShape colShape, IEntity entity, bool state)
        {
            if (!entity.Exists || entity.Type != BaseObjectType.Player)
                return;
            IPlayer client = entity as IPlayer;
            if(state && this.depotInProgress == false)
            {
                this.depotInProgress = true;
                client.SendNotificationSuccess("Génial ! Restez ici pour vider votre camion !");
                client.DisplayHelp("Déchargement de la remorque en cours, veuillez patenter! ", 30000);
                this.timer = Utils.Utils.Delay(30000, false, () =>
                {
                    if (!depotInProgress)
                        this.timer.Stop();
                    switch (this.ProgressState)
                    {
                        case 0:
                            this.ProgressState++;
                            break;
                        case 1:
                            client.DisplayHelp("On décharge ! Attendez encore un peu ! ", 30000);
                            this.ProgressState++;
                            break;
                        case 2:
                            client.DisplayHelp("On y est presque, un p'tit peu plus, et vous êtes libre ! ", 30000);
                            this.ProgressState++;
                            break;
                        default:
                            client.DisplayHelp("~g~C'est bon! ~w~Tu es libre ! ", 10000);
                            this.ProgressState = 0;
                            this.timer.Stop();
                            this.timer = null;
                            this.DustManClient.EmitLocked("DustMan_Callback");
                            break;
                    }
                });

            } else
            {
                if(depotInProgress)
                {
                    this.depotInProgress = false;
                    client.SendNotificationError("Vous aviez pour mission de rester ici, revenez !");
                }
            }
                
        }
    }
}
