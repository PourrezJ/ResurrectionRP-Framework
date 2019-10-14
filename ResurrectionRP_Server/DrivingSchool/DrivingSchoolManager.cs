using AltV.Net.Elements.Entities;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Enums;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.DrivingSchool
{

    public class DrivingSchoolManager
    {
        private List<DrivingSchool> drivingSchools = new List<DrivingSchool>();


        public void Load()
        {
            var schooltrajetcar = new List<Ride>()
            {
                new Ride(){ Position = new Vector3(67.20625f, -1484.257f, 28), Speed = 70 }, // 0
                new Ride(){ Position = new Vector3(149.4662f, -1582.298f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(262.6104f, -1666.38f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(345.8558f, -1730.38f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(136.2186f, -2020.983f, 17), Speed = 120 },
                new Ride(){ Position = new Vector3(-349.693f, -2087.118f, 23), Speed = 120 }, // 5
                new Ride(){ Position = new Vector3(-742.8346f, -1773.291f, 28), Speed = 120 },
                new Ride(){ Position = new Vector3(-196.9204f, -546.7769f, 26), Speed = 120 },
                new Ride(){ Position = new Vector3(130.9345f, -545.3768f, 33), Speed = 120 },
                new Ride(){ Position = new Vector3(246.7274f, -549.4575f, 42), Speed = 70 },
                new Ride(){ Position = new Vector3(172.8465f, -788.5532f, 30), Speed = 70 },
                new Ride(){ Position = new Vector3(110.1009f, -973.5505f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(57.22042f, -1112.927f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(81.27916f, -1335.946f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(122.6912f, -1392.933f, 28), Speed = 70 },
                new Ride(){ Position = new Vector3(86.93179f, -1436.302f, 28), Speed = 70 } //15
            };

            //var drivingSchool = new DrivingSchool(0, new Vector3(76.07864f, -1455.614f, 29.29165f), new Models.Location(new Vector3(87.29823f, -1436.695f, 28.59703f), new Vector3(0.03492294f, 0.02653446f, 142.6451f)), Models.LicenseType.Car, 2500, schooltrajetcar, VehicleModel.Asea);
            var drivingSchool = new DrivingSchool(0, new Vector3(76.07864f, -1455.614f, 28.29165f), new Models.Location(new Vector3(87.29823f, -1436.695f, 28.59703f), new Vector3(0.03492294f, 0.02653446f, 142.6451f)), Models.LicenseType.Car, 2, schooltrajetcar, VehicleModel.Asea);

            drivingSchools.Add(drivingSchool);

        }
/*
        private async Task DrivingSchool_End(object[] args)
        {
            IPlayer client = args[0] as IPlayer;

            if (!client.Exists)
                return;

            if (byte.TryParse(args[1].ToString(), out byte id))
            {
                var school = drivingSchools.Find(p => p.Id == id);
                await school?.End(client, int.Parse(args[2].ToString()));
            }
        }

        private async Task DrivingSchool_Cancel(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (byte.TryParse(args[0].ToString(), out byte id))
            {
                var school = drivingSchools.Find(p => p.Id == id);
                await school?.Cancel(client);
            }
        }

        private async Task DrivingSchoolManager_Open(IPlayer client, object[] args)
        {
            if (!client.Exists)
                return;

            if (byte.TryParse(args[0].ToString(), out byte id))
            {
                var school = drivingSchools.Find(p => p.Id == id);
                school?.OpenMenuDrivingSchool(client);
            }

            await Task.CompletedTask;
        }*/
    }
}
