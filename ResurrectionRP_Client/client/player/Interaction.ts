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

            if (key == 69) { // E
                if (isInColshape) {
                    alt.emitServer("InteractionInColshape", key);
                }

                if (game.isAnyObjectNearPoint(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, true) &&
                    game.getClosestObjectOfType(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, game.getHashKey("prop_money_bag_01"), false, true, false) != 0) {
                    alt.emit("interactWithPickableObject", game.getClosestObjectOfType(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, game.getHashKey("prop_money_bag_01"), false, true, false) );

                }

                if (raycastResult.isHit && raycastResult.entityType == 2) {
                    var vehicle: alt.Vehicle = alt.Vehicle.all.find(v => v.scriptID == raycastResult.hitEntity);

                    if (vehicle == null || vehicle == undefined)
                        return;

                    alt.emitServer('OpenXtremVehicle', vehicle.id);
                } else if (raycastResult.isHit && raycastResult.entityType == 1) {
                    var player: alt.Player = alt.Player.all.find(p => p.scriptID == raycastResult.hitEntity);

                    if (player == null || player == undefined)
                        return;

                    alt.emitServer('OpenXtremPlayer', player.id);
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isPompe(raycastResult.entityHash)) {
                    alt.logError(game.getDistanceBetweenCoords(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, raycastResult.pos.x, raycastResult.pos.y, raycastResult.pos.z, false));
                    if (game.getDistanceBetweenCoords(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, raycastResult.pos.x, raycastResult.pos.y, raycastResult.pos.z, false) < 5) {
                        alt.emitServer('OpenGasPumpMenu');
                    }
                    
                }
                else
                    alt.emitServer('OnKeyPress', key);
            }
            else if (key == 85) { // U
                let resultVeh = Raycast.line(5, 2, alt.Player.local.scriptID);

                if (resultVeh.isHit && resultVeh.entityType == 2) {
                    var vehicle: alt.Vehicle = alt.Vehicle.all.find(p => p.scriptID == resultVeh.hitEntity);
                    alt.emitServer('LockUnlockVehicle', vehicle);
                }
            }
            else { // Optimiser ce call ? En envoyant que les clés qui sont succeptibles d'être utilisée pour une interaction
                alt.emitServer('OnKeyPress', key);
            }
        });

        alt.everyTick(() => {

            if (!alt.Player.local.getMeta("IsConnected"))
                return;

            if (game.isEntityDead(alt.Player.local.scriptID, false))
                return;

            raycastResult = Raycast.line(2, -1, alt.Player.local.scriptID);
            if (raycastResult.isHit && raycastResult.entityType == 2 && alt.Player.local.vehicle == null) {
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule", 100)
            } else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isAtm(raycastResult.entityHash)) {
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec l'ATM", 100)
            } else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isPompe(raycastResult.entityHash)) {
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec la pompe à essence", 100)
            }

            if (game.isAnyObjectNearPoint(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, true) &&
                game.getClosestObjectOfType(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, 2, game.getHashKey("prop_money_bag_01"), false, true, false) != 0
            )
                alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour ramasser l'objet", 100);

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
    private static isPompe(entityHash: number): boolean {
        switch (entityHash) {
            case 1339433404:
            case 1933174915:
            case -2007231801:
            case -462817101:
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