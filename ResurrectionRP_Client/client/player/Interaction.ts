import * as alt from 'alt';
import * as game from 'natives';
import Raycast, * as raycast from 'client/Utils/Raycast';
import * as chat from 'client/chat/chat';
import * as MenuManager from 'client/MenuManager/MenuManager';
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
var IsInColshape: boolean = false;
export class Interaction {
    constructor() {
        alt.onServer("setStateInColShape", (state: boolean) => {
            IsInColshape = state;
        });
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
            if (key == 69 && resultVeh.isHit || key == 69 && resultPed.isHit ) {// E // F3 : 114
                if (resultVeh.isHit && resultVeh.entityType == 2) {
                    alt.emitServer('OpenXtremVehicle');
                }
                else if (resultPed.isHit && resultPed.entityType == 1) {
                    var player: alt.Player = alt.Player.all.find(p => p.scriptID == resultPed.hitEntity);
                    if (player == null || player == undefined)
                        return;
                    alt.emitServer('OpenXtremPlayer', player.id);
                }
            }
            else if (key == 85 && resultVeh.isHit) { // U
                if (resultVeh.isHit && resultVeh.entityType == 2) {
                    var vehicle = alt.Vehicle.all.find(p => p.scriptID == resultVeh.hitEntity);
                    alt.emitServer('LockUnlockVehicle', vehicle);
                }
            } else if (IsInColshape && (key == 85 || key == 69 || key == 85 || key == 89 || key == 78 )) { // U / E / W / Y / N
                alt.emitServer("InteractionInColshape", key);
            }
            else if(key == 85 || key == 69 || key == 85 || key == 89 || key == 78){ // Optimiser ce call ? En envoyant que les clés qui sont succeptibles d'être utilisée pour une interaction
                alt.emitServer('OnKeyPress', key);
            }
        });

        alt.on('update', () => {
            let result = Raycast.line(4, 2, alt.Player.local.scriptID);

            if (result.isHit && result.entityType == 2 && alt.Player.local.vehicle == null) {
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule.", 100)
            }
        });
    }
}