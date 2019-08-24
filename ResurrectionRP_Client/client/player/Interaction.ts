import * as alt from 'alt';
import * as game from 'natives';
import Raycast, * as raycast from 'client/Utils/Raycast';
import * as chat from 'client/chat/chat';
import * as Utils from 'client/utils';


export class Interaction {
    constructor() {
        alt.on("keydown", (key) => {
            if (game.isPauseMenuActive() || chat.isOpened())
                return;

            let result = Raycast.line(5, 2, alt.Player.local.scriptID);
/*            alt.logWarning(`Hit Pos: ${JSON.stringify(result.pos)}`);
            alt.log(`Entity hitted: ${result.hitEntity}`);*/
            alt.log(`Entity Type: ${result.entityType}`);
            //alt.log(`Entity Hash: ${result.entityHash}`);
            alt.log(`Key pressed: ${key}`);
            if (key === 69) { // F3 : 114
                if (result.isHit && result.entityType == 2) {
                    alt.emitServer('OpenXtremVehicle');
                }
            }
            else if (key === 85) { // U
                if (result.isHit && result.entityType == 2) {
                    var vehicle = alt.Vehicle.all.find(p => p.scriptID == result.hitEntity);
                    alt.emitServer('LockUnlockVehicle', vehicle);
                }
            }
            else {
                alt.emitServer('OnKeyPress', key);
            }

        });

        alt.on("update", () => {
            let result = Raycast.line(4, 2, alt.Player.local.scriptID);
            
            if (result.isHit && result.entityType == 2) {
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule.", 100)
            }
            let near = Raycast.line(4, 2, alt.Player.local.scriptID);
            if (game.isEntityAPed(near.hitEntity))
                alt.log("Entity type: " + near.entityType);
        });

    }
}