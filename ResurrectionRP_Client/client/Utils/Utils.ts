
import * as alt from 'alt';
import * as game from 'natives';
import * as enums from '../Utils/Enums/Enums';

var helpText;
var subtitle;
var loading;

export function initialize() {

    alt.onServer('TaskStartScenarioAtPosition', (scenarioName: string, x: number, y: number, z: number, heading: number, duration: number, sittingScenario: boolean, teleport: boolean) => {
        alt.log(`${x} ${y} ${z} ${heading}`);
        game.taskStartScenarioAtPosition(alt.Player.local.scriptID, scenarioName, x, y, z, heading, duration, sittingScenario, teleport);
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
        game.requestCollisionAtCoord(x,y,z);
    });

    alt.onServer('ShowNotification', (imageName, headerMsg, detailsMsg, message) => {
        game.setNotificationTextEntry('STRING');
        game.addTextComponentSubstringPlayerName(message);
        game.setNotificationMessageClanTag(imageName.toUpperCase(), imageName.toUpperCase(), false, 4, headerMsg, detailsMsg, 1.0, '');
        game.drawNotification(false, false);
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

    alt.onServer('CreateBlip', (sprite: number, position: alt.Vector3, name: string, scale: number, color: number, alpha: number, shortRange: boolean) =>
    {
        createBlip(position, sprite, color, name, alpha, scale, shortRange);
    });

    alt.onServer('setEntityHeading', (entity: alt.Entity, angle: number) =>
    {
        game.setEntityHeading(entity.scriptID, angle);
    });

    alt.onServer('ShowNotification', (text) => {
        game.setNotificationTextEntry("STRING");
        game.addTextComponentSubstringPlayerName(text);
        game.drawNotification(true, false);
    });

    alt.on('SET_NOTIFICATION_BACKGROUND_COLOR', (args: any[]) => game.setNotificationBackgroundColor(parseInt(args[0])))

    alt.onServer("SetNotificationMessage", (img, sender, subject, message) =>
    {
        game.setNotificationTextEntry("STRING");
        game.addTextComponentSubstringPlayerName(message);
        game.setNotificationMessage2(img.toUpperCase(), img.toUpperCase(), false, 4, sender, subject);
        game.drawNotification(false, false);
    });

    alt.on("SetNotificationMessage", (img, sender, subject, message) =>
    {
        game.setNotificationTextEntry("STRING");
        game.addTextComponentSubstringPlayerName(message);
        game.setNotificationMessage2(img.toUpperCase(), img.toUpperCase(), false, 4, sender, subject);
        game.drawNotification(false, false);
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
        game.setDisableVehicleSirenSound(vehicle.scriptID, status);
    });

    function SetNotificationPicture(message: string, dict: string, img: string, flash: boolean, iconType: number, sender: string, subject: string) {
        game.setNotificationTextEntry("STRING");
        game.addTextComponentSubstringPlayerName(message);
        game.setNotificationMessage2(dict, img, flash, iconType, sender, subject);
        game.drawNotification(true, false);
    }

    function RequestScaleForm(scaleformId: string) {

        var sf = game.requestScaleformMovie(scaleformId);
        return sf;
    }

    function BeginTextCommand(textCommand: string) {
        game.beginTextCommandScaleformString(textCommand);
        game.endTextCommandScaleformString();
        //RAGE.Game.Graphics.PopScaleformMovieFunctionVoid();
        
    }

    class HelpText {
        text: string;
        time: number;
        helpText: string;
        constructor(text, time) {
            this.text = text;
            this.time = Date.now() + time;
            helpText = this
        }

        Draw() {
            if (this.time < Date.now()) {
                helpText = undefined;
            }

            game.beginTextCommandDisplayHelp('STRING');
            game.addTextComponentSubstringPlayerName(this.text);
            game.endTextCommandDisplayHelp(0, false, true, 0);
        }
    }

    class Subtitle {
        alpha: number;
        text: string;
        time: number;
        res: any;
        constructor(text, time) {
            this.alpha = 255;
            this.text = text;
            this.time = Date.now() + time;
            this.res = game.getActiveScreenResolution(0, 0);
            subtitle = this;
        }

        Draw() {
            if (this.time < Date.now()) {
                this.alpha -= 1;
            }

            if (this.alpha <= 10) {
                subtitle = undefined;
                return;
            }

            drawText(this.text, 0.5, 0.80, 0.8, 4, 255, 255, 255, this.alpha, true);
        }
    }
    class Loading {
        text: string;
        type: any;
        toggled: boolean;
        time: number;
        constructor(text, time, type, toggled) {
            this.text = text;
            this.type = type;
            this.toggled = toggled;

            if (time !== null && time !== undefined) {
                this.time = Date.now() + time;
            }

            loading = this;
            game.busyspinnerOff();
            game.beginTextCommandBusyspinnerOn('STRING');
            game.addTextComponentSubstringPlayerName(this.text);
            game.endTextCommandBusyspinnerOn(this.type);
        }

        Draw() {
            if (this.time < Date.now()) {
                loading = undefined;
                game.busyspinnerOff();
            }

            if (this.toggled !== null && this.toggled !== undefined && !this.toggled) {
                loading = undefined;
                game.busyspinnerOff();
            }
        }
    }

    /**
 * Draw text in an update loop.
 * @param msg 
 * @param x is a float 0 - 1.0
 * @param y is a float 0 - 1.0
 */
    function drawText(msg, x, y, scale, fontType, r, g, b, a, useOutline = true, useDropShadow = true, layer = 0) {
        game.setScriptGfxDrawOrder(layer);
        game.beginTextCommandDisplayText('STRING');
        game.addTextComponentSubstringPlayerName(msg);
        game.setTextFont(fontType);
        game.setTextScale(1, scale);
        game.setTextWrap(0.0, 1.0);
        game.setTextCentre(true);
        game.setTextColour(r, g, b, a);

        if (useOutline)
            game.setTextOutline();

        if (useDropShadow)
            game.setTextDropShadow();

        game.endTextCommandDisplayText(x, y, 0);
    }

    alt.everyTick(() => {
        if (helpText !== undefined)
            helpText.Draw();
        if (subtitle !== undefined)
            subtitle.Draw();
        if (loading !== undefined)
            loading.Draw();
    });

    alt.on('disconnect', () => {

        // Unfreeze Player
        game.freezeEntityPosition(alt.Player.local.scriptID, false);

        // Destroy All Cameras
        game.renderScriptCams(false, false, 0, false, false, 0);
        game.destroyAllCams(true);

        // Turn off Screen Fades
        game.doScreenFadeIn(1);
        game.transitionFromBlurred(1);
    });

    
}

export function Distance(positionOne, positionTwo) {
    return Math.sqrt(Math.pow(positionOne.x - positionTwo.x, 2) + Math.pow(positionOne.y - positionTwo.y, 2) + Math.pow(positionOne.z - positionTwo.z, 2));
}

//export function GetCameraDirection() {
//    const heading = game.getGameplayCamRelativeHeading() + game.getEntityHeading(alt.Player.local.scriptID);
//    const pitch = game.getGameplayCamRot(0).x;
//    var x = -Math.sin(heading * Math.PI / 180.0);
//    var y = Math.cos(heading * Math.PI / 180.0);
//    var z = Math.sin(pitch * Math.PI / 180.0);
//    var len = Math.sqrt(x * x + y * y + z * z);
//    if (len !== 0) {
//        x = x / len;
//        y = y / len;
//        z = z / len;
//    }
//    return new alt.Vector3(x, y, z);
//}

export function GetCameraDirection() {
    const heading = game.getGameplayCamRelativeHeading() + game.getEntityHeading(alt.Player.local.scriptID);
    const pitch = game.getGameplayCamRot(0).x;
    var x = -Math.sin(heading * Math.PI / 180.0);
    var y = Math.cos(heading * Math.PI / 180.0);
    var z = Math.sin(pitch * Math.PI / 180.0);

    return new alt.Vector3(x, y, z);
}

export function ArrayRemove(arr, value) {

    return arr.filter(function (ele) {
        return ele != value;
    });

}

//let _necessaryControlsForController = [2,1,25,24];
//let _necessaryControlsForKeyboard = [201, 195, 196, 187, 188, 189, 190, 202, 217, 242, 241, 239, 240, 31, 30, 21, 22, 23, 71, 72, 89, 9, 8, 90, 76, 63, 64, 278, 279, 34, 35, 189, 190];
let _necessaryControls =
    [
        enums.Control.FrontendAccept,
        enums.Control.FrontendAxisX,
        enums.Control.FrontendAxisY,
        enums.Control.FrontendDown,
        enums.Control.FrontendUp,
        enums.Control.FrontendLeft,
        enums.Control.FrontendRight,
        enums.Control.FrontendCancel,
        enums.Control.FrontendSelect,
        enums.Control.CursorScrollDown,
        enums.Control.CursorScrollUp,
        enums.Control.CursorX,
        enums.Control.CursorY,
        enums.Control.MoveUpDown,
        enums.Control.MoveLeftRight,
        enums.Control.Sprint,
        //enums.Control.Jump,
        enums.Control.Enter,
        enums.Control.VehicleExit,
        enums.Control.VehicleAccelerate,
        enums.Control.VehicleBrake,
        enums.Control.VehicleMoveLeftRight,
        enums.Control.VehicleFlyYawLeft,
        enums.Control.FlyLeftRight,
        enums.Control.FlyUpDown,
        enums.Control.VehicleFlyYawRight,
        enums.Control.VehicleHandbrake,
    ];

export function DisEnableControls(enabled: boolean) {

    if (enabled)
        game.enableAllControlActions(0);
    else
        game.disableAllControlActions(0);

    if (enabled)
        return;

    _necessaryControls.forEach((control: number) => {
        game.enableControlAction(0, control, true);
    });

}

export function playAnimation(dictionary, name, speed, durationInMS, flag) {

    if (game.hasAnimDictLoaded(dictionary)) {
        game.taskPlayAnim(
            alt.Player.local.scriptID,
            dictionary,
            name,
            speed,
            -1,
            durationInMS,
            flag,
            1.0,
            false,
            false,
            false
        );
        return;
    }

    let res = loadAnim(dictionary);
    res.then(() => {
        game.taskPlayAnim(
            alt.Player.local.scriptID,
            dictionary,
            name,
            speed,
            -1,
            durationInMS,
            flag,
            1.0,
            false,
            false,
            false
        );
    });
}

export async function loadAnim(dict) {
    return new Promise(resolve => {
        game.requestAnimDict(dict);

        let inter = alt.setInterval(() => {
            if (game.hasAnimDictLoaded(dict)) {
                resolve(true);
                alt.clearInterval(inter);
                return;
            }
        }, 5);
    });
}

export function GetWaypointPos()
{
    let id: number = game.getFirstBlipInfoId(8);
    return (id > 0) ? game.getBlipInfoIdCoord(id) : new alt.Vector3(0,0,0);
}

export async function Wait(ms: number) {
    return new Promise(resolve => {
        alt.setTimeout(resolve, ms);
    });
}

export async function ForceGroundZ(v: alt.Vector3) {
    let zcoord = 0.0;
    let temp = null;

    let x: number = parseInt(v.x.toString());
    let y: number = parseInt(v.y.toString());

    let firstCheck = [0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000];

    let secondCheck = [
        1000, 900, 800, 700, 600, 500,
        400, 300, 200, 100, 0, -100, -200, -300, -400, -500
    ];

    let thirdCheck = [
        -500, -400, -300, -200, -100, 0,
        100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
    ];

    temp = game.getGroundZFor3dCoord(x, y, 1000, zcoord, true, false);
    await Wait(10);
    zcoord = temp[1];

    if (zcoord == 0) {
        for (let i = firstCheck.length - 1; i >= 0; i--) {
            game.requestCollisionAtCoord(x, y, firstCheck[i]);
            await Wait(10);
        }
        temp = game.getGroundZFor3dCoord(x, y, 1000, zcoord, true, false);
        alt.log(temp);
        zcoord = temp[1];
    }

    if (zcoord == 0) {
        for (let i = secondCheck.length - 1; i >= 0; i--) {
            game.requestCollisionAtCoord(x, y, secondCheck[i]);
            await Wait(10);
        }

        temp = game.getGroundZFor3dCoord(x, y, 1000, zcoord, true, false);
        zcoord = temp[1];
    }

    if (zcoord == 0) {
        for (let i = thirdCheck.length - 1; i >= 0; i--) {
            game.requestCollisionAtCoord(x, y, secondCheck[i]);
            await Wait(10);
        }
        temp = game.getGroundZFor3dCoord(x, y, 1000, zcoord, true, false);
        zcoord = temp[1];
    }
    v.z = zcoord + 1;
    return v;
}

export function loadModelAsync(model) {
    return new Promise((resolve, reject) => {
        if (typeof model === 'string') {
            model = game.getHashKey(model);
        }

        if (!game.isModelValid(model))
            return resolve(false);

        if (game.hasModelLoaded(model))
            return resolve(true);

        game.requestModel(model);

        let interval = alt.setInterval(() => {
            if (game.hasModelLoaded(model)) {
                alt.clearInterval(interval);
                return resolve(true);
            }
        }, 0);
    });
}

export async function loadMovement(dict) {
    return new Promise(resolve => {
        game.requestAnimSet(dict);

        let inter = alt.setInterval(() => {
            if (game.hasAnimSetLoaded(dict)) {
                resolve(true);
                alt.clearInterval(inter);
                return;
            }
        }, 5);
    });
}

export function createBlip(pos: alt.Vector3, sprite: number, color: number, name: string, alpha: number, scale: number, shortRange: boolean): alt.PointBlip {

    let x: number = pos.x as number;
    let y: number = pos.y as number;
    let z: number = pos.z as number;

    const blip = new alt.PointBlip(x, y, z);
    blip.shortRange = true;
    blip.sprite = sprite;
    blip.color = color;
    blip.name = name;
    blip.alpha = alpha;
    blip.scale = scale;
    blip.shortRange = shortRange;

    return blip;
}
