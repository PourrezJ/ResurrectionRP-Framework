import * as alt from 'alt-client';
import * as game from 'natives';
import * as utils from '../Utils/Utils'


export class Effects
{
    constructor() {
        alt.onServer("AlcoholDrink", this.AlcoholDrink.bind(this));
    }

    private async AlcoholDrink(quantity: number): Promise<void>
    {
        if (game.hasAnimDictLoaded('MOVE_M@DRUNK@VERYDRUNK'))
            await utils.loadAnim('MOVE_M@DRUNK@VERYDRUNK');

        game.setPedIsDrunk(alt.Player.local.scriptID, true);
        game.shakeGameplayCam('DRUNK_SHAKE', quantity);
        game.setPedConfigFlag(alt.Player.local.scriptID, 100, true);
        game.setPedMovementClipset(alt.Player.local.scriptID, 'MOVE_M@DRUNK@VERYDRUNK', quantity);
    }
}