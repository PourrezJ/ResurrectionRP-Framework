
using System.Collections.Generic;
using ItemID = ResurrectionRP_Server.Models.InventoryData.ItemID;
using WeaponHash = ResurrectionRP_Server.Utils.Enums.WeaponHash;
using SeedType = ResurrectionRP_Server.Illegal.WeedLab.SeedType;
using ResurrectionRP_Server.Models;
using ResurrectionRP_Server.Phone;

namespace ResurrectionRP_Server.Items
{
    class LoadItem
    {
        private static List<Models.Item> items;
        public static List<Models.Item> ItemsList
        {
            get
            {
                if (items == null)
                {
                    items = new List<Item>();
                    items.AddRange(LoadItemList());
                }
                return items;
            }
        }

        private static List<Item> LoadItemList()
        {
            return new List<Item>()
                {
                    new Eat(ItemID.Lasagnes, "Lasagnes", "Les lasagnes de la Mama!", 1 , true, true, true, food:15),
                    new Eat(ItemID.Fromage, "Fromage de chèvre", "Fromage de chèvre bien odorant", 1, true, true,true, food:15, isDockable: true, itemPrice: 23, icon:"cheese"),
                    new Eat(ItemID.Sardine, "Sardine", "La sardine est une espèce de poisson de la famille des Clupeidae", 1, true, true,true, food:2, isDockable: true, itemPrice: 4, icon:"sardine"),
                    new Eat(ItemID.JambonBeurre, "Sandwich jambon beurre", "Sandwich jambon beurre au pain sans gluten", 1 , true, true, true, food:15,icon:"jambon-beurre", isDockable:true, itemPrice:20),
                    new Eat(ItemID.Donuts, "Un Donuts", "D'Oh!", 1 , true, true, true, food:15, icon:"donuts", isDockable:true, itemPrice:15),
                    new Eat(ItemID.Nems, "Un Nems de poulet", "Nems au poulet et autre viande", 1 , true, true, true, food:10, icon:"nems", isDockable:true, itemPrice:8),
                    new Eat(ItemID.Sushi, "Du Sushi", "Du bon thon rose.", 1 , true, true,true, food:30, icon:"sushi", isDockable:true, itemPrice:25),
                    new Eat(ItemID.Pizza, "Pizza", "Pizza pas très ronde.", 1 , true, true, true, food:15, icon:"pizza", isDockable:true, itemPrice:13),
                    new Eat(ItemID.Caviar, "Caviar", "Oeuf de gélatine.", 1 , true, true, true, food:30, icon:"cavair", isDockable:true, itemPrice:46),
                    new Eat(ItemID.Syrniki, "Syrniki", "Crêpe russe.", 1 , true, true, true, food:15, icon:"syrniki", isDockable:true, itemPrice:20),
                    new Eat(ItemID.Fricadelle, "Fricadelle", "On ne sait pas bien ce qu il y a dedans...", 1 , true, true, true, food:30, icon:"fricadelle", isDockable:true, itemPrice:25),
                    new Eat(ItemID.HotDog, "HotDog", "Du pur chien chaud ...", 1 , true, true, true, food:15, icon:"hotdog", isDockable:true, itemPrice:12),
                    new Eat(ItemID.Chocolat, "Chocolat", "Belge fourré …", 1 , true, true, true, food:15, icon:"chocolate", isDockable:true, itemPrice:12),
                    new Eat(ItemID.Tacos , "Tacos ", "Sans gluten", 1 , true, true, true, food:30, icon:"tacos", isDockable:true, itemPrice:12),
                    new Eat(ItemID.Coffee, "Un Café", "Du café soluble dégueulasse.", 1 , true, true,true, drink:15, icon:"coffee", isDockable:true, itemPrice:12),
                    new Eat(ItemID.Water, "Bouteille Eau", "Eau potable?.", 1 , true, true, true, drink:20, icon:"water", isDockable: true, itemPrice: 20),
                    new Eat(ItemID.Sprunk, "Canette de Sprunk", "Peut-être radioactif...", 1 , true, true, true, drink:15, icon:"sprunk", isDockable: true, itemPrice: 12),
                    new Eat(ItemID.Ramen, "Ramen", "Des nouilles chinoise en gros ...", 1 , true, true, true, food:15),
                    new Eat(ItemID.Grappa, "Grappa", "Je suis rital et je le reste ... ", 1 , true, true, true, drink:20),
                    new Eat(ItemID.Apple , "Pomme", "Une pomme verte et juteuse, oh y'a un trou dedans", 2 , true, true, true, food:5, icon:"apple"),
                    new Eat(ItemID.AppleJuice , "Jus de pomme", "mi pisse, mi pomme", 1 , true, true, true, food:5, drink:5),
                    new Eat(ItemID.GrapeJuice , "Jus de raisin", "mi pisse, mi raisin", 1 , true, true, true, food:5, drink:5),
                    new Eat(ItemID.Pain , "Pain", "Du pain rassis", 1 , true, true, true, food:5),
                    new Eat(ItemID.Raisin, "Raisin", "Du raisin vert comestible", 0.005, true, true,true, food: 2, drink:2, icon:"grapes"),

                    // new Eat(ItemID.Nouille, "Des Nouilles Chinoises", "Une boite de nouille", 1 , true, true, true, food:15, isDockable:true, itemPrice:11.2),

                    new Unusable(ItemID.PetrolBrute, "Pétrole brute", "", 2, icon:"petrol-brut"),
                    new Unusable(ItemID.Petrol, "Pétrole Raffiné", "Pas sûre qu'il soit aussi éfficase que la mirabelle.", 1, icon:"petrol"),
                    new Unusable(ItemID.GrappeRaisin, "Grappe de raisin", "Une grappe de raisin vert", 2, icon:"grapes"),
                    new Unusable(ItemID.Sable, "Sable", "Du sable qui sent la pisse de chat.", 2),
                    new Unusable(ItemID.Bouteille, "Bouteille en Verre Brute", "Une bouteille en verre vide.", 1, icon:"bouteille-verre"),
                    new Unusable(ItemID.BouteilleTraite, "Bouteille en Verre Stérilisé", "Une bouteille en verre vide stérile.", 1, icon:"bouteille-verre", isDockable: true, itemPrice: 268),
                    new Unusable(ItemID.RondinDeBois, "Rondin de bois", "Un rondin de bois humide.", 3, icon:"wood"),
                    new Unusable(ItemID.GSauvetage, "Gilet de sauvetage", "à ton âge tu ne sais toujours pas nagez ?", 1 , true, true, true),

                    new GasJerrycan(ItemID.Jerrycan,"Jerrican d'essence","Un jerrycan d'essence, utilisé le près d un véhicule pour le rationner.",16, icon:"jerrycan", isDockable: true, itemPrice: 35, isUsable: true),

                    new HealItem(ItemID.Bandages,"Bandages","Des bandages pour soigner les blessures légères.",1,true,true,true,life:5, icon:"bandage"),
                    new HealItem(ItemID.KitSoin,"Kit de Soin","Un Kit de soin pour les blessures graves.",2,true,true,true,life:75, icon:"kit-soin"),
                    new Defibrilator(ItemID.Defibrilateur,"Defibrilateur","Un défibrilateur et tout le matériel nécessaire à une réanimation.",3,true,true,true, icon:"defibrilator"),

                    new IdentityCard(ItemID.IdentityCard, "Passeport", ""),

                    new HandCuff(ItemID.Handcuff, "Paire de Menottes", "", 1, icon:"menotte"),
                    new HandCuff(ItemID.Handcuff, "Serflex", "", 1, icon:"menotte", isDockable: true, itemPrice: 50),

                    new CrateTools(ItemID.CrateTool, "Caisse a outil", "", 5, icon:"caissetool"),

                    new Alcohol(ItemID.Sake, "Du Sake", "BANNZAIII!!!!!", 1, true, true,true, drink:15, isDockable: true, itemPrice: 98, icon:"sake", alcohol:1),
                    new Alcohol(ItemID.Champagne, "Champagne", "Juste si tu es riche", 1 , true, true, true, drink:20, icon:"champagne", isDockable: true, itemPrice: 392, alcohol:0.1),
                    new Alcohol(ItemID.Whisky, "Whisky", "Juste si tu es pauvre", 1 , true, true, true, drink:20, timer:120000, icon:"whisky", isDockable: true, itemPrice: 294, alcohol:0.5),
                    new Alcohol(ItemID.Gnole, "Gnole", "Made in Dakota qui peut rendre aveugle", 1 , true, true, true,icon:"gnole", drink:20, timer:120000, isDockable: true, itemPrice: 98, alcohol:1),
                    new Alcohol(ItemID.Biere, "Biere", "Biere Belge bien chaude", 1 , true, true, true, drink:15,icon:"beer", timer:40000, isDockable: true, itemPrice: 98),
                    new Alcohol(ItemID.Tequila, "Tequila", "AIE CARAMBA", 1 , true, true, true,icon:"tequila", drink:20, timer:90000, isDockable: true, itemPrice: 196, alcohol:0.5),
                    new Alcohol(ItemID.Vodka, "Vodka", "100% pur patate!", 1 , true, true, true,icon:"vodka", drink:20, timer:120000, isDockable: true, itemPrice: 294, alcohol:0.5),
                    new Alcohol(ItemID.Mirabelle , "Mirabelle", "Peux servir d'antiseptique ou de carburant.", 1 , true, true, true, drink:30, timer:120000, alcohol:1),
                    new Alcohol(ItemID.Vine, "Une Bouteille de Vin", "Une bouteille de Vin.", 1, true, true,true, icon:"vin"),
                    new Alcohol(ItemID.Vine, "Martini", "Une bouteille de Vin.", 1, true, true,true, icon:"whisky"),
                    new Alcohol(ItemID.Vine, "Alcool de serpent", "", 1, true, true,true, icon:"vin", isDockable: true, itemPrice: 98, alcohol:1),
                    new Alcohol(ItemID.Rhum , "Rhum", "Tous les chemins mènent au Rhum", 1 , true, true, true, drink:20, timer:120000, isDockable: false, itemPrice: 140, icon: "rum", alcohol:0.5),


                    new Unusable(ItemID.Plastic, "Matière Polymère", "", 4),

                    new BuildingItem(ItemID.Building, "barriere", "", 1, modelhash:1072616162),

                    new ClothItem(ItemID.TShirt, "TShirt Blanc", "Un tshirt qui sent la sueur", new ClothData(1, 0, 0), 0, true, false, false, true, false, 0, classes:"shirt", icon:"shirt"),

                    new PhoneItem(ItemID.Phone, "Téléphone", "", PhoneManager.GeneratePhone(), 1, true, true, false, true, true, 500, "phone", "phone"),
                    new RadioItem(ItemID.Radio, "Radio", "", 1, true, true, false, true, true, 500, "radio", "talky", "radio"),
                    new BagItem(ItemID.Bag, "Backpack", "", new ClothData(1, 0, 0), 25, 20, 1, true, false, false, true, true, 500, classes: "backpack", icon: "backpack"),

                    new Weapons(ItemID.Weapon, "Poignard", "", 1, hash: WeaponHash.Dagger, isDockable: true, itemPrice: 2730),
                    new Weapons(ItemID.Weapon, "Batte", "", 3, hash: WeaponHash.Bat, isDockable: true, itemPrice: 2520),
                    new Weapons(ItemID.Weapon, "Pied de biche", "", 3, hash: WeaponHash.Crowbar, isDockable: true, itemPrice: 3150),
                    new Weapons(ItemID.Weapon, "Club de golf", "", 3, hash: WeaponHash.GolfClub, isDockable: true, itemPrice: 3150),
                    new Weapons(ItemID.Weapon, "Marteau (arme)", "", 3, hash: WeaponHash.Hammer, isDockable: true, itemPrice: 3150),
                    new Weapons(ItemID.Weapon, "Hachette", "", 3, hash: WeaponHash.Hatchet, isDockable: true, itemPrice: 3780),
                    new Weapons(ItemID.Weapon, "Poing américain", "", 3, hash: WeaponHash.KnuckleDuster, isDockable: true, itemPrice: 1785),
                    new Weapons(ItemID.Knife, "Couteau", "", 2, hash: WeaponHash.Knife, isDockable: true, itemPrice: 2940),
                    //new Weapons(ItemID.Weapon, "Machette", "", 3, hash: WeaponHash.Knife, isDockable: true, itemPrice: 3360),
                    new Weapons(ItemID.Weapon, "Cran d'arret", "", 3, hash: WeaponHash.SwitchBlade, isDockable: true, itemPrice: 3360),
                    new Weapons(ItemID.Weapon, "Cle anglaise", "", 3, hash: WeaponHash.Wrench, isDockable: true, itemPrice: 3150),
                    new Weapons(ItemID.Weapon, "Hache", "", 3, hash: WeaponHash.BattleAxe, isDockable: true, itemPrice: 3150),
                    new Weapons(ItemID.Weapon, "Queue de Billard", "", 3, hash: WeaponHash.PoolCue, isDockable: true, itemPrice: 2100),
                    new Weapons(ItemID.Weapon, "Pistolet", "", 3, hash: WeaponHash.Pistol, isDockable: true, itemPrice: 28000),
                    new Weapons(ItemID.Weapon, "Heavy Revolver", "", 3, hash: WeaponHash.HeavyPistol, isDockable: true, itemPrice: 40000),
                    new Weapons(ItemID.Weapon, "Double Action Revolver", "", 3, hash: WeaponHash.DoubleAction, isDockable: true, itemPrice: 40000),
                    //new Weapons(ItemID.Weapon, "Hachette en pierre", "", 3, hash: WeaponHash.StoneHatchet, isDockable: true, itemPrice: 2100),
                    new Weapons(ItemID.Weapon, "Katana", "", 3, hash: WeaponHash.Bottle, isDockable: true, itemPrice: 3100),
                    new Weapons(ItemID.Weapon, "Lampe Torche", "", 1, hash: WeaponHash.Flashlight, isDockable: true, itemPrice: 250),

                    new HealItem(ItemID.RhumLiquide, "Rhum Liquide", "Déinfecte et sert de carburant.",1,true,true,true, life:0, icon:"bandage"),
                    new Unusable(ItemID.Canneasurcre, "Canne a sucre", "", 2),

                    new Unusable(ItemID.BouquetFleur, "Bouquet de fleur", "", 1, isDockable: true, itemPrice: 100),
                    new Unusable(ItemID.Bague, "Bague Jourdan Acier", "", 1, isDockable: true, itemPrice: 500),
                    new Unusable(ItemID.Bague, "Bague en Or", "", 1, isDockable: true, itemPrice: 1000),

                    new SeedItem(ItemID.GOrange, "Graine d'Orange Bud", "", SeedType.Orange, 0.1, true, false, true, true, isDockable: true, itemPrice: 50,icon:"graine"),
                    new SeedItem(ItemID.GPurple, "Graine de Purple", "", SeedType.Purple, 0.1, true, false, true, true, isDockable: true, itemPrice: 50,icon:"graine"),
                    new SeedItem(ItemID.GSkunk, "Graine de Skunk", "", SeedType.Skunk, 0.1, true, false, true, true, isDockable: true, itemPrice: 50,icon:"graine"),
                    new SeedItem(ItemID.GWhite, "Graine de WhiteWidow", "", SeedType.WhiteWidow, 0.1, true, false, true, true, isDockable: true, itemPrice: 50,icon:"graine"),
                    new Unusable(ItemID.Secateur, "Sécateur", "Permet de se couper les ongles de pieds ou la beuh", 1, icon:"secateur", isDockable: true, itemPrice: 90),
                    new Unusable(ItemID.Hydro, "Système Hydroponique", "Ne permet pas de prendre sa douche", 1, icon:"hydroponics", isDockable: true, itemPrice: 350),
                    new Unusable(ItemID.BSkunk, "Pochon de Skunk", "Un pochon de 25gr de Skunk", 1, icon:"weed"),
                    new Unusable(ItemID.BPurple, "Pochon de Purple", "Un pochon de 25gr de Purple", 1, icon:"weed"),
                    new Unusable(ItemID.BOrange, "Pochon d'Orange Bud", "Un pochon de 25gr d'Orange Bud", 1, icon:"weed"),
                    new Unusable(ItemID.BWhiteWidow, "Pochon de White Widow", "Un pochon de 25gr de White Widow", 1, icon:"weed"),

                    new LockPick(ItemID.LockPick, "Kit de crochetage", "", 1, true, false, true, true),

                   // new Unusable(ItemID.Microphone, "Micro de scène", "Un micro made in china")


                   // Relative to Miners FarmTGa
                    
                    new Axe(ItemID.Hache, "Hache", "Pour couper du bois", 1, icon: "pickaxe"),

                    new Tool(ItemID.Pioche, "Pioche", "Pioche basique", 2, icon: "pickaxe", isDockable: true, itemPrice: 150),
                    new Tool(ItemID.MarteauPiqueur, "Marteau Piqueur", "Marteau piqueur", 15, isUsable: false, isStackable: false, isDropable: true, isDockable: true, itemPrice: 2500, icon: "marteau-piqueur", miningrate: 5),
                    new Tool(ItemID.Marteau, "Marteau (Outil)", "Fond le cuivre", 3, isUsable: false, isStackable: false, isDropable: true, isDockable: true, itemPrice: 150, icon: "marteau", miningrate: 1),
                    new Tool(ItemID.Pelle, "Pelle","Permet de creuser", 2, isDockable: true, itemPrice: 150, miningrate: 1, icon: "pelle"),
                    new Tool(ItemID.Soufflet, "Soufflet", "Permet de souffler le verre", 5, itemPrice: 150, isDockable: true, icon: "unknown-item"),

                    new Unusable(ItemID.DetecteurMetaux, "Detecteur de Metaux", "Utile pour detecter l'or !", 5, false, false, false, true, true, 5000, icon: "metal_detector"),
                    new Unusable(ItemID.Seau, "Seau", "Utile pour contenir du sable", 5, isDockable: true, itemPrice: 150, icon: "seau"),

                    new Unusable(ItemID.CharbonBrute, "Minerai de Charbon brute", "", 2, icon: "charbon_brute"),
                    new Unusable(ItemID.CharbonTraite, "Sac de Charbon", "", 1, icon: "sac_de_charbon"),
                    new Unusable(ItemID.MineraiFer, "Minerai de Fer", "", 2, icon: "minerai_fer"),

                    new Unusable(ItemID.PepiteOr, "Pépite d'or", "", 1, isUsable: false, isStackable: true, isDropable: true, icon: "charbon_brute"),
                    new Unusable(ItemID.LingotOr, "Lingot d'or", "", 2, isStackable: true, icon: "lingot_or"),
                    new Unusable(ItemID.SacArgent, "Sac d'argent", "", 1, isStackable: true, icon: "sac_argent"),

                    new Unusable(ItemID.MineraiCuivre, "Minerai de Cuivre", "Un morceau de roche", 2, icon: "minerai_fer"),
                    new Unusable(ItemID.CuivreFondu, "Cuivre fondu", "A besoin d'être matellé", 2, icon: "cuivre_fondu"),
                    new Unusable(ItemID.Cuivre, "Cuivre", "Du bon cuivre de gitan", 1, icon: "cuivre"),


            };
        }
    }
}
