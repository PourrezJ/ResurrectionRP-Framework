using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using ResurrectionRP_Server.Models.InventoryData;
using System.Collections.Generic;
using System.Linq;

namespace ResurrectionRP_Server.Models
{
    public class Bourse
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ItemID, double> Values = new Dictionary<ItemID, double>()
        {
            { ItemID.Cuivre, 0 },
            { ItemID.BouteilleTraite, 0 },
            { ItemID.GrapeJuice, 0 },
            { ItemID.Rhum, 0 },
        };

        public double GetCurrentPrice(ItemID itemID, double somme)
        {
            if (Values.ContainsKey(itemID))
            {
                double pourcent = somme * (Values[itemID] / 100);
                return System.Math.Round(somme + pourcent, 2);
            }
            return 0;
        }

        public void Update(ItemID itemID, int quantity)
        {
            lock (Values)
            {
                if (Values.ContainsKey(itemID))
                {
                    double somme = quantity * (0.2 / 100);
                    Values[itemID] -= somme;

                    for(int i = 0; i < Values.Count; i++)
                    {
                        var item = Values.ElementAt(i);

                        if (itemID == item.Key)
                            continue;

                        Values[item.Key] += somme;
                    }
                }
            }
        }
    }
}
