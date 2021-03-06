﻿import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from '../Chat/Chat'
import * as camera from '../Models/Camera';

let inLogin = false;
let browser: alt.WebView = null;

export function init() {

   

    alt.onServer('OpenLogin', (args: any[]) => {
        try {
            alt.emit("FadeIn", 0);
            let social = game.scGetNickname();
            game.setPlayerInvincible(game.playerId(), true);
            game.displayRadar(false);
            game.displayHud(false);
            chat.hide(true);
            alt.toggleGameControls(false);
            alt.showCursor(true);
            inLogin = true;

            // Camera
            var cameras: any[] = [
                new camera.Camera({ x: 266.9648, y: - 1425.167, z: 238.3106 }, { x: -10, y: 0, z: 182.3031 }),
                new camera.Camera({ x: 746.0171, y: 1188.269, z: 347.9676 }, { x: 0, y: 0, z: 162.129 }),
                new camera.Camera({ x: 1910.248, y: 3483.922, z: 58.06739 }, { x: 0, y: 0, z: 40 }),
                new camera.Camera({ x: 1939.142, y: 5346.873, z: 175.9322 }, { x: -15, y: 0, z: 199.2901 }),
                new camera.Camera({ x: 15.02087, y: 6089.843, z: 136.7869 }, { x: -15, y: 0, z: 63.14677 }),
                new camera.Camera({ x: 940.410, y: -3242.49, z: 23.49856 }, { x: 0, y: 0, z: 35.33151 }),
            ];
            function getRandomInt(max) {
                return Math.floor(Math.random() * Math.floor(max));
            }
            var _cam = cameras[getRandomInt(5)];
            _cam.SetActiveCamera(true);

            browser = new alt.WebView('http://resource/client/cef/login/index.html', true);
            browser.emit('callEvent', social);
            browser.focus();

            browser.on("SendLogin", (arg: string) => alt.emitServer('SendLogin', arg))
            browser.on("ExitGame", (arg: string) => alt.emitServer('ExitGame'))

            alt.onServer("LoginOK", (arg: boolean) => {

                alt.log("Connexion acceptée, en cours.");
                game.doScreenFadeOut(0);
                game.setPlayerInvincible(game.playerId(), false);
                game.displayRadar(true);
                game.displayHud(true);
                chat.hide(false);
                
                inLogin = false;
                alt.showCursor(false);

                cameras.forEach((cam) => cam.SetActiveCamera(false));
                alt.toggleGameControls(true);
                browser.destroy();
                browser = null;
                game.renderScriptCams(false, false, 0, true, false, 0);
                game.destroyAllCams(true);

                alt.emitServer("LogPlayer");
            });

            alt.onServer("LoginError", () => {
                browser.emit('error');
            });

        } catch(ex) {
            alt.log(ex);
        }
    });

    alt.onServer("GetSocialClub", (arg: string) => 
        alt.emitServer(arg, game.scGetNickname()));
}