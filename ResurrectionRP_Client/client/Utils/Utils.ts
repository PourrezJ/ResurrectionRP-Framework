import * as alt from 'alt-client';
import * as game from 'natives';
import * as enums from '../Utils/Enums/Enums';

export function SetNotificationPicture(message: string, dict: string, img: string, flash: boolean, iconType: number, sender: string, subject: string) {
    game.beginTextCommandThefeedPost("STRING");
    game.addTextComponentSubstringPlayerName(message);
    game.endTextCommandThefeedPostMessagetext(dict, img, flash, iconType, sender, subject);
    game.endTextCommandThefeedPostTicker(true, false);
}

export function RequestScaleForm(scaleformId: string) {

    var sf = game.requestScaleformMovie(scaleformId);
    return sf;
}

export function BeginTextCommand(textCommand: string) {
    game.beginTextCommandScaleformString(textCommand);
    game.endTextCommandScaleformString();
}

export function drawText(msg, x, y, scale, fontType, r, g, b, a, useOutline = true, useDropShadow = true, layer = 0) {
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

export function Distance(positionOne, positionTwo) {
    return Math.sqrt(Math.pow(positionOne.x - positionTwo.x, 2) + Math.pow(positionOne.y - positionTwo.y, 2) + Math.pow(positionOne.z - positionTwo.z, 2));
}

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
    return new alt.Vector3(v.x, v.y, zcoord + 1);
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

export function isPlayer(entity: number): boolean {
    return (alt.Player.all.find(p => p.scriptID === entity) != null)
}