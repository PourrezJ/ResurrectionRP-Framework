using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using System.Numerics;

namespace ResurrectionRP_Server.DrivingSchool
{
    public class Exam : IDisposable
    {
        #region Fields
        public IPlayer PlayerExam;
        public Entities.Vehicles.VehicleHandler VehicleExam;
        public int CurrentCheckpoint = -1;
        public List<Ride> Trajectoire;
        public ICheckpoint checkpoint;
        public IColShape colshape = null;
        public int avert = 0;
        public int id = 0;
        #endregion

        #region Constructor
        public Exam(IPlayer client, Entities.Vehicles.VehicleHandler vehicle, List<Ride> traj, int SchooldId)
        {
            PlayerExam = client;
            VehicleExam = vehicle;
            this.id = SchooldId;
            this.Trajectoire = traj;

            AltAsync.OnPlayerEnterVehicle += this.OnEnterVehicle;
            
        }
        #endregion

        #region Methods
        private async Task onColShape(IColShape colShape, IEntity entity, bool state)
        {
            if (!state || entity.Type != BaseObjectType.Player)
                return;
            if (colShape.Position != this.colshape.Position)
                return;
            this.colshape.GetData("DrivingSchool", out bool result);
            if (result != true)
                return;
            await this.NextTraj();
        }

        private async Task NextTraj()
        {
            if(this.checkpoint != null)
                Alt.RemoveCheckpoint(this.checkpoint);
            if (this.colshape != null)
                Alt.RemoveColShape(this.colshape);


            this.CurrentCheckpoint++;
            if(this.CurrentCheckpoint == this.Trajectoire.Count )
            {
                await endTraj();
                return;
            }

            this.checkpoint = await AltAsync.CreateCheckpoint(this.PlayerExam, (byte)CheckpointType.Cyclinder2, this.Trajectoire[this.CurrentCheckpoint].Position, 3, 6, new Rgba(255, 255, 255, 128));
            this.colshape = Alt.CreateColShapeCylinder(this.Trajectoire[this.CurrentCheckpoint].Position, 3, 6);
            this.PlayerExam.SetWaypoint(this.Trajectoire[this.CurrentCheckpoint].Position);
            this.colshape.SetData("DrivingSchool", true);


        }

        private async Task endTraj()
        {
            await this.PlayerExam.EmitAsync("DrivingSchool_End");
            await this.End();
            await this.PlayerExam.EmitAsync("DeleteWaypoint");
            await AltAsync.EmitAsync(PlayerExam, "DrivingSchool_End", this.id, this.avert);
        }

        private Task VehicleChecker(IPlayer client, object[] args)
        {
            if ((long)args[0] > Trajectoire[CurrentCheckpoint].Speed)
            {
                client.SendNotificationError("Votre vitesse est bien trop excessive ! Ralentissez bon sang !");
                avert++;
            }

            return Task.CompletedTask;
        }

        private async Task OnEnterVehicle(IVehicle vehicle, IPlayer client, byte seat)
        {
            if (seat != 1)
                return;
            if (vehicle != this.VehicleExam.Vehicle)
                return;
            //await client.SendNotificationPicture("Ok! Maintenant allumer le moteur ~r~(F3) ~w~et rendez-vous au prochain point.", "CHAR_ANDREAS", false, 0, "CHAR_ANDREAS",  "Auto-école");
            client.SendNotificationSuccess("Génial, l'instructeur vous fait signe de démarrer le moteur (F3), et de rejoindre le point blanc");
            await client.EmitAsync("DrivingSchool_Start");
            AltAsync.OnClient("DrivingSchool_Checker", VehicleChecker);
            await NextTraj();

            AltAsync.OnColShape += this.onColShape;
        }
        public async Task End()
        {
            await VehicleExam.Delete();
            PlayerExam.GetPlayerHandler()?.RemoveKey(VehicleExam);
            Dispose();
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
