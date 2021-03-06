﻿import * as alt from 'alt-client';
import * as game from 'natives';
import * as enums from '../Utils/Enums/Enums';
import * as veh from '../Vehicle/Vehicle';
import * as utils from '../Utils/Utils';
import * as ui from '../Helpers/UiHelper';

import { Time as TimeLib } from '../Env/Time';
import { Survival as SurvivalLib } from '../Player/Survival';
import { Hud as HudLib } from '../Player/Hud';
import { RPGInventoryManager } from '../Menus/rpgInventory/RPGinventory';
import { Weather as WeatherLib } from '../Env/Weather';
import { Interaction as InteractionLib } from '../Player/Interaction';
import PhoneManager from '../Phone/PhoneManager';
import { DrivingSchool } from '../DrivingSchool';
import { RadioManager } from '../Menus/RadioManager';
import { DustManManager } from '../Jobs/DustManManager';
import { VoiceChat } from '../Voice/VoiceChat';
import { Medical } from '../Medical';
import { Weedlabs } from '../Env/WeedLabs';
import { Effects } from './Effects';
import { EmergencyCall } from '../EmergencyCall';
import { Dock } from '../Env/Dock';
//import ObjectTool from '../Debug/ObjectTool';

export class Game {
    //region Static Var   
    firstCheck: number[] = [0, 100, 200, 300, 400, 500600, 700, 800, 900, 1000];
    //endregion Static Var

    //region Variables
    private _LevelRank: enums.AdminRank = enums.AdminRank.Player;
    public get LevelRank(): enums.AdminRank { return this._LevelRank }

    private _PlayerName: string;
    public get PlayerName(): string { return this._PlayerName }

    private _Time: TimeLib = new TimeLib();
    public get Time(): TimeLib { return this._Time; }

    private _Weather: WeatherLib = null;
    public get Weather(): WeatherLib { return this._Weather; }

    private _IsConnected: boolean;
    public get IsConnected(): boolean { return this._IsConnected; }

    private _IsCuffed: boolean;
    public get IsCuffed(): boolean { return this._IsCuffed; }

    public static isDebug: boolean = false;

    public GetDoor: boolean;

    private _stats: string[] = [ "SP0_STAMINA", "SP0_STRENGTH", "SP0_LUNG_CAPACITY", "SP0_WHEELIE_ABILITY", "SP0_FLYING_ABILITY", "SP0_SHOOTING_ABILITY", "SP0_STEALTH_ABILITY" ]
    private lastCheck: number;

    private _Survival: SurvivalLib;
    public get Survival(): SurvivalLib { return this._Survival; }

    private _Hud: HudLib;
    public get Hud(): HudLib { return this._Hud; }

    private _Inventory: RPGInventoryManager;
    public get Inventory(): RPGInventoryManager { return this._Inventory; }

    private _Voice: VoiceChat;
    public get Voice(): VoiceChat { return this._Voice; }

    private _Radio: RadioManager;
    public get Radio(): RadioManager { return this._Radio; }

    public DebugInfo: boolean = false;

    constructor(
        StaffRank: number,
        IdentiteName: string,
        Money: number,
        Thirst: number,
        Hunger: number,
        Time: string,
        Weather: string,
        WeatherWind: number,
        WeatherWindDirection: number,
        isDebug: boolean,
        Position: Vector3
    ) {
        try {
            alt.Player.local.setMeta("IsConnected", false);

            game.requestCollisionAtCoord(Position.x, Position.y, Position.z);

            alt.log("Chargement de vos données");
            var playerId = alt.Player.local.scriptID;

            this._LevelRank = StaffRank;
            this._PlayerName = IdentiteName;
            
            var time = JSON.parse(Time);        
            Game.isDebug = isDebug;
            alt.Player.local.setMeta("IsDebug", isDebug);
            alt.Player.local.setMeta("LevelRank", this.LevelRank);

            new InteractionLib();
            new PhoneManager();
            new DrivingSchool();
            new DustManManager();
            new Medical();
            new EmergencyCall();
            new Weedlabs();
            new Effects();
            new Dock();
            //if (isDebug)
            //    new ObjectTool();

            game.setAudioFlag('LoadMPData', true);
            game.setAudioFlag('DisableFlightMusic', true);
            game.setAudioFlag('PoliceScannerDisabled', true);

            for (var i: number = 0; i <= 5; i++)
                game.disableHospitalRestart(i, true);

            game.setPlayerHealthRechargeMultiplier(playerId, 0);

            alt.log('Données chargées');

            alt.log('Chargement des pools');
            this._Time = new TimeLib(time.Hours, time.Minutes, time.Seconds);
            this._Survival = new SurvivalLib(Hunger, Thirst);
            this._Voice = new VoiceChat();
            this._Hud = new HudLib(Money);
            this._Inventory = new RPGInventoryManager();
            this._Weather = new WeatherLib(Weather, WeatherWind, WeatherWindDirection);
            this._Radio = new RadioManager();
            alt.log('Chargement des pools done');

            alt.log("Chargement des stats");

            game.startAudioScene("FBI_HEIST_H5_MUTE_AMBIENCE_SCENE");

            game.setPedConfigFlag(alt.Player.local.scriptID, 35, false);
            game.setPedConfigFlag(alt.Player.local.scriptID, 184, false);
            game.setPedConfigFlag(alt.Player.local.scriptID, 429, true);

            game.setTimeScale(1);
            
            alt.setStat('stamina', 100);
            alt.setStat('strength', 100);
            alt.setStat('lung_capacity', 100);
            alt.setStat('wheelie_ability', 100);
            alt.setStat('flying_ability', 100);
            alt.setStat('shooting_ability', 100);
            alt.setStat('stealth_ability', 100);

            alt.log("Stats terminées");

            alt.Player.local.setMeta("IsConnected", true);

            alt.on('keydown', (key) => {
                if (key == 117)
                    this.DebugInfo = !this.DebugInfo
            });
            /*
            alt.setInterval(() =>
            {
                alt.Player.all.forEach((entity: alt.Player) => {

                    if (!entity.valid)
                        return;

                    let invincible: boolean = entity.getSyncedMeta("SetInvincible");
                    let invisible: boolean = entity.getSyncedMeta("SetInvisible");

                    game.setEntityAlpha(entity.scriptID, invisible ? 0 : 255, false);
                    game.setEntityInvincible(entity.scriptID, invincible);
                });
            }, 250);
            */
            alt.on('gameEntityCreate', (entity: alt.Entity) => {

                alt.setTimeout(async () => {
                    if (!game.isEntityAPed(entity.scriptID))
                        return;

                    game.clearPedBloodDamage(entity.scriptID);

                    let invincible: boolean = entity.getSyncedMeta("SetInvincible");
                    let invisible: boolean = entity.getSyncedMeta("SetInvisible");
                    let walkingStyle: string = entity.getSyncedMeta("WalkingStyle");
                    let facialAnim: string = entity.getSyncedMeta("FacialAnim");
                    let crounch: boolean = entity.getSyncedMeta("Crounch");

                    game.setEntityAlpha(entity.scriptID, invisible ? 0 : 255, false);
                    game.setEntityInvincible(entity.scriptID, invincible);

                    if (crounch) {
                        await utils.loadMovement("move_ped_crouched");

                        game.setPedMovementClipset(entity.scriptID, "move_ped_crouched", 0.2);
                        game.setPedStrafeClipset(entity.scriptID, "move_ped_crouched_strafing");
                    }

                    if (walkingStyle != null) {

                        await utils.loadMovement(walkingStyle);
                        game.setPedMovementClipset(entity.scriptID, walkingStyle, 0.2);
                    }

                    if (facialAnim != null)
                        game.setFacialIdleAnimOverride(entity.scriptID, facialAnim, undefined);
                }, 250);
            });

            alt.on('syncedMetaChange', async (entity: alt.Entity, key: string, value: any) => {
                if (!game.isEntityAPed(entity.scriptID))
                    return;

                switch (key) {
                    case 'SetInvisible':
                        alt.log("SetInvisible");
                        game.setEntityAlpha(entity.scriptID, value ? 0 : 255, false);
                        break;

                    case 'SetInvincible':
                        alt.log("SetInvincible");
                        game.setEntityInvincible(entity.scriptID, value);
                        break;

                    case 'WalkingStyle':
                        if (value != null) {
                            await utils.loadMovement(value);
                            game.setPedMovementClipset(entity.scriptID, value, 0.2);
                        }
                        else
                            game.resetPedMovementClipset(entity.scriptID, 0.1);
                        break;

                    case 'FacialAnim':
                        game.setFacialIdleAnimOverride(entity.scriptID, value, undefined);
                        break;

                    case 'Crounch':
                        
                        if (value) {
                            await utils.loadMovement("move_ped_crouched");
                            game.setPedMovementClipset(entity.scriptID, "move_ped_crouched", 0.2);
                            game.setPedStrafeClipset(entity.scriptID, "move_ped_crouched_strafing");
                        }
                        else {
                            let walkingStyle: string = entity.getSyncedMeta("WalkingStyle");
                            if (walkingStyle != null) {

                                await utils.loadMovement(walkingStyle);
                                game.setPedMovementClipset(entity.scriptID, walkingStyle, 0.2);
                            }
                            else
                                game.resetPedMovementClipset(entity.scriptID, 0.1);

                        }
                        break;
                }
            });

            alt.log("Player Loaded.");
        }
        catch (ex)
        {
            alt.log(ex);
        }
        
        alt.everyTick(() => {
            
            for (let i = 12; i <= 19; i++)
                game.disableControlAction(2, i, true);


            if (this.DebugInfo && Game.isDebug) {
                ui.DrawText2d("X: " + Math.round(alt.Player.local.pos.x * 1000) / 1000 + " Y: " + Math.round(1000 * alt.Player.local.pos.y) / 1000 + " Z: " + Math.round(1000 * alt.Player.local.pos.z) / 1000, 0.5, 0.08, 0.3, 4, 255, 255, 255, 180, true, true, 99);
                if (alt.Player.local.vehicle != null) {
                    ui.DrawText2d("Essence: " + Math.round(100 * veh.getFuel()) / 100 + "/" + veh.getMaxFuel() + " Consommation: " + Math.round(1000 * veh.getFuelConsumption()) / 1000, 0.5, 0.10, 0.3, 4, 255, 255, 255, 180, true, true, 99);
                }
                //if (ObjectTool.Object != undefined) {
                //    ui.DrawText2d("DEBUG OBJECT X: " + Math.round(ObjectTool.x * 1000) / 1000 + " Y: " + Math.round(1000 * ObjectTool.y) / 1000 + " Z: " + Math.round(1000 * ObjectTool.z) / 1000, 0.5, 0.12, 0.3, 4, 255, 255, 255, 180, true, true, 99);
                //    ui.DrawText2d("DEBUG OBJECT RX: " + Math.round(ObjectTool.rx * 1000) / 1000 + " RY: " + Math.round(1000 * ObjectTool.ry) / 1000 + " RZ: " + Math.round(1000 * ObjectTool.rz) / 1000, 0.5, 0.14, 0.3, 4, 255, 255, 255, 180, true, true, 99);
                //}
            }
            this.disableSeatShuffle();
            this._Time.OnTick();
        });
    }

    public disableSeatShuffle() {
        if (!game.isPedInAnyVehicle(alt.Player.local.scriptID, undefined)) return;
        let vehicle = game.getVehiclePedIsIn(
            alt.Player.local.scriptID,
            undefined
        );

        let passenger = game.getPedInVehicleSeat(vehicle, 0, 0);

        if (!game.getIsTaskActive(passenger, 165)) return;

        if (game.isVehicleSeatFree(vehicle, -1, false)) {
            if (passenger === alt.Player.local.scriptID) {
                game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle, 0);
            }
        }
    }

    public static closeAllMenus() {
        alt.emit("InventoryManager_CloseMenu");
    }
}