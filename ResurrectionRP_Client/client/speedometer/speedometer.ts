import * as alt from 'alt';
import * as game from 'natives';

let player = alt.Player.local;
let fuelMax = 100;
let fuelCur = 100;
let speedoWindow = new alt.WebView('http://resources/resurrectionrp/client/cef/speedometer/speedometer.html');
let lastSent = Date.now();

export function initialize() {
    alt.onServer('OnPlayerLeaveVehicle', hide);
    alt.onServer('OnPlayerEnterVehicle', show);

    alt.on('update', () => {
        if ((Date.now() - lastSent) > 50) {
            lastSent = Date.now();

            if (player.vehicle !== null && speedoWindow !== null) {
                let speed = player.vehicle.speed * 3.6;
                let rpm = player.vehicle.rpm * 591;

                if (rpm >= 591)
                    rpm = 591;

                let lightState = game.getVehicleLightsState(player.vehicle.scriptID, true, true);
                let light = 0;

                if (lightState[1] && lightState[2])
                    light = 2;
                else if (lightState[1] && !lightState[2])
                    light = 1;

                let engineHealth = game.getVehicleEngineHealth(player.vehicle.scriptID);
                speedoWindow.emit('setSpeed', speed, rpm, player.vehicle.gear, light, engineHealth, fuelCur, fuelMax);
            }
        }
    });
}

export function hide() {
    if (speedoWindow !== null) {
        speedoWindow.emit('hideSpeedometer');
    }
}

export function show(vehicle, seat, currentFuel, maxFuel) {
    if (speedoWindow !== null) {
        speedoWindow.emit('showSpeedometer');
    }

    fuelMax = maxFuel;
    fuelCur = currentFuel;
}