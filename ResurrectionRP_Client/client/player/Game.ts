import * as alt from 'alt';
import * as game from 'natives';
import * as enums from 'client/Utils/Enums/Enums';
import * as chat from 'client/chat/chat';
import Raycast, * as raycast from 'client/Utils/Raycast';

import { Time as TimeLib } from 'client/Env/Time';
import { Survival as SurvivalLib } from 'client/player/Survival';
import { Hud as HudLib } from 'client/player/Hud';
import { RPGInventoryManager } from 'client/menus/rpgInventory/RPGinventory';
import { Weather as WeatherLib } from 'client/Env/Weather';
import { Interaction as InteractionLib } from 'client/Player/Interaction';
import { Doors as DoorsManagerLib } from 'client/Env/Doors';
import { PhoneManager } from 'client/phone/PhoneManager';
import { DrivingSchool } from 'client/DrivingSchool';


export class Game {
    //region Static Var
    
    firstCheck: number[] = [0, 100, 200, 300, 400, 500600, 700, 800, 900, 1000];
    //endregion Static Var

    //region Variables
    private _LevelRank: enums.AdminRank = enums.AdminRank.Player;
    public get LevelRank(): number { return this._LevelRank }


    private _PlayerName: string;
    public get PlayerName(): string { return this._PlayerName }

    private _Time: TimeLib = new TimeLib();
    public get Time(): TimeLib { return this._Time; }

    private _Weather: WeatherLib = null;
    public get Weather(): WeatherLib { return this._Weather; }

    private _Doors: DoorsManagerLib = null;
    public get Doors(): DoorsManagerLib { return this._Doors; }

    private _IsConnected: boolean;
    public get IsConnected(): boolean { return this._IsConnected; }

    private _IsCuffed: boolean;
    public get IsCuffed(): boolean { return this._IsCuffed; }

    private _IsDebug: boolean;
    public get IsDebug(): boolean { return this._IsDebug; }

    public GetDoor: boolean;

    private _stats: string[] = [ "SP0_STAMINA", "SP0_STRENGTH", "SP0_LUNG_CAPACITY", "SP0_WHEELIE_ABILITY", "SP0_FLYING_ABILITY", "SP0_SHOOTING_ABILITY", "SP0_STEALTH_ABILITY" ]
    private lastCheck: number;
    //endregion variables


    //REGION Pools

    private _Survival: SurvivalLib;
    public get Survival(): SurvivalLib { return this._Survival; }

    private _Hud: HudLib;
    public get Hud(): HudLib { return this._Hud; }

    private _Inventory: RPGInventoryManager;
    public get Inventory(): RPGInventoryManager { return this._Inventory; }

    //End region pools
    //constructor
    constructor(
        StaffRank: number,
        IdentiteName: string,
        Money: number,
        Thirst: number,
        Hunger: number,
        AnimSettings: string,
        Time: string,
        Weather: string,
        WeatherWind: number,
        WeatherWindDirection: number,
        isDebug: boolean,
        Location: string
    ) {
        try {

            alt.log("Chargement de vos données");
            var playerId = alt.Player.local.scriptID;

            this._LevelRank = StaffRank;
            this._PlayerName = IdentiteName;
            this._Survival = new SurvivalLib(Hunger, Thirst);
            var time = JSON.parse(Time);
            this._Time = new TimeLib(time.Hours, time.Minutes, time.Seconds);
            this._IsDebug = isDebug;

            new InteractionLib();
            new PhoneManager();
            new DrivingSchool();

            game.setAudioFlag('LoadMPData', true);
            game.setAudioFlag('DisableFlightMusic', true);
            game.setAudioFlag('PoliceScannerDisabled', true);

            for (var i: number = 0; i <= 5; i++)
                game.disableHospitalRestart(i, true);

            for (var i: number = 12; i <= 19; i++)
                game.disableControlAction(0, i, true);
            game.setPlayerHealthRechargeMultiplier(playerId, 0);

            alt.log('Données chargées');

            alt.log('Chargement des pools');
            this._Hud = new HudLib(Money);
            this._Doors = new DoorsManagerLib();
            this._Inventory = new RPGInventoryManager();
            this._Weather = new WeatherLib(Weather, WeatherWind, WeatherWindDirection);
            alt.log('Chargement des pools done');

            alt.log("Chargement des stats");
            game.setPedConfigFlag(playerId, 35, false);
            game.setPedConfigFlag(playerId, 429, true);

            game.setRelationshipBetweenGroups(2, game.getHashKey("SYNCPED"), game.getHashKey("SYNCPED_TEAMMATES"));
            game.setRelationshipBetweenGroups(2, game.getHashKey("SYNCPED_TEAMMATES"), game.getHashKey("SYNCPED"));

            game.startAudioScene("FBI_HEIST_H5_MUTE_AMBIENCE_SCENE");
            game.setPedConfigFlag(alt.Player.local.scriptID, 35, true);
            game.setPedConfigFlag(alt.Player.local.scriptID, 429, true);

            alt.log("Stats terminées");

            this._IsConnected = true;
        } catch (ex) {
            alt.log(ex);
        }
        

        alt.on("update", () => {
            //game.disableControlAction(0, 75, true);
            //game.disableControlAction(0, 58, true);

            this._Time.OnTick();

            // HUD
            if (!this.Hud.Hide) {
                if (this.Hud.Browser != null)
                    this.Hud.Browser.emit("setHUD", this.Survival.Hunger,
                        this.Survival.Thirst,
                        0, //Salty is talking SaltyClient.Voice.IsTalking
                        0, //Salty range
/*                                        var range = "Parler";
                if (RAGE.Elements.Player.LocalPlayer.GetSharedData(SaltyShared.SharedData.Voice_VoiceRange) != null)
                    range = (string)RAGE.Elements.Player.LocalPlayer.GetSharedData(SaltyShared.SharedData.Voice_VoiceRange);*/
                        this.Hud.Money,
                        0   //Is mic muted SaltyClient.Voice.IsMicrophoneMuted
                    );
            }

        });

        alt.on("toggleChatAdminRank", this.toggleChatAdminRank);



    }
    //end constructor
    //methods

    public toggleChatAdminRank = () => {
        if (this.LevelRank > enums.AdminRank.Player)
            alt.emit("toggleChat", true);
    }

    public static closeAllMenus() {
        alt.emit("InventoryManager_CloseMenu");
        alt.emit("toggleChat");
    }

    //endmethods
}