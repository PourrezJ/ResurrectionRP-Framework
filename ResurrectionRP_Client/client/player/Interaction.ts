import * as alt from 'alt';
import * as game from 'natives';
import Raycast, * as raycast from 'client/Utils/Raycast';
import * as chat from 'client/chat/chat';


export class Interaction {
    constructor() {
        alt.on("keydown", (key) => {
            if (game.isPauseMenuActive() || chat.isOpened())
                return;
            if (key === 69) {
                if (alt.Player.local.vehicle != null)
                    return;

                let result = Raycast.line(5, 2, alt.Player.local.scriptID);

                if (result.isHit) {
                    if (!game.isEntityAVehicle(result.entityHash))
                        return;
                    alt.emitServer('OpenXtremVehicle');
                }
            }
        });
    }
}