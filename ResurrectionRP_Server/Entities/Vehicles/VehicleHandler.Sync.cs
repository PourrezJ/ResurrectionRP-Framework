using AltV.Net.Async;
using AltV.Net.Enums;
using ResurrectionRP_Server.Entities.Vehicles.Data;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Vehicles
{
    public partial class VehicleHandler
    {
        public VehicleDoorState GetDoorState(VehicleDoor door) => VehicleSync.Door[(byte)door];

        public async Task SetDoorState(VehicleDoor door, VehicleDoorState state)
        {
            await Vehicle.SetDoorStateAsync(door, state);
            
            VehicleSync.Door[(byte)door] = state;
            /*
            if (Vehicle != null && Vehicle.Exists)
            {
                var players = AltV.Net.Server.


                var players = await MP.Players.GetInRangeAsync(await Vehicle.GetPositionAsync(), Config.GetInt("stream-distance"));

                foreach (var player in players)
                {
                    if (!player.Exists)
                        continue;
                    await player.CallAsync("VehStream_SetDoorsStatus", Vehicle.Id, door, state);
                }
            }*/
        }
    }
}
