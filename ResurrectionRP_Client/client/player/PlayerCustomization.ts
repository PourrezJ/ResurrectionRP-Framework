import * as alt from 'alt-client';
import * as game from 'natives';
import * as utils from '../Utils/Utils';

export function init() {
    alt.onServer('ComponentVariation', (arg: number, arg1: number, arg2: number, arg3: number) => {
        game.setPedComponentVariation(alt.Player.local.scriptID, arg, arg1, arg2, arg3);
    });

    alt.onServer('PropVariation', (arg: number, arg1: number, arg2: number) => {
        game.setPedPropIndex(alt.Player.local.scriptID, arg, arg1, arg2, true);
    });

    alt.onServer('ClearProp', (componentId: number) => {
        game.clearPedProp(alt.Player.local.scriptID, componentId);
    });

    alt.onServer('HairVariation', (arg0: number, arg1: number) => {
        game.setPedHairColor(alt.Player.local.scriptID, arg0, arg1);
    });

    alt.onServer('EyeColorVariation', (arg: number) => {
        game.setPedEyeColor(alt.Player.local.scriptID, arg);
    })

    alt.onServer('HeadVariation', (arg0: number, arg1: number, arg2: number, arg3: number, arg4: number, arg5: number, arg6: number, arg7: number, arg8: number) => {
        game.setPedHeadBlendData(alt.Player.local.scriptID, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, false);
    });

    alt.onServer('FaceFeatureVariation', (arg0: number, arg1: number) => {
        game.setPedFaceFeature(alt.Player.local.scriptID, arg0, arg1 )
    });

    alt.onServer('HeadOverlayVariation', (arg0: number, arg1: number, arg2: number, arg3: number, arg4: number) => {
        game.setPedHeadOverlay(alt.Player.local.scriptID, arg4, arg0, arg1);
        game.setPedHeadOverlayColor(alt.Player.local.scriptID, arg4, 1, arg2, arg3);
    });

    alt.onServer('DecorationVariation', (arg0: number, arg1: number) => {
        game.addPedDecorationFromHashes(alt.Player.local.scriptID, arg0, arg1);
    });

    alt.onServer('ClearDecorations', () => {
        game.clearPedDecorations(alt.Player.local.scriptID);
    });

    alt.onServer('PlayAnimation', (arg0: string) =>
    {
        let sync = JSON.parse(arg0);
        utils.playAnimation(sync.AnimDict, sync.AnimName, sync.BlendInSpeed, sync.Duraction, sync.Flag);
    });
}