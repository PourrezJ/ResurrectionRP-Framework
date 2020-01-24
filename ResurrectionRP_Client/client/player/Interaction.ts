import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from '../chat/chat';
import * as MenuManager from '../MenuManager/MenuManager';
import * as Utils from '../Utils/Utils';
import * as Globals from '../Utils/Globals';
import * as Streamer from '../Streamer/NetworkingEntityClient';
import Ragdoll from '../Ragdoll';
import { EmergencyCall } from '../EmergencyCall';
import { Raycast, RaycastResultInterface } from '../Utils/Raycast';

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
const ragdoll = new Ragdoll();

var raycastResult = null;
var canClose: boolean = true;

export class Interaction {

    colshapes: number[] = [];
    private tick: number = 0;

    constructor()
    {
        alt.onServer('OnPlayerEnterColshape', (colshapeId: number) => {
            this.colshapes.unshift(colshapeId);
            alt.log('OnPlayerEnterColshape, colshapes: ' + this.colshapes.length);
        });

        alt.onServer('OnPlayerLeaveColshape', (colshapeId: number) => {
            this.colshapes = this.colshapes.filter(item => item != colshapeId);
            alt.log('OnPlayerLeaveColshape, colshapes: ' + this.colshapes.length);
        });

        alt.on('canClose', (close) => {
            canClose = close;
        });

        alt.on('keyup', (key) => {
            if (game.isPauseMenuActive() || chat.isOpened() || MenuManager.hasMenuOpen())
                return;

            alt.emitServer('OnKeyUp', key);

            if (key === 82) {
                ragdoll.stop();
            }
        });

        alt.on("keydown", (key) => {
            if (game.isPauseMenuActive() || chat.isOpened() || MenuManager.hasMenuOpen() && !canClose)
                return;

            switch (key) {
                case 88:    // X
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

                    if (raycastResult == null)
                        return;
                    if (raycastResult.isHit) {
                        Streamer.NetworkingEntityClient.EntityList.forEach((item, index) => {
                            if (item == raycastResult.hitEntity) {
                                objNetId = index;
                            }
                        });

                        //let item = game.getClosestObjectOfType(raycastResult.entityPos.x, raycastResult.entityPos.y, raycastResult.entityPos.z, Globals.MAX_INTERACTION_DISTANCE, raycastResult.hitEntity, false, false, false)

                        raycastResult["entityPos"] = game.getEntityCoords(raycastResult.hitEntity, true);
                        raycastResult["entityHeading"] = game.getEntityHeading(raycastResult.hitEntity);
                    }

                    if (raycastResult.isHit && raycastResult.entityType == 1 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                        player = alt.Player.all.find(p => p.scriptID == raycastResult.hitEntity);
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), null, player, objNetId, game.getEntityHeading(raycastResult.hitEntity));
                    }
                    else if (raycastResult.isHit && raycastResult.entityType == 2 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                        vehicle = alt.Vehicle.all.find(v => v.scriptID == raycastResult.hitEntity);
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), vehicle, null, objNetId, game.getEntityHeading(raycastResult.hitEntity));
                    }
                    else if (raycastResult.isHit && raycastResult.entityType == 3 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), null, null, objNetId, game.getEntityHeading(raycastResult.hitEntity));
                    }
                    else {
                        alt.emitServer('OnKeyPress', key, JSON.stringify(raycastResult), null, null, objNetId, game.getEntityHeading(raycastResult.hitEntity));
                    }
                    break;

                case 82: // R
                    if (alt.Player.local.getMeta("CanClose") == false)
                        return;
                    ragdoll.start();
                    break;
                case 89:
                    {   
                        if (EmergencyCall.IsAnyMissionAvailable || EmergencyCall.IsInMission) {
                            alt.emitServer("InteractEmergencyCall", "openMenu", "", "");
                            alt.log("Call emergency: " + EmergencyCall.IsAnyMissionAvailable + " " + EmergencyCall.IsInMission);
                        }         
                    }
            }
        });

        alt.everyTick(() =>
        {
            this.tick++;
            if (this.tick % 20) {
                if (!alt.Player.local.getMeta("IsConnected"))
                    return;

                if (!game.hasStreamedTextureDictLoaded("srange_gen"))
                    game.requestStreamedTextureDict("srange_gen", true);

                if (game.isEntityDead(alt.Player.local.scriptID, false))
                    return;

                if (game.isPedSittingInAnyVehicle(alt.Player.local.scriptID))
                    return;

                const _pos = game.getGameplayCamCoord();
                const _dir: alt.Vector3 = Utils.GetCameraDirection();

                // Origin is camera position, not player position, so need to set higher values when player has its camera far for character
                const _farAway = new alt.Vector3(
                    _pos.x + (_dir.x * 9),
                    _pos.y + (_dir.y * 9),
                    _pos.z + (_dir.z * 9),
                )

                raycastResult = Raycast.castCapsule(_pos, _farAway, alt.Player.local.scriptID, 255, 1);

                if (raycastResult == null)
                    return;

                if (raycastResult.entityHash == 0)
                    raycastResult = Raycast.raycastRayFromTo(_pos, _farAway, alt.Player.local.scriptID, 255);

                if (raycastResult == null)
                    return;

                //if (raycastResult.entityHash != null)
                //    ui.DrawText2d(raycastResult.entityHash.toString() + " " + raycastResult.isHit.toString(), 0.5, 0.5, 0.5, 4, 255, 255, 255, 255, true, true, 99);

                if (raycastResult.isHit && raycastResult.entityType == 2 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le véhicule");
                    game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 0, 130, 0, 255, false);     
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isAtm(raycastResult.entityHash) && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec l'ATM");
                    game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 0, 130, 0, 255, false);
                }
                else if (raycastResult.isHit && raycastResult.entityType == 3 && Interaction.isPompe(raycastResult.entityHash) && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec la pompe à essence");
                    game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 0, 130, 0, 255, false);
                }
                else if (game.isAnyObjectNearPoint(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, Globals.MAX_INTERACTION_DISTANCE, true) && game.getClosestObjectOfType(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, Globals.MAX_INTERACTION_DISTANCE, game.getHashKey("prop_money_bag_01"), false, true, false) != 0) {
                    Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour ramasser l'objet");
                    game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 0, 130, 0, 255, false);   
                }
                else if (raycastResult.isHit && raycastResult.entityType == 1 && Utils.Distance(alt.Player.local.pos, raycastResult.pos) <= Globals.MAX_INTERACTION_DISTANCE) {
                    if (!Utils.isPlayer(raycastResult.hitEntity))
                        Interaction.displayHelp("Appuyez sur ~INPUT_CONTEXT~ pour intéragir avec le Ped");
                    game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 0, 130, 0, 255, false);
                }
                else
                    game.drawSprite("srange_gen", "hits_dot", 0.5, 0.5, 0.005, 0.007, 0, 255, 255, 255, 60, false);
            }
        });
    }

    private isInColshape() {
        return this.colshapes.length > 0;
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