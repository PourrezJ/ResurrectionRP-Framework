import * as alt from 'alt';
import * as game from 'natives';

export default class Ragdoll {

    private ragdoll: boolean;

    constructor() {
        this.ragdoll = false;

        alt.everyTick(() => {
            if (this.ragdoll) {
                this.doRagdoll();
            }
        });
    }

    start() {
        this.doRagdoll();
    }

    stop() {
        this.setRagdoll(false);
    }

    setRagdoll(value) {
        this.ragdoll = value;
    }

    doRagdoll() {
            // prevent if player is in any vehicle
            if (game.isPedInAnyVehicle(alt.Player.local.scriptID, false)) {

            // but not on bikes
                if (!game.isPedOnAnyBike(alt.Player.local.scriptID)) {
                return;
            }

        } else {

            const currentWeapon = game.getSelectedPedWeapon(alt.Player.local.scriptID);

            // prevent if player is holding weapon and isn't jumping or climbing
                if (game.getWeaponClipSize(currentWeapon) > 0 && !game.isPedJumping(alt.Player.local.scriptID) && !game.isPlayerClimbing(alt.Player.local.scriptID)) {
                return;
            }
        }

        game.setPedToRagdoll(alt.Player.local.scriptID, 1000, 1000, 0, false, false, false);
    }
}
