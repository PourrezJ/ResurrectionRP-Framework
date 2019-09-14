using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System.Numerics;

namespace ResurrectionRP_Server.Utils.Extensions
{
    public static class EntityExtensions
    {
        public static bool TryGetData<T>(this IVehicle entity, string key, out T data)
        {
            if (entity.GetData<T>(key, out var containingData) == false)
            {
                data = default(T);
                return false;
            }

            data = (T)containingData;
            return true;
        }

    }
    

}
