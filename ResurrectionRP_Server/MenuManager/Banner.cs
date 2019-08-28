namespace ResurrectionRP_Server
{
    public sealed class Banner
    {
        #region Public properties
        public string Dict;
        public string Name;
        #endregion

        #region Public static readonly properties
        public static readonly Banner Barber = new Banner("shopui_title_barber", "shopui_title_barber");
        public static readonly Banner Barber2 = new Banner("shopui_title_barber2", "shopui_title_barber2");
        public static readonly Banner Barber3 = new Banner("shopui_title_barber3", "shopui_title_barber3");
        public static readonly Banner Barber4 = new Banner("shopui_title_barber4", "shopui_title_barber4");
        public static readonly Banner CarMod = new Banner("shopui_title_carmod", "shopui_title_carmod");
        public static readonly Banner CarMod2 = new Banner("shopui_title_carmod2", "shopui_title_carmod2");
        public static readonly Banner ClubHouseMod = new Banner("shopui_title_clubhousemod", "shopui_title_clubhousemod");
        public static readonly Banner Convenience = new Banner("shopui_title_conveniencestore", "shopui_title_conveniencestore");
        public static readonly Banner Darts = new Banner("shopui_title_darts", "shopui_title_darts");
        public static readonly Banner Franklin = new Banner("shopui_title_graphics_franklin", "shopui_title_graphics_franklin");
        public static readonly Banner Garage = new Banner("shopui_title_ie_modgarage", "shopui_title_ie_modgarage");
        public static readonly Banner Gas = new Banner("shopui_title_gasstation", "shopui_title_gasstation");
        public static readonly Banner Golf = new Banner("shopui_title_golfshop", "shopui_title_golfshop");
        public static readonly Banner Guns = new Banner("shopui_title_gr_gunmod", "shopui_title_gr_gunmod");
        public static readonly Banner Hangar = new Banner("shopui_title_sm_hangar", "shopui_title_sm_hangar");
        public static readonly Banner Michael = new Banner("shopui_title_graphics_michael", "shopui_title_graphics_michael");
        public static readonly Banner GunClub = new Banner("shopui_title_gunclub", "shopui_title_gunclub");
        public static readonly Banner HighFashion = new Banner("shopui_title_highendfashion", "shopui_title_highendfashion");
        public static readonly Banner HighSalon = new Banner("shopui_title_highendsalon", "shopui_title_highendsalon");
        public static readonly Banner Liquor = new Banner("shopui_title_liqourstore", "shopui_title_liqourstore");
        public static readonly Banner Liquor2 = new Banner("shopui_title_liqourstore2", "shopui_title_liqourstore2");
        public static readonly Banner Liquor3 = new Banner("shopui_title_liqourstore3", "shopui_title_liqourstore3");
        public static readonly Banner LowFashion = new Banner("shopui_title_lowendfashion", "shopui_title_lowendfashion");
        public static readonly Banner LowFashion2 = new Banner("shopui_title_lowendfashion2", "shopui_title_lowendfashion2");
        public static readonly Banner MidFashion = new Banner("shopui_title_midfashion", "shopui_title_midfashion");
        public static readonly Banner MovieMasks = new Banner("shopui_title_movie_masks", "shopui_title_movie_masks");
        public static readonly Banner Sale = new Banner("mpshops", "shopui_title_graphics_sale");
        public static readonly Banner SuperMod = new Banner("shopui_title_supermod", "shopui_title_supermod");
        public static readonly Banner Tattoos = new Banner("shopui_title_tattoos", "shopui_title_tattoos");
        public static readonly Banner Tattoos2 = new Banner("shopui_title_tattoos2", "shopui_title_tattoos2");
        public static readonly Banner Tattoos3 = new Banner("shopui_title_tattoos3", "shopui_title_tattoos3");
        public static readonly Banner Tattoos4 = new Banner("shopui_title_tattoos4", "shopui_title_tattoos4");
        public static readonly Banner Tattoos5 = new Banner("shopui_title_tattoos5", "shopui_title_tattoos5");
        public static readonly Banner Tennis = new Banner("shopui_title_tennis", "shopui_title_tennis");
        public static readonly Banner Trevor = new Banner("shopui_title_graphics_trevor", "shopui_title_graphics_trevor");
        public static readonly Banner VehicleUpgrade = new Banner("shopui_title_exec_vechupgrade", "shopui_title_exec_vechupgrade");
        #endregion

        #region Constructor
        private Banner(string dict, string name)
        {
            Dict = dict;
            Name = name;
        }
        #endregion
    }
}
