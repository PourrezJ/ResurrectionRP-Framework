
import * as alt from 'alt';
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

    alt.onServer('HeadVariation', (args0: number, args1: number, args2: number, args3: number, args4: number, args5: number, args6: number, args7: number, args8: number) => {
        game.setPedHeadBlendData(alt.Player.local.scriptID, args0, args1, args2, args3, args4, args5, args6, args7, args8, false);
    });

    alt.onServer('FaceFeatureVariation', (args0: number, args1: number) => {
        game.setPedFaceFeature(alt.Player.local.scriptID, args0, args1 )
    });

    alt.onServer('HeadOverlayVariation', (args0: number, args1: number, args2: number, args3: number, args4: number) => {
        game.setPedHeadOverlay(alt.Player.local.scriptID, args4, args0, args1);
        game.setPedHeadOverlayColor(alt.Player.local.scriptID, args4, 1, args2, args3);
    });

    alt.onServer('DecorationVariation', (args0: number, args1: number) => {
        game.addPedDecorationFromHashes(alt.Player.local.scriptID, args0, args1);
    });

    alt.onServer('PlayAnimation', (args0: string) =>
    {
        let sync = JSON.parse(args0);
        utils.playAnimation(sync.AnimDict, sync.AnimName, sync.BlendInSpeed, sync.Duraction, sync.Flag);
    });
}