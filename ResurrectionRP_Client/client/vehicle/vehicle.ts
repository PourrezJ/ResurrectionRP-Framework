import * as alt from 'alt';
import * as game from 'natives';

let player = alt.Player.local;
let fuelMax = 100;
let fuelCur = 100;
let fuelConsum = 5.5;
var CurrentMilage = 0.0;
let speedoWindow = new alt.WebView('http://resource/client/cef/speedometer/speedometer.html');
let lastSent = Date.now();
let lastPos = null;

export function initialize() {
    alt.onServer('HideSpeedometer', hide2);
    alt.onServer('OnPlayerLeaveVehicle', hide);
    alt.onServer('OnPlayerEnterVehicle', show);
    alt.onServer('UpdateFuel', (fuel: number) => {
        fuelCur = fuel;
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

            if (player.vehicle == null)
                return;
            if (game.getPedInVehicleSeat(player.vehicle.scriptID, -1, player.scriptID)) {
                var newPos = alt.Player.local.pos;
                if (lastPos == null)
                    lastPos = newPos;

                var deltaX = newPos.x - lastPos.x;
                var deltaY = newPos.y - lastPos.y;
                var deltaZ = newPos.z - lastPos.z;
                lastPos = newPos;
                var distance = (Math.sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ) / 1000);

                CurrentMilage += distance;

                if (game.getIsVehicleEngineRunning(player.vehicle.scriptID)) {
                    var SpeedFuel = 0;
                    if (speed > 100) {
                        SpeedFuel = (speed / 100) * 5;
                    } else {
                        SpeedFuel = (-(speed - 50)/100)*5
                    }
                    fuelCur -= (fuelConsum * distance * SpeedFuel) / 100;
                }
            }
        }
    });
}

export function hide(vehicle) {
    if (speedoWindow !== null) {
        speedoWindow.emit('hideSpeedometer');
    }
    alt.emitServer('updateFuelAndMilage', vehicle, fuelCur, CurrentMilage);
}

export function hide2() {
    if (speedoWindow !== null) {
        speedoWindow.emit('hideSpeedometer');
    }
}

export function show(vehicle, seat, currentFuel, maxFuel, milage, fuelconsumption) {
    if (speedoWindow !== null) {
        speedoWindow.emit('showSpeedometer');
    }
    CurrentMilage = milage;
    fuelMax = maxFuel;
    fuelCur = currentFuel;
}