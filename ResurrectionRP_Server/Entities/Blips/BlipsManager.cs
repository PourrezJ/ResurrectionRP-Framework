using System.Numerics;
using System.Collections.Concurrent;

namespace ResurrectionRP_Server.Entities.Blips
{
    public static class BlipsManager
    {
        public static ConcurrentDictionary<int, Blips> BlipList = new ConcurrentDictionary<int, Blips>();

        public static Blips CreateBlip(string name, Vector3 pos, BlipColor color, int sprite, float scale = 1f, bool shortRange = true)
        {
            Blips blip = null;

            lock (Streamer.Streamer.ListStaticEntities)
            {
                blip = new Blips(name, pos, (int)color, sprite, scale, shortRange, Streamer.Streamer.StaticEntityNumber++);
            }

            Streamer.Streamer.AddStaticEntityBlip(blip);
            BlipList.TryAdd(blip.id, blip);

            return blip;
        }

        public static Blips CreateBlip(string name, Vector3 pos, int color, int sprite, float scale = 1f, bool shortRange = true)
        {
            Blips blip = null;

            lock (Streamer.Streamer.ListStaticEntities)
            {
                blip = new Blips(name, pos, color, sprite, scale, shortRange, Streamer.Streamer.StaticEntityNumber++);
            }

            Streamer.Streamer.AddStaticEntityBlip(blip);
            BlipList.TryAdd(blip.id, blip);

            return blip;
        }
        public static Blips SetColor(Blips entity, int color)
        {
            Blips blip = BlipList[entity.id];
            blip.color = color;
            Streamer.Streamer.UpdateStaticEntityBlip(blip);
            return entity;
        }

        public static bool Destroy(Blips entity)
        {
            Streamer.Streamer.DestroyStaticEntityBlip(BlipList[entity.id]);

            if (BlipList.TryRemove(entity.id, out Blips blip))
                return true;
            else
                return false;
        }
    }
}
