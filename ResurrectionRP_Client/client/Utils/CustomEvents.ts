import * as alt from 'alt';
import * as game from 'natives';
import * as chat from '../chat/chat'

let player = alt.Player.local;
let vehicle: alt.Vehicle = null;
let playerVehicleStatus: number = 0;

export function initialize() {
    alt.everyTick(() => {
        if (player.vehicle != null) {
            if (player.vehicle != vehicle) {
                vehicle = player.vehicle;
                playerVehicleStatus = 0;
            }

            if (playerVehicleStatus == 0 && game.isPedInAnyVehicle(player.scriptID, true)) {
                alt.emit('onPlayerEnterVehicle', player.vehicle, game.getSeatPedIsTryingToEnter(player.scriptID));
                playerVehicleStatus = 1;
            }

            const sitting = game.isPedSittingInAnyVehicle(player.scriptID);

            if (sitting && playerVehicleStatus == 1) {
                playerVehicleStatus = 2;
            } else if (!sitting && playerVehicleStatus == 2) {
                playerVehicleStatus = 3;
                alt.emit('onPlayerLeaveVehicle', player.vehicle, getPlayerSeat());
            }
        }
        else if (player.vehicle == null) {
            if (playerVehicleStatus == 2) {
                playerVehicleStatus = 0;
                vehicle = null;
                alt.emit('onPlayerLeaveVehicle', null, -2);
            } else if (playerVehicleStatus == 3) {
                vehicle = null;
                playerVehicleStatus = 0;
            }
        }
    });
}

function getPlayerSeat() {
    let nbSeats = game.getVehicleModelNumberOfSeats(vehicle.model);
    let seat = -2;

    for (let i = -1; i < nbSeats - 1; i++) {
        let ped = game.getPedInVehicleSeat(vehicle.scriptID, i, i);

        if (ped == player.scriptID) {
            seat = i;
            break;
        }
    }

    return seat;
}

