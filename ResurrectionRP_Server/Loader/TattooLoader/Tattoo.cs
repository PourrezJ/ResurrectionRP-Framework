namespace ResurrectionRP_Server.Loader.TattooLoader
{
    /*
 * ZONE_TORSO = 0
 * ZONE_HEAD = 1
 * ZONE_LEFT_ARM = 2
 * ZONE_RIGHT_ARM = 3
 * ZONE_LEFT_LEG = 4
 * ZONE_RIGHT_LEG = 5
 */


    public class Tattoo
    {
        public string Collection { get; set; }
        public string Name { get; set; }
        public string LocalizedName { get; set; }
        public string HashNameMale { get; set; }
        public string HashNameFemale { get; set; }
        public string Zone { get; set; }
        public int ZoneID { get; set; }
        public int Price { get; set; }
    }
}
