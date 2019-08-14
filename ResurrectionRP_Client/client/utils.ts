
import * as alt from 'alt';
import * as game from 'natives';
import * as UiHelper from 'client/helpers/UiHelper';

export function initialize() {
    alt.onServer('SetPlayerIntoVehicle', (vehicle, seat) => {
        alt.setTimeout(() => {
            game.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, seat);
        }, 20);
    });
    alt.on('Display_subtitle', (args: any[]) => UiHelper.ShowSubTitle(args[0], parseInt(args[1])));

    alt.on('SET_NOTIFICATION_BACKGROUND_COLOR', (args: any[]) => game.setNotificationBackgroundColor(parseInt(args[0])))

    alt.on('RemoveLoadingPrompt', () => game.removeLoadingPrompt());

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

    
}
