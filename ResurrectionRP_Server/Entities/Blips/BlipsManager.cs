using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Collections.Concurrent;
using AltV.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ResurrectionRP_Server.Entities.Blips
{
    public class BlipsManager
    {
        public ConcurrentDictionary<int, Blips> BlipList = new ConcurrentDictionary<int, Blips>();

        public static int CreateBlip(string name, Vector3 pos, int color, int sprite, float scale = 1f, bool shortRange = true)
        {
            Blips blip = null;

            lock (GameMode.Instance.Streamer.ListStaticEntities)
            {
                blip = new Blips(name, pos, color, sprite, scale, shortRange, GameMode.Instance.Streamer.StaticEntityNumber++);
            }

            GameMode.Instance.Streamer.AddStaticEntityBlip(blip);
            GameMode.Instance.BlipsManager.BlipList.TryAdd(blip.id, blip);

            return blip.id;

        }
        public static int SetColor(int entityId, int color)
        {
            Blips blip = GameMode.Instance.BlipsManager.BlipList[entityId];
            blip.color = color;
            GameMode.Instance.Streamer.UpdateStaticEntityBlip(blip);
            return entityId;
        }

        public static bool Destroy(int entityId)
        {
            GameMode.Instance.Streamer.DestroyStaticEntityBlip(GameMode.Instance.BlipsManager.BlipList[entityId]);

            if (GameMode.Instance.BlipsManager.BlipList.TryRemove(entityId, out Blips blip))
                return true;
            else
                return false;
        }
    }
}
