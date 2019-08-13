
namespace ResurrectionRP_Server.Models
{
    public enum LicenseType
    {
        Car,
        Air,
        Boat,
        Bike
    }

    public class License
    {
        public LicenseType Type;
        public int Point;

        public License(LicenseType type, int point = 12)
        {
            Type = type;
            Point = point;
        }
    }
}
