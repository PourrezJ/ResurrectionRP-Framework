import * as alt from 'alt';
import * as game from 'natives';

export class Blesse {
    constructor() {
        alt.onServer("ResurrectPlayer", () =>
        {

            //Game.Instance.KeyHandler.Remove((int)ConsoleKey.Y); TODO
            //Game.Instance.KeyHandler.Remove((int)ConsoleKey.R);

            game.setPlayerHealthRechargeMultiplier(alt.Player.local.scriptID, 0);
            game.animpostfxStop("DeathFailMPIn")
            game.setCamEffect(0);

            game.setFadeInAfterDeathArrest(false);
            game.setFadeOutAfterArrest(false);
            game.pauseDeathArrestRestart(true);
            game.setFadeInAfterLoad(false);
            game.setFadeOutAfterDeath(false);
        });
    }
}