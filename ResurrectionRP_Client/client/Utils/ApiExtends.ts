
import * as alt from 'alt-client';
import * as game from 'natives';
import * as util from './Utils';
import { Loading } from '../Env/Loading';
import { HelpText } from '../Env/HelpText';
import { Subtitle } from '../Env/Subtitle';


export function initialize()
{
    alt.onServer('TaskStartScenarioAtPosition', (scenarioName: string, x: number, y: number, z: number, heading: number, duration: number, sittingScenario: boolean, teleport: boolean) => {
        alt.log(`${x} ${y} ${z} ${heading}`);
        game.taskStartScenarioAtPosition(alt.Player.local.scriptID, scenarioName, x, y, z, heading, duration, sittingScenario, teleport);
    });

    alt.onServer('TaskAdvancedPlayAnimation', (animDict: string, animName: string, posX: number, posY: number, posZ: number, rotX: number, rotY: number, rotZ: number, speed: number, speedMultiplier: number, duration: number, flag: number, animTime: number) => {
        alt.log("TaskAdvancedPlayAnimation");
        game.taskPlayAnimAdvanced(alt.Player.local.scriptID, animDict, animName, posX, posY, posZ, rotX, rotY, rotZ, speed, speedMultiplier, duration, flag, animTime, 15000, 15000);
    });
    alt.onServer('StopAnimation', () => {
        game.clearPedTasks(alt.Player.local.scriptID);
        game.clearPedSecondaryTask(alt.Player.local.scriptID);
    });

    alt.onServer('SetWaypoint', (posx: number, posy: number, override: boolean) => {
        if (game.isWaypointActive() && override)
            game.setWaypointOff();
        if (override && !game.isWaypointActive())
            game.setNewWaypoint(posx, posy);
    });

    alt.onServer('DeleteWaypoint', () => {
        game.setWaypointOff();
    })

    alt.onServer("RequestCollisionAtCoords", (x: number, y: number, z: number) => {
        game.requestCollisionAtCoord(x, y, z);
    });

    alt.onServer('ShowNotification', (imageName, headerMsg, detailsMsg, message) => {
        game.beginTextCommandThefeedPost('STRING');
        game.addTextComponentSubstringPlayerName(message);
        game.endTextCommandThefeedPostMessagetextWithCrewTag(imageName.toUpperCase(), imageName.toUpperCase(), false, 4, headerMsg, detailsMsg, 1.0, '');
        game.endTextCommandThefeedPostTicker(false, false);
    });


    alt.onServer('SetPlayerIntoVehicle', (vehicle, seat) => {
        let timeout = alt.setTimeout(() => {
            alt.clearInterval(interval);
        }, 5000);

        let interval = alt.setInterval(() => {
            if (alt.Player.local.vehicle == null) {
                game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, seat);
            } else {
                alt.clearInterval(interval);
                alt.clearTimeout(timeout);
            }
        }, 20);
    });

    alt.onServer('SetPlayerOutOfVehicle', (force: boolean) => {
        game.taskLeaveVehicle(alt.Player.local.scriptID, alt.Player.local.vehicle.scriptID, force ? 16 : 0);
    });

    alt.onServer('TrySetPlayerIntoVehicle', (vehicle: alt.Vehicle) => {
        var success: boolean = false;
        var seat: number = game.getVehicleModelNumberOfSeats(vehicle.model);

        for (var i = seat - 2; i > -1; i--) {
            alt.log("Nombre de siège: " + seat);
            alt.log("Vérificatin actuelle: " + i);

            if (game.isVehicleSeatFree(vehicle.scriptID, i, false) && success == false) {
                game.taskEnterVehicle(alt.Player.local.scriptID, vehicle.scriptID, 10000, i, 1, 1, 0);
                success = true;
            }
        }
        if (!success) {

            alt.emit('alertNotify', "Erreur", "Aucun siège dans le véhicule disponible! ", 5000);
        }

        alt.setTimeout(() => {
            game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, seat);
        }, 20);
    });

    alt.onServer('toggleControl', (state: boolean) => {
        alt.toggleGameControls(state);
    });

    alt.onServer('Display_Help', (text, time) => {
        new HelpText(text, time);
    });

    alt.on('Display_Help', (text, time) => {
        new HelpText(text, time);
    });

    alt.onServer('Display_subtitle', (text, time) => {
        new Subtitle(text, time);
    });

    alt.on('Display_subtitle', (text, time) => {
        new Subtitle(text, time);
    });

    alt.onServer('ShowLoading', (text, time, type, toggled) => {
        new Loading(text, time, type, toggled);
    });

    alt.onServer('ShowCursor', (state: boolean) => {
        alt.showCursor(state);
    });

    alt.onServer('CreateBlip', (sprite: number, position: alt.Vector3, name: string, scale: number, color: number, alpha: number, shortRange: boolean) => {
        util.createBlip(position, sprite, color, name, alpha, scale, shortRange);
    });

    alt.onServer('setEntityHeading', (entity: alt.Entity, angle: number) => {
        game.setEntityHeading(entity.scriptID, angle);
    });

    alt.onServer('ShowNotification', (text) => {
        game.beginTextCommandThefeedPost("STRING");
        game.addTextComponentSubstringPlayerName(text);
        game.endTextCommandThefeedPostTicker(true, false);
    });

    alt.on('SET_NOTIFICATION_BACKGROUND_COLOR', (args: any[]) => game.thefeedSetNextPostBackgroundColor(parseInt(args[0])))

    alt.onServer("SetNotificationMessage", (img, sender, subject, message) => {
        game.beginTextCommandThefeedPost("STRING");
        game.addTextComponentSubstringPlayerName(message);
        game.endTextCommandThefeedPostMessagetext(img.toUpperCase(), img.toUpperCase(), false, 4, sender, subject);
        game.endTextCommandThefeedPostTicker(false, false);
    });

    alt.on("SetNotificationMessage", (img, sender, subject, message) => {
        game.beginTextCommandThefeedPost("STRING");
        game.addTextComponentSubstringPlayerName(message);
        game.endTextCommandThefeedPostMessagetext(img.toUpperCase(), img.toUpperCase(), false, 4, sender, subject);
        game.endTextCommandThefeedPostTicker(false, false);
    });

    alt.on('RemoveLoadingPrompt', () => game.busyspinnerOff());

    alt.onServer('FadeIn', (args: number) => game.doScreenFadeIn(args));
    alt.onServer('FadeOut', (args: number) => game.doScreenFadeOut(args));
    alt.on('FadeIn', (args: number) => game.doScreenFadeIn(args));
    alt.on('FadeOut', (args: number) => game.doScreenFadeOut(args));

    /*
     * Vehicle
    */
    alt.onServer('VehicleSetSirenSound', (vehicle: alt.Vehicle, status: boolean) => {
        game.setVehicleHasMutedSirens(vehicle.scriptID, status);
    });

    alt.onServer('PlaySoundFrontEnd', (id: number, audioName: string, audioRef: string) => {
        game.playSoundFrontend(id, audioName, audioRef, true);   
    });

    alt.onServer('PlaySoundFromEntity', (entity: alt.Entity, id: number, audioName: string, audioRef: string) => {
        game.playSoundFromEntity(id, audioName, entity.scriptID, audioRef, true, 0);
    });

    alt.onServer('SetVehicleOnGroundProperly', (vehicle: alt.Vehicle) => {
        game.setVehicleOnGroundProperly(vehicle.scriptID, 5);
    });

    alt.onServer('SetVehiclePosition', (vehicle: alt.Vehicle, pos: Vector3) => {
        game.setEntityCoords(vehicle.scriptID, pos.x, pos.y, pos.z, false, false, false, false);
    });
}