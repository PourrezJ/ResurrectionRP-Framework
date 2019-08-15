
import * as alt from 'alt';
import * as game from 'natives';


export function init() {
    var playerId = alt.Player.local.scriptID;
    

    alt.onServer('ComponentVariation', (arg: number, arg1: number, arg2: number, arg3: number) => {
        game.setPedComponentVariation(playerId, arg, arg1, arg2, arg3);
    });
    alt.onServer('HairVariation', (arg0: number, arg1: number) => {
        game.setPedHairColor(playerId, arg0, arg1)
    });
    alt.onServer('EyeColorVariation', (arg: number) => {
        game.setPedEyeColor(playerId, arg);
    })
    alt.onServer('HeadVariation', (args0: number, args1: number, args2: number, args3: number, args4: number, args5: number, args6: number, args7: number, args8: number) => {
        game.setPedHeadBlendData(playerId, args0, args1, args2, args3, args4, args5, args6, args7, args8, false);
    });
    alt.onServer('FaceFeatureVariation', (args0: number, args1: number) => {
        game.setPedFaceFeature(playerId, args0, args1 )
    });
    alt.onServer('HeadOverlayVariation', (args0: number, args1: number, args2: number, args3: number, args4: number) => {
        game.setPedHeadOverlay(playerId, args0, args1, args2);
        game.setPedHeadOverlayColor(playerId, args0, 0, args3, args4);
        
    });
    alt.onServer('DecorationVariation', (args0: number, args1: number) => {
        game.setPedDecoration(playerId, args0, args1);
    });
}