
import * as alt from 'alt';
import * as game from 'natives';
import * as UiHelper from 'client/helpers/UiHelper';


var helpText;
var subtitle;
var loading;


export function initialize() {
    alt.onServer('SetPlayerIntoVehicle', (vehicle, seat) => {
        alt.setTimeout(() => {
            game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, seat);
        }, 20);
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

    alt.onServer('showLoading', (text, time, type, toggled) => {
        new Loading(text, time, type, toggled);
    });

    alt.onServer("showCursor", (state: boolean) => alt.showCursor(state));
    alt.onServer('showNotification', (text) => {
        game.setNotificationTextEntry("STRING");
        game.addTextComponentSubstringPlayerName(text);
        game.drawNotification(true, false);
    });

    alt.on('SET_NOTIFICATION_BACKGROUND_COLOR', (args: any[]) => game.setNotificationBackgroundColor(parseInt(args[0])))

    alt.on('RemoveLoadingPrompt', () => game.removeLoadingPrompt());

    alt.onServer('FadeIn', (args: any[]) => game.doScreenFadeIn(parseInt(args[0])));
    alt.onServer('FadeOut', (args: any[]) => game.doScreenFadeOut(parseInt(args[0])));
    alt.on('FadeIn', (args: any[]) => game.doScreenFadeIn(parseInt(args[0])));
    alt.on('FadeOut', (args: any[]) => game.doScreenFadeOut(parseInt(args[0])));

    alt.on('SetNotificationMessage', (args: any[]) => SetNotificationPicture(args[0], args[1], args[1], args[2], args[3], args[4], args[5]))

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

    function ForceGroundZ(v: Vector3) {
        let zcoord = 0.0;

        let firstCheck = [0, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000];

        let secondCheck = [
            1000, 900, 800, 700, 600, 500,
            400, 300, 200, 100, 0, -100, -200, -300, -400, -500
        ];

        let thirdCheck = [
            -500, -400, -300, -200, -100, 0,
            100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
        ];

        game.getGroundZFor3dCoord(v[0], v[1], 1000, zcoord, false);

        if (zcoord == 0) {
            for (let i = firstCheck.length - 1; i >= 0; i--)
            {
                game.requestCollisionAtCoord(v[0], v[1], firstCheck[i])
                game.wait(0);
            }

            game.getGroundZFor3dCoord(v[0], v[1], 1000, zcoord, false);
        }

        if (zcoord == 0) {
            for (let i = secondCheck.length - 1; i >= 0; i--)
            {
                game.requestCollisionAtCoord(v[0], v[1], secondCheck[i]);
                game.wait(0);
            }
            
            game.getGroundZFor3dCoord(v[0], v[1], 1000, zcoord, false);
        }

        if (zcoord == 0) {
            for (let i = thirdCheck.length - 1; i >= 0; i--)
            {
                game.requestCollisionAtCoord(v[0], v[1], secondCheck[i]);
                game.wait(0);
            }

            game.getGroundZFor3dCoord(v[0], v[1], 1000, zcoord, false);
        }
        return [v[0], v[1], zcoord];
        //return new Vector3(v.X, v.Y, zcoord);
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

            drawText(this.text, 0.5, 0.85, 0.8, 4, 255, 255, 255, this.alpha, true);
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
            game.removeLoadingPrompt();
            game.beginTextCommandBusyString('STRING');
            game.addTextComponentSubstringPlayerName(this.text);
            game.endTextCommandBusyString(this.type);
        }

        Draw() {
            if (this.time < Date.now()) {
                loading = undefined;
                game.removeLoadingPrompt();
            }

            if (this.toggled !== null && this.toggled !== undefined && !this.toggled) {
                loading = undefined;
                game.removeLoadingPrompt();
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
        game.setUiLayer(layer);
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

        game.endTextCommandDisplayText(x, y);
    }


    alt.on('update', () => {
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
        game.renderScriptCams(false, false, 0, false, false);
        game.destroyAllCams(true);

        // Turn off Screen Fades
        game.doScreenFadeIn(1);
        game.transitionFromBlurred(1);
    });

    
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
    var len = Math.sqrt(x * x + y * y + z * z);
    if (len !== 0) {
        x = x / len;
        y = y / len;
        z = z / len;
    }
    return new alt.Vector3(x, y, z);
}
