using AltV.Net;
using AltV.Net.Async;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ResurrectionRP_Server.Models;
using System;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        #region Fields
        [BsonIgnore]
        private static readonly double _updateWaitTime = 1000.0;
        [BsonIgnore]
        private DateTime _lastUpdateRequest;
        [BsonIgnore]
        private bool _updateWaiting = false;
        [BsonIgnore]
        private int _nbUpdateRequests;
        #endregion

        #region Methods
        public async Task InsertVehicle()
        {
            try
            {
                if (SpawnVeh)
                    return;

                await Database.MongoDB.Insert("vehicles", this);
            }
            catch (BsonException be)
            {
                Alt.Server.LogError(be.Message);
            }
        }

        public void UpdateFull(Location location = null)
        {
            if (SpawnVeh)
                return;

            LastUse = DateTime.Now;

            if (Vehicle != null)
            {
                if (!Vehicle.Exists)
                    return;

                if (location != null)
                    Location = location;

                UpdateProperties();
                UpdateInBackground();
            }
        }

        public void UpdateInBackground()
        {
            _lastUpdateRequest = DateTime.Now;

            if (_updateWaiting)
            {
                _nbUpdateRequests++;
                return;
            }

            _updateWaiting = true;
            _nbUpdateRequests = 1;

            Task.Run(async () =>
            {
                DateTime updateTime = _lastUpdateRequest.AddMilliseconds(_updateWaitTime);

                while (DateTime.Now < updateTime)
                {
                    TimeSpan waitTime = updateTime - DateTime.Now;

                    if (waitTime.TotalMilliseconds < 1)
                        waitTime = new TimeSpan(0, 0, 0, 0, 1);

                    await Task.Delay((int)waitTime.TotalMilliseconds);
                    updateTime = _lastUpdateRequest.AddMilliseconds(_updateWaitTime);
                }

                try
                {
                    var result = await Database.MongoDB.Update(this, "vehicles", Plate);

                    if (result.ModifiedCount == 0)
                        Alt.Server.LogError($"Vehicule Update error for vehicle: {Plate} - Owner: {OwnerID}");

                    _updateWaiting = false;
                }
                catch (Exception ex)
                {
                    Alt.Server.LogError("VehicleHandler Update: " + ex);
                }
            });
        }

        public async Task<bool> RemoveInDatabase()
        {
            var result = await Database.MongoDB.Delete<VehicleHandler>("vehicles", Plate);
            return (result.DeletedCount != 0);
        }
        #endregion
    }
}