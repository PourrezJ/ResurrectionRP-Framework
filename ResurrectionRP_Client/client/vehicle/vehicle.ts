import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from '../chat/chat';

let player = alt.Player.local;
let fuelMax = 100;
let fuelCur = 100;
let fuelConsum = 5.5;
var CurrentMilage = 0.0;
let speedoWindow = new alt.WebView('http://resource/client/cef/speedometer/speedometer.html', true);
let lastSent = Date.now();
let keepEngineOn: boolean = true;
let enginePreviousState: boolean;
let engineState: boolean;
let hasTrailer: boolean = false;
let Trailer: number = null;
let speedoState: boolean = false;
let hudHidden: boolean = false;

export function initialize() {
    alt.onServer('OnPlayerEnterVehicle', onPlayerEnterVehicle);
    alt.onServer('SetDoorState', setDoorState);
    alt.onServer('HornPreview', hornPreview);
    alt.onServer("DetachTrailer", () => {
        if (hasTrailer)
            game.detachVehicleFromTrailer(player.vehicle.scriptID);
    });

    alt.onServer('UpdateFuel', (fuel: number) => {
        fuelCur = fuel;
    });

    alt.onServer('UpdateMilage', (milage: number) => {
        CurrentMilage = milage;
    });

    alt.onServer('vehicleFix', (vehicle: alt.Vehicle) => {
        game.setVehicleFixed(vehicle.scriptID);
    });

    alt.onServer('windowState', (window: number) => {
        chat.pushMessage('[Client] IsWindowDamaged: ' + !game.isVehicleWindowIntact(alt.Player.local.vehicle.scriptID, window));
    });

    alt.onServer('smashWindow', (window: number) => {
        game.smashVehicleWindow(alt.Player.local.vehicle.scriptID, window);
    });

    alt.onServer('fixWindow', (window: number) => {
        game.fixVehicleWindow(alt.Player.local.vehicle.scriptID, window);
    });

    alt.onServer('countVehicles', (window: number) => {
        chat.pushMessage('Vehicles: ' + alt.Vehicle.all.length);
    });

    alt.on('gameEntityCreate', (entity: alt.Entity) => {

        try {
            if (game.isEntityAVehicle(entity.scriptID))
            {
                let vehId = entity.scriptID;

                //if (game.isVehicleSeatFree(vehId, -1, false))
                //    game.setVehicleOnGroundProperly(vehId, 0);

                game.setVehicleAsNoLongerNeeded(vehId);
                game.setEntityAsMissionEntity(vehId, true, true);

                alt.setTimeout(() => {
                    let sirenSound: boolean = entity.getSyncedMeta("SirenDisabled");

                    game.setVehicleHasMutedSirens(vehId, (sirenSound == null) ? false : sirenSound)

                    let freezed: boolean = entity.getSyncedMeta("IsFreezed");
                    game.freezeEntityPosition(vehId, (freezed == null) ? false : freezed);

                    let invincible: boolean = entity.getSyncedMeta("IsInvincible");
                    game.setEntityInvincible(vehId, (invincible == null) ? false : invincible);

                    let neonState: boolean = entity.getSyncedMeta("NeonState");

                    for (let i = 0; i < 4; i++) {
                        game.setVehicleNeonLightEnabled(vehId, i, neonState);
                    }

                    let neonColor: number = entity.getSyncedMeta("NeonColor");
                    const b = (neonColor & 0xFF);
                    const g = (neonColor & 0xFF00) >>> 8;
                    const r = (neonColor & 0xFF0000) >>> 16;
                    game.setVehicleNeonLightsColour(vehId, r, g, b);

                    let torque: number = entity.getSyncedMeta("torqueMultiplicator");
                    game.setVehicleCheatPowerIncrease(vehId, torque);

                    let power: number = entity.getSyncedMeta("powerMultiplicator");
                    game.setVehicleCheatPowerIncrease(vehId, power);

                    if (entity.getSyncedMeta("CustomHandling") != null)
                    {
                        alt.log("vehicle has custom handling");
                        let customHandling: any = JSON.parse(entity.getSyncedMeta("CustomHandling"));
                        let handling = alt.HandlingData.getForModel(entity.model);

                        for (const key in handlingproperties) {
                            const property = handlingproperties[key];

                            alt.log(property + ": " + customHandling[property])

                            if (property === 'centreOfMassOffset' || property === 'inertiaMultiplier') {
                                handling[property] = new alt.Vector3(0,0,0);

                            } else if (property === 'initialDriveGears' || property === 'modelFlags' || property === 'monetaryValue' || property === 'modelFlags' || property === 'handlingFlags' || property === 'damageFlags') {
                                handling[property] = parseInt(customHandling[property]);
                            } else {
                                handling[property] = parseFloat(customHandling[property]);
                            }
                        }
                    }

                }, 2500);
            }
        }
        catch (e) {
            alt.log("Error in setting data: " + e);
        }
    });

    alt.on('syncedMetaChange', (entity: alt.Entity, key: string, value: any) => {
        if (game.isEntityAVehicle(entity.scriptID)) {
            switch (key) {
                case 'SirenDisabled':
                    game.setVehicleHasMutedSirens(entity.scriptID, value);
                    break;

                case 'IsFreezed':
                    game.freezeEntityPosition(entity.scriptID, value);
                    break;

                case 'IsInvincible':
                    game.setEntityInvincible(entity.scriptID, value);
                    break;

                case 'NeonState':
                    for (let i = 0; i < 4; i++) {
                        game.setVehicleNeonLightEnabled(entity.scriptID, i, value);
                    }
                    break;

                case 'NeonColor':
                    const b = (value & 0xFF);
                    const g = (value & 0xFF00) >>> 8;
                    const r = (value & 0xFF0000) >>> 16;
                    game.setVehicleNeonLightsColour(entity.scriptID, r, g, b);
                    break;

                case 'torqueMultiplicator':
                    game.setVehicleCheatPowerIncrease(entity.scriptID, value);
                    break;

                case 'powerMultiplicator':
                    game.setVehicleCheatPowerIncrease(entity.scriptID, value);
                    break;
                /*
                case 'CustomHandling':
                    let customHandling: alt.HandlingData = JSON.parse(entity.getSyncedMeta("CustomHandling"));

                    let handling = alt.HandlingData.getForModel(entity.model);

                    handlingproperties.forEach((properties: string) => {
                        handling[properties] = customHandling[properties];
                    });
                    break;*/
            }
        }
    });

    alt.on('onPlayerLeaveVehicle', (vehicle: alt.Vehicle, seat: number) => {
        showSpeedometer(false);

        if (vehicle != null && seat == -1 && enginePreviousState) {
            game.setVehicleEngineOn(vehicle.scriptID, keepEngineOn, true, true);
        }
    });

    alt.onServer("GetHandling", (model: number) => {
        let handling: any = alt.HandlingData.getForModel(model);

        let data = {};

        for (const key in handlingproperties) {
            const property = handlingproperties[key];

            if (property === 'centreOfMassOffset' || property === 'inertiaMultiplier') {
                const value: alt.Vector3 = handling[property];

                data[property] = value;

            } else if (property === 'initialDriveGears' || property === 'modelFlags' || property === 'monetaryValue' || property === 'modelFlags' || property === 'handlingFlags' || property === 'damageFlags') {
                data[property] = parseInt(handling[property]);
            } else {
                data[property] = parseFloat(handling[property]).toFixed(6);
            }
        }

        alt.emitServer("CallbackGetHandling", model.toString(), JSON.stringify(data));
    });

    alt.everyTick(() => {
        if (player.vehicle != null ) {
            enginePreviousState = engineState;
            engineState = game.getIsVehicleEngineRunning(player.vehicle.scriptID);

            if (game.isThisModelABicycle(player.vehicle.model))
                game.setVehicleEngineOn(player.vehicle.scriptID, true, true, false);

            if (game.isVehicleAttachedToTrailer(player.vehicle.scriptID) != hasTrailer) {
                hasTrailer = game.isVehicleAttachedToTrailer(player.vehicle.scriptID)
                Trailer = game.getVehicleTrailerVehicle(player.vehicle.scriptID, 0)[1];
                alt.emitServer("UpdateTrailer", player.vehicle, hasTrailer, (Trailer > 0) ? alt.Vehicle.all.find(p => p.scriptID == Trailer) : null);
            }
        }

        if ((Date.now() - lastSent) > 33) {
            lastSent = Date.now();

            if (player.vehicle == null)
                return;

            let speed = game.getEntitySpeed(player.vehicle.scriptID) * 3.6;
            let rpm = player.vehicle.rpm * 591;

            if (speedoWindow != null) {
                if (rpm >= 591)
                    rpm = 591;

                let lightState = game.getVehicleLightsState(player.vehicle.scriptID, true, true);
                let light = 0;

                if (lightState[1] && lightState[2])
                    light = 2;
                else if (lightState[1] && !lightState[2])
                    light = 1;

                let engineHealth = game.getVehicleEngineHealth(player.vehicle.scriptID);
                let engineOn = game.getIsVehicleEngineRunning(player.vehicle.scriptID);
                speedoWindow.emit('setSpeed', speed, rpm, player.vehicle.gear, light, engineOn, engineHealth, fuelCur, fuelMax, CurrentMilage);
            }
        }
    });
}

export function onPlayerEnterVehicle(vehicle: alt.Vehicle, seat: number, currentFuel: number, maxFuel: number, milage: number, fuelconsumption: number) {
    CurrentMilage = milage;
    fuelMax = maxFuel;
    fuelCur = currentFuel;
    fuelConsum = fuelconsumption;

    if (seat == 1) {
        showSpeedometer(true);
    }
}

export function showSpeedometer(show: boolean) {
    speedoState = show;

    if (show && speedoWindow !== null && !hudHidden) {
        speedoWindow.emit('showSpeedometer');
    } else if (!show && speedoWindow !== null || hudHidden) {
        speedoWindow.emit('hideSpeedometer');
    }
}

export function hideHud(hide: boolean) {
    hudHidden = hide;
    showSpeedometer(speedoState);
}

export function setDoorState(vehicle: alt.Vehicle, door: number, state: number, option: boolean) {
    switch (state) {
        case 0:
            game.setVehicleDoorShut(vehicle.scriptID, door, option);
            break;
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
        case 6:
        case 7:
            game.setVehicleDoorOpen(vehicle.scriptID, door, false, option);
            break;      
        case 255:
            game.setVehicleDoorBroken(vehicle.scriptID, door, option);
            break;
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

function PlayerStartsLeavingVehicle() {
}

export function getFuel(): number { return fuelCur; }
export function getFuelConsumption(): number { return fuelConsum; }
export function getMaxFuel(): number { return fuelMax; }
export function getKm(): number { return CurrentMilage; }

const handlingproperties = [
    'mass',
    'initialDragCoeff',
    'percentSubmerged',
    'centreOfMassOffset',
    'inertiaMultiplier',
    'driveBiasFront',
    'initialDriveGears',
    'initialDriveForce',
    'driveInertia',
    'clutchChangeRateScaleUpShift',
    'clutchChangeRateScaleDownShift',
    'initialDriveMaxFlatVel',
    'breakForce',
    'brakeBiasFront',
    'handBrakeForce',
    'steeringLock',
    'tractionCurveMax',
    'tractionCurveMin',
    'tractionCurveLateral',
    'tractionSpringDeltaMax',
    'lowSpeedTractionLossMult',
    'camberStiffnesss',
    'tractionBiasFront',
    'tractionLossMult',
    'suspensionForce',
    'suspensionCompDamp',
    'suspensionReboundDamp',
    'suspensionUpperLimit',
    'suspensionLowerLimit',
    'suspensionRaise',
    'suspensionBiasFront',
    'antiRollBarForce',
    'antiRollBarBiasFront',
    'rollCentreHeightFront',
    'rollCentreHeightRear',
    'collisionDamageMult',
    'weaponDamageMult',
    'deformationDamageMult',
    'engineDamageMult',
    'petrolTankVolume',
    'oilVolume',
    'seatOffsetDistX',
    'seatOffsetDistY',
    'seatOffsetDistZ',
    'monetaryValue',
    'modelFlags',
    'handlingFlags',
    'damageFlags'
];
