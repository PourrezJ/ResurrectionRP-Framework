namespace ResurrectionRP_Server.Utils.Enums
{
    static class Events
    {
        public const string PlayerInitialised = "PlayerInitialised";

        // Menu
        public const string OpenAdminMenu = "OpenAdminMenu";
        public const string OpenMainMenu = "OpenMainMenu";
        public const string OpenVehicleMenu = "OpenVehicleMenu";
        public const string OpenATMMenu = "OpenATMMenu";
        public const string OpenGasPompMenu = "OpenGasPompMenu";
        public const string MenuManager_CloseMenu = "MenuManager_CloseMenu";
        public const string InteractionKey = "InteractionKey";
        public const string BuyKey = "BuyKey";
        public const string PhoneKey = "OpenPhone";
        public const string HandsUP = "HandsUP";
        public const string UpdateNeeded = "UpdateNeeded";

        public const string SetToWayPoint = "SetToWayPoint";
        public const string UpdateMoneyHUD = "UpdateMoneyHUD";

        public const string AnnonceGlobal = "AnnonceGlobal";

        public const string startIntroduction = "start_introduction";

        public const string setWaypoint = "SetWaypoint";
        public const string createMarker = "createMarker";

        public const string merrychristmas = "merrychristmas";
        public const string startstatut = "startstatut";

        public const string UnlockVehicle = "UnlockVehicle";

        public const string OpenBox = "OpenBox";

        // API Extension
        public const string setPlayerMovementClipset = "setPlayerMovementClipset";
        public const string Display_subtitle = "Display_subtitle";

        public const string UpdateHungerThirst = "UpdateHungerThirst";
        public const string PlayAnimation = "PlayAnimation";


        //PED API
        public const string CreatePed = "CreatePed";
        public const string CreateAllPed = "CreateAllPed";
        public const string TaskEnterVehicle = "TaskEnterVehicle";
        public const string TaskPlayAnim = "TaskPlayAnim";

        //Colshapes
        public const string OnPlayerEnterColShape = "OnPlayerEnterColshape";
        public const string OnPlayerExitColShape = "OnPlayerExitColshape";
        public const string OnVehicleEnterColShape = "OnVehicleEnterColshape";
        public const string OnVehicleExitColShape = "OnVehicleExitColshape";

    }
}
