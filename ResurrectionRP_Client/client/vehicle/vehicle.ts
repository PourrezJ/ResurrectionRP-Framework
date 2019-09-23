import * as alt from 'alt';
import * as game from 'natives';
import * as chat from '../chat/chat';

let player = alt.Player.local;
let fuelMax = 100;
let fuelCur = 100;
let fuelConsum = 5.5;
var CurrentMilage = 0.0;
let speedoWindow = new alt.WebView('http://resource/client/cef/speedometer/speedometer.html');
let lastSent = Date.now();
let lastPos = null;
let playerVehicle: alt.Vehicle = null;
let keepEngineOn: boolean = false;

export function initialize() {
    alt.onServer('HideSpeedometer', hideSpeedometer);
    alt.onServer('OnPlayerLeaveVehicle', hideSpeedometer);
    alt.onServer('OnPlayerEnterVehicle', showSpeedometer);
    alt.onServer('SetDoorState', setDoorState);
    alt.onServer('HornPreview', hornPreview);

    alt.onServer('UpdateFuel', (fuel: number) => {
        fuelCur = fuel;
    });

    alt.onServer('UpdateMilage', (milage: number) => {
        CurrentMilage = milage;
    });

    alt.onServer('keepEngineState', (state: boolean) => {
        keepEngineOn = state;
    })

    alt.onServer('keepEngineState', (state: boolean) => {
        keepEngineOn = state;
    })

    alt.onServer('vehicleFix', (vehicle: alt.Vehicle) => {
        game.setVehicleFixed(vehicle.scriptID);
    })

    alt.on('gameEntityCreate', (entity) => {

        if (game.isEntityAVehicle(entity.scriptID)) {
            try
            {
                if (game.isVehicleSeatFree(alt.Player.local.scriptID, -1, false))
                    game.setVehicleOnGroundProperly(alt.Player.local.scriptID, 5.0);

                alt.setTimeout(() => {

                    let sirenSound: boolean = entity.getSyncedMeta("SirenDisabled");
                    let vehId = entity.scriptID;
                    if (sirenSound) {
                        game.setDisableVehicleSirenSound(vehId, true);
                    }
                    else if (sirenSound) {
                        game.setDisableVehicleSirenSound(vehId, false);
                    }

                    let freezed: boolean = entity.getSyncedMeta("IsFreezed");
                    game.freezeEntityPosition(entity.scriptID, (freezed == null) ? false : freezed);

                    let invincible: boolean = entity.getSyncedMeta("IsInvincible");
                    game.setEntityInvincible(entity.scriptID, (invincible == null) ? false : invincible);

                }, 500); 
            }
            catch (e) {
                alt.log("Error in setting data: " + e);
            }
        }
    });

    alt.on('syncedMetaChange', (entity: alt.Vehicle, key: string, value: any) => {
        switch (key) {
            case "SirenDisabled":
                game.setDisableVehicleSirenSound(entity.scriptID, value);
                break;

            case "IsFreezed":
                game.freezeEntityPosition(entity.scriptID, value);
                break;

            case "IsInvincible":
                game.setEntityInvincible(entity.scriptID, value);
                break;
        }
    });

    alt.everyTick(() => {
        if ((Date.now() - lastSent) > 33) {
            lastSent = Date.now();

            if (player.vehicle === null)
                return;

            let speed = player.vehicle.speed * 3.6;
            let rpm = player.vehicle.rpm * 591;

            if (speedoWindow !== null) {

                if (rpm >= 591)
                    rpm = 591;

                let lightState = game.getVehicleLightsState(player.vehicle.scriptID, true, true);
                let light = 0;

                if (lightState[1] && lightState[2])
                    light = 2;
                else if (lightState[1] && !lightState[2])
                    light = 1;

                let engineHealth = game.getVehicleEngineHealth(player.vehicle.scriptID);
                speedoWindow.emit('setSpeed', speed, rpm, player.vehicle.gear, light, engineHealth, fuelCur, fuelMax, CurrentMilage);
            }
        }
    });
}

export function hideSpeedometer(vehicle = null) {
    if (speedoWindow !== null) {
        speedoWindow.emit('hideSpeedometer');
    }
    game.setVehicleEngineOn(playerVehicle.scriptID, keepEngineOn, true, true);
    if(vehicle != null)
        alt.emitServer('UpdateFuelAndMilage', vehicle, fuelCur, CurrentMilage);
}

export function showSpeedometer(vehicle, seat, currentFuel, maxFuel, milage, fuelconsumption) {
    if (speedoWindow !== null && game.getPedInVehicleSeat(player.vehicle.scriptID, -1, player.scriptID) == player.scriptID) {
        speedoWindow.emit('showSpeedometer');
    }
    keepEngineOn = false;

    playerVehicle = alt.Player.local.vehicle;
    

    CurrentMilage = milage;
    fuelMax = maxFuel;
    fuelCur = currentFuel;
}

export function setDoorState(vehicle: alt.Vehicle, door: number, state: number, option: boolean) {
    if (state == 0) {
        game.setVehicleDoorShut(vehicle.scriptID, door, option);
    } else if (state == 255) {
        game.setVehicleDoorBroken(vehicle.scriptID, door, option);
    } else {
        game.setVehicleDoorOpen(vehicle.scriptID, door, false, option);
    }
}

function hornPreview(vehicle: alt.Vehicle, horn: number, preview: boolean) {
    if (preview) {
        game.setVehicleMod(vehicle.scriptID, 14, horn, false);
        game.startVehicleHorn(vehicle.scriptID, 600000, game.getHashKey("HELDDOWN"), false);
    } else {
        // Hack to stop the horn
        game.setVehicleMod(vehicle.scriptID, 14, -1, false);
        game.setHornEnabled(vehicle.scriptID, false);
        game.startVehicleHorn(vehicle.scriptID, 0, game.getHashKey("NORMAL"), false);
        game.setHornEnabled(vehicle.scriptID, true);
        game.setVehicleMod(vehicle.scriptID, 14, horn, false);
    }
}

export function getFuel(): number { return fuelCur; }
export function getFuelConsumption(): number { return fuelConsum; }
export function getMaxFuel(): number { return fuelMax; }
export function getKm(): number { return CurrentMilage; }