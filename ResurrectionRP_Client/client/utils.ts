import * as alt from 'alt';
import * as game from 'natives';

export function initialize() {
    alt.onServer('SetPlayerIntoVehicle', (vehicle, seat) => {
        alt.setTimeout(() => {
            game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, seat);
        }, 20);
    });
}
