using AltV.Net.Elements.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ResurrectionRP_Server
{
    // BUG v752 : ColShape.IsEntityIn() returns always false
    public static class ColshapeExtension
    {
        public static void AddEntity(this IColShape colshape, IEntity entity)
        {
            if (!entity.Exists)
                return;

            lock (colshape)
            {
                colshape.GetData("Entities", out List<IEntity> entities);

                if (entities != null && !entities.Contains(entity))
                    entities.Add(entity);
                else if (entities == null)
                {
                    entities = new List<IEntity>();
                    entities.Add(entity);
                    colshape.SetData("Entities", entities);
                }
            }
        }

        public static void RemoveEntity(this IColShape colshape, IPlayer entity)
        {
            lock (colshape)
            {
                colshape.GetData("Entities", out List<IEntity> entities);

                if (entities.Contains(entity))
                    entities.Remove(entity);
            }
        }

        public static bool IsEntityInColShape(this IColShape colshape, IEntity client)
        {
            colshape.GetData("Entities", out List<IEntity> entities);

            if (entities == null || !entities.Contains(client))
                return false;

            return true;
        }
    }
}
