namespace ResurrectionRP_Server.Society.Societies.Bennys
{
    public partial class Bennys : Garage
    {
        private static GarageData.EsthetiqueData[] _esthetiqueModList = new GarageData.EsthetiqueData[]
        {
            new GarageData.EsthetiqueData(0, "Spoiler", 2000),
            new GarageData.EsthetiqueData(1, "Pare-choc Avant", 2000),
            new GarageData.EsthetiqueData(2, "Pare-choc Arrière", 2000),
            new GarageData.EsthetiqueData(3, "Jupe latérale", 2000),
            new GarageData.EsthetiqueData(4, "Échappement", 2000),
            new GarageData.EsthetiqueData(5, "Cadre", 2000),
            new GarageData.EsthetiqueData(6, "Grille", 2000),
            new GarageData.EsthetiqueData(7, "Capot", 2000),
            new GarageData.EsthetiqueData(8, "Aile", 2000),
            new GarageData.EsthetiqueData(9, "Aile Droite", 2000),
            new GarageData.EsthetiqueData(10, "Toit", 2000),
            new GarageData.EsthetiqueData(14, "Klaxon", 2000),
            new GarageData.EsthetiqueData(22, "Xenon", 2000),
            new GarageData.EsthetiqueData(23, "Roues", 2000),
            new GarageData.EsthetiqueData(25, "Supports de plaque", 2000),
            new GarageData.EsthetiqueData(27, "Design intérieur", 2000),
            new GarageData.EsthetiqueData(28, "Ornements", 2000),
            new GarageData.EsthetiqueData(30, "Compteur", 2000),
            new GarageData.EsthetiqueData(33, "Volant", 2000),
            new GarageData.EsthetiqueData(34, "Levier de vitesses", 2000),
            new GarageData.EsthetiqueData(35, "Plaques", 2000),
            new GarageData.EsthetiqueData(38, "Hydraulics", 2000),
            new GarageData.EsthetiqueData(46, "Teinte des vitres", 2000),
            new GarageData.EsthetiqueData(48, "Skin", 2000),
            new GarageData.EsthetiqueData(62, "Assiette", 2000),
            new GarageData.EsthetiqueData(69, "Teinte des vitres", 2000),
            new GarageData.EsthetiqueData(74, "Couleur du tableau de bord", 2000),
            new GarageData.EsthetiqueData(75, "Couleur de garniture", 2000)
        };

        private static GarageData.PerformanceData[] _performanceModList = new GarageData.PerformanceData[]
        {
            new GarageData.PerformanceData(11, "Moteur", new double[] { 0, 7000, 14000, 21000, 35000, 50000}),
            new GarageData.PerformanceData(12, "Frein", new double[] { 0, 3500, 7000, 10500}),
            new GarageData.PerformanceData(13, "Transmission", new double[] { 0, 5250, 10500, 15750}),
            new GarageData.PerformanceData(15, "Suspensions", new double[] { 0, 1750, 2625, 3500, 5250})
            // new GarageData.PerformanceData(18, "Turbo", new double[] { 0, 78250 })
        };
    }
}
