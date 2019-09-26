import * as alt from 'alt';
import * as game from 'natives';
import Raycast, * as raycast from '../Utils/Raycast';
import * as chat from '../chat/chat';
import * as MenuManager from '../MenuManager/MenuManager';
import * as Utils from '../Utils/Utils';
import * as Globals from '../Utils/Globals';
import * as Streamer from '../Streamer/NetworkingEntityClient';

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
var raycastResult = null;
var canClose: boolean = true;

export class Interaction {

    nbColshapes: number = 0;
    private tick: number = 0;

    constructor()
    {
        alt.onServer("SetStateInColShape", (state: boolean) => {
            if (state) {
                this.nbColshapes++;
            } else {
                this.nbColshapes--;
            }
        });

        alt.on('keyup', (key) => {
            if (game.isPauseMenuActive() || chat.isOpened() || MenuManager.hasMenuOpen())
                return;

            alt.emitServer('OnKeyUp', key);
        });

        alt.on("keydown", (key) => {
            if (game.isPauseMenuActive() || chat.isOpened() || MenuManager.hasMenuOpen() && !canClose)
                return;

            switch (key) {
                case 69:    // E
                case 85:    // U
                case 87:    // W
                case 113:   // F2
                case 114:   // F3
                case 115:   // F4
                case 116:   // F5
                case 84:    // T
                case 73:    // I
                case 77:    // M
                case 8:     // BackSpace
                case 71:    // G
                case 33:    // PageUP
                case 34:    // PageDown
                case 38:    // ArrowUP
                case 40:    // ArrowDown
                case 96:    // 0
                case 97:    // 1
                case 98:    // 2
                case 99:    // 3    
                case 100:   // 4
                case 101:   // 5
                case 102:   // 6
                case 103:   // 7
                case 104:   // 8
                case 105:   // 9
                case 20:    // Verr Maj
                case 49:    // &
                case 50:    // é
                case 51:    // "

                    let vehicle: alt.Vehicle = null;
                    let player: alt.Player = null;
                    let objNetId = -1;

                    if (raycastResult.isHit) {
                        Streamer.NetworkingEntityClient.EntityList.forEach((item, index) => {
                            if (item == raycastResult.hitEntity) {
                                objNetId = index;
                            }
                        });
                    }

                    if (raycastResult.isHit && raycastResult.entityType == 1 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                        player = alt.Player.all.find(p => p.scriptID == raycastResult.hitEntity);
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), null, player, objNetId);
                    }
                    else if (raycastResult.isHit && raycastResult.entityType == 2 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                        vehicle = alt.Vehicle.all.find(v => v.scriptID == raycastResult.hitEntity);
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), vehicle, null, objNetId);
                    }
                    else if (raycastResult.isHit && raycastResult.entityType == 3 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), null, null, objNetId);
                    }
                    else {
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), null, null, objNetId);
                    }

                    if (key == 69 && this.isInColshape()) {
                        alt.emitServer('InteractionInColshape', key, JSON.stringify(raycastResult));
                    }
                    
                    break;
            }
        });

        alt.everyTick(() =>
        {
            this.tick++;
            if (this.tick % 20) {
                if (!alt.Player.local.getMeta("IsConnected"))
                    return;

                if (game.isEntityDead(alt.Player.local.scriptID, false))
                    return;

                var _pos = game.getGameplayCamCoord();
                var _dir: any = Utils.GetCameraDirection();

                // Origin is camera position, not player position, so need to set higher values when player has its camera far for character
                var _farAway = new alt.Vector3(
                    _pos.x + (_dir.x * 9),
                    _pos.y + (_dir.y * 9),
                    _pos.z + (_dir.z * 9),
                )

                raycastResult = Raycast.raycastRayFromTo(_pos, _farAway, alt.Player.local.scriptID, 255);

                if (raycastResult.isHit && raycastResult.entityType == 2 && alt.Player.local.vehicle == null && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule");
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isAtm(raycastResult.entityHash) && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec l'ATM");
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isPompe(raycastResult.entityHash) && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec la pompe à essence");
                }
                else if (game.isAnyObjectNearPoint(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, Globals.MAX_INTERACTION_DISTANCE, true) && game.getClosestObjectOfType(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, Globals.MAX_INTERACTION_DISTANCE, game.getHashKey("prop_money_bag_01"), false, true, false) != 0) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour ramasser l'objet");
                }
            }
        });
    }

    private isInColshape() {
        return this.nbColshapes > 0;
    }

    private static displayHelp(text: string) {
        game.beginTextCommandDisplayHelp('STRING');
        game.addTextComponentSubstringPlayerName(text);
        game.endTextCommandDisplayHelp(0, false, true, 5000);
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

    private static isPompe(entityHash: number): boolean {
        switch (entityHash) {
            case 1339433404:
            case 1933174915:
            case -2007231801:
            case -462817101:
            case -469694731:
            case 1694452750:
            case 1694:
            case 750:
                return true;
        }
        return false;
    }

    public static isPickable(entityHash: number): boolean {
        if (game.objectValueGetString(entityHash, "pickup")[0] == "")
            return false;
        return true;
    }

    public static GetCanClose(): boolean {   
        return canClose;
    }

    public static SetCanClose(close: boolean) {
        alt.Player.local.setMeta("CanClose", close);
        canClose = close;
    }
}

export function getRaycastResult() {
    return raycastResult;
}