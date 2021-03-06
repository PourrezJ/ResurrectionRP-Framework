﻿import * as alt from 'alt-client';
import * as game from 'natives';
import * as chat from '../Chat/Chat';
import { Camera } from '../Models/Camera';

const playerPoint = {
    x:  402.8664,
    y: -996.4108,
    z: -100.00027
};

export function OpenCharCreator() {
    class HeadBlend {
        public ShapeFirst: number;
        public ShapeSecond: number;
        public ShapeThird: number;
        public SkinFirst: number;
        public SkinSecond: number;
        public SkinThird: number;
        public ShapeMix: number;
        public SkinMix: number;
        public ThirdMix: number;
    }

    var _camera: Camera;
    var view: alt.WebView;

    try {
        alt.showCursor(true);
        chat.hide(true);

        game.destroyAllCams(true);
        _camera = new Camera({ x: 402.6751, y: -997.00025, z: -98.30025 }, { x: 0, y: 0, z: 0 })
        _camera.SetActiveCamera(true);

        game.renderScriptCams(true, false, 0, true, false, 0);
        game.displayHud(false);
        game.displayRadar(false);
        game.setMouseCursorSprite(6);

        game.requestModel(game.getHashKey('mp_m_freemode_01'));
        game.requestModel(game.getHashKey('mp_f_freemode_01'));
        
        game.setEntityCoords(alt.Player.local.scriptID, playerPoint.x, playerPoint.y, playerPoint.z, false, false, false, false);
        game.setEntityHeading(alt.Player.local.scriptID, 180);

        //game.setPedDefaultComponentVariation(alt.Player.local.scriptID);
        game.setPedHeadBlendData(alt.Player.local.scriptID, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);

        view = new alt.WebView("http://resource/client/cef/charcreator/index.html", true);
        view.focus();

        alt.toggleGameControls(false);

        view.on('CreatorLoad', () => {
            alt.setTimeout(() => view.emit('CharCreatorLoad'), 1000);
        });

        view.on('setGender', (gender: any) => {
            alt.setModel(gender == 1 ? 'mp_f_freemode_01' : 'mp_m_freemode_01');
            game.setEntityCoords(alt.Player.local.scriptID, playerPoint.x, playerPoint.y, playerPoint.z, false, false, false, false);
            game.setEntityHeading(alt.Player.local.scriptID, 180);

            if (gender == 0)
                game.setPedHeadBlendData(alt.Player.local.scriptID, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
            else
                game.setPedHeadBlendData(alt.Player.local.scriptID, 21, 0, 0, 0, 0, 0, 0, 0, 0, false);
        });

        view.on('setComponentVariation', (type: number, index: number) => {
            game.setPedComponentVariation(alt.Player.local.scriptID, type, index, 0, 0);
        })

        view.on('saveCharacter', (first: string, second: string) => {
            view.destroy();
            view = null;
            game.doScreenFadeOut(0);
            game.displayHud(true);
            game.displayRadar(true);
            alt.showCursor(false);
            game.freezeEntityPosition(alt.Player.local.scriptID, false);
            alt.toggleGameControls(true);
            _camera.SetActiveCamera(false);
            _camera.Destroy();
            alt.log('Sauvegarde du character');
            alt.emitServer('MakePlayer', first, second);
            
        });

        view.on('setHairColor', (type: number, index: number) => {
            game.setPedHairColor(alt.Player.local.scriptID, type, index);
        });

        view.on('setEyeColor', (index: number) => {
            game.setPedEyeColor(alt.Player.local.scriptID, index);
        });

        view.on('setHairStyle', (type: number) => {
            game.setPedComponentVariation(alt.Player.local.scriptID, 2, type, 0, 2);
        });

        view.on('setHeadOverlayColor', (type: number, index: number) => {
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, type, 1, index, 0);
        });

        view.on('setFaceFeature', (type: number, index: number) => {
            game.setPedFaceFeature(alt.Player.local.scriptID, type, index);
        });

        view.on('updateHeadOverlay', (type: number, index: string) => {
            var def = JSON.parse(index);
            game.setPedHeadOverlay(alt.Player.local.scriptID, type, def.Index, def.Opacity);
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, type, 1, def.Color, def.SecondaryColor)
        });

        view.on('setHeadBlend', (type: string) => {
            var p = JSON.parse(type);
            game.setPedHeadBlendData(alt.Player.local.scriptID, p.ShapeFirst, p.ShapeSecond, 0, p.SkinFirst, p.SkinSecond, 0, p.ShapeMix, p.SkinMix, 0, false);
        });

        game.doScreenFadeIn(0);
    } catch (ex) {
        alt.log(ex);
    }
}