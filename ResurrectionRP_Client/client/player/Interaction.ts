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
var isInColshape: boolean = false;
var raycastResult = null;
var canClose: boolean;

export class Interaction {

    private tick: number = 0;
    constructor() {
        alt.onServer("SetStateInColShape", (state: boolean) => {
            isInColshape = state;
        });

        alt.on('CanClose', (status) => {
            canClose = status;
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

                    if (key == 69 && isInColshape)
                        alt.emitServer("InteractionInColshape", key);

                    let vehicle: alt.Vehicle = null;
                    let player: alt.Player = null;

                    if (raycastResult.isHit && raycastResult.entityType == 2) {
                        vehicle = alt.Vehicle.all.find(v => v.scriptID == raycastResult.hitEntity);
                    }
                    else if (raycastResult.isHit && raycastResult.entityType == 1) {
                         player  = alt.Player.all.find(p => p.scriptID == raycastResult.hitEntity);
                    }

                    alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), vehicle, player);
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

                var _farAway = new alt.Vector3(
                    _pos.x + (_dir.x * 5),
                    _pos.y + (_dir.y * 5),
                    _pos.z + (_dir.z * 5),
                )

                raycastResult = Raycast.raycastRayFromTo(_pos, _farAway, alt.Player.local.scriptID, -1);

                if (raycastResult.isHit && raycastResult.entityType == 2 && alt.Player.local.vehicle == null) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule");
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isAtm(raycastResult.entityHash))
                {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec l'ATM");
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isPompe(raycastResult.entityHash)) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec la pompe à essence");
                }

                if (game.isAnyObjectNearPoint(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, true) &&
                    game.getClosestObjectOfType(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, game.getHashKey("prop_money_bag_01"), false, true, false) != 0
                )
                Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour ramasser l'objet");
            }
        });
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
}