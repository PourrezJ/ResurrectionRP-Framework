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

export class Interaction {
    constructor() {
        alt.on('keydown', (key) => {
            if (game.isPauseMenuActive() || chat.isOpened() || MenuManager.hasMenuOpen())
                return;

/*        alt.logWarning(`Hit Pos: ${JSON.stringify(result.pos)}`);
        alt.log(`Entity hit: ${result.hitEntity}`);
        alt.log(`Entity Type: ${result.entityType}`);
        alt.log(`Entity Hash: ${result.entityHash}`);
        alt.log(`Key pressed: ${key}`);*/
            if (key == 69) { // E // F3 : 114
                let result = Raycast.line(5, 22, alt.Player.local.scriptID);

                if (result.isHit && result.entityType == 2) {
                    alt.emitServer('OpenXtremVehicle');
                } else if (result.isHit && result.entityType == 1) {
                    alt.emitServer('OpenXtremPlayer');
                } else if (result.isHit && result.entityType == 3) {
                    if (Interaction.isAtm(result.entityHash)) {
                        alt.emitServer('OpenAtmMenu');
                    }
                }
            }
            else if (key == 85) { // U
                let resultVeh = Raycast.line(5, 2, alt.Player.local.scriptID);

                if (resultVeh.isHit && resultVeh.entityType == 2) {
                    var vehicle = alt.Vehicle.all.find(p => p.scriptID == resultVeh.hitEntity);
                    alt.emitServer('LockUnlockVehicle', vehicle);
                }
            }
            else {
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

    private static isAtm(entityHash: number): boolean {
        switch (entityHash) {
            case 3424098598:
            case 506770882:
            case 2930269768:
            case 3168729781:
                return true;
        }

        return false;
    }
}