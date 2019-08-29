import * as alt from 'alt';
import * as game from 'natives';
import Raycast, * as raycast from 'client/Utils/Raycast';
import * as chat from 'client/chat/chat';
import * as Utils from 'client/Utils/utils';

/*
 * POUR LE RAY CAST LES FLAGS
     * 1: Intersect with map
    2: Intersect with vehicles (used to be mission entities?) (includes train)
    4: Intersect with peds? (same as 8)
    8: Intersect with peds? (same as 4)
    16: Intersect with objects
    32: Unknown
    64: Unknown
    128: Unknown
    256: Intersect with vegetation (plants, coral. trees not included)

 * */

export class Interaction {
    constructor() {
        alt.on("keydown", (key) => {
            if (game.isPauseMenuActive() || chat.isOpened())
                return;

            let resultVeh = Raycast.line(5, 2, alt.Player.local.scriptID);
            let resultPed = Raycast.line(5, 4, alt.Player.local.scriptID);
/*        alt.logWarning(`Hit Pos: ${JSON.stringify(result.pos)}`);
        alt.log(`Entity hitted: ${result.hitEntity}`);
        alt.log(`Entity Type: ${result.entityType}`);
        alt.log(`Entity Hash: ${result.entityHash}`);
        alt.log(`Key pressed: ${key}`);*/
            if (key == 69) {// E // F3 : 114
                if (resultVeh.isHit && resultVeh.entityType == 2) {
                    alt.emitServer('OpenXtremVehicle');
                }
                if (resultPed.isHit && resultPed.entityType == 1) {
                    var player:alt.Player = alt.Player.all.find(p => p.scriptID == resultPed.hitEntity);
                    if (player == null || player == undefined)
                        return;
                    alt.emitServer('OpenXtremPlayer', player.id);
                }
            }
            else if (key == 85) { // U
                if (resultVeh.isHit && resultVeh.entityType == 2) {
                    var vehicle = alt.Vehicle.all.find(p => p.scriptID == resultVeh.hitEntity);
                    alt.emitServer('LockUnlockVehicle', vehicle);
                }
            }
            else {
                alt.emitServer('OnKeyPress', key);
            }

        });

        alt.on("update", () => {
            let result = Raycast.line(4, 2, alt.Player.local.scriptID);

            if (result.isHit && result.entityType == 2 && alt.Player.local.vehicle == null) {
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule.", 100)
            }
        });

    }
}