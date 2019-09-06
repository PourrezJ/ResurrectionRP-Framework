import * as alt from 'alt';
import * as native from 'natives';

export class Ped {
    public scriptID: number;

    constructor(model, pos) {
        this.scriptID = native.createPed(
            1,
            native.getHashKey(model),
            pos.x,
            pos.y,
            pos.z,
            0,
            false,
            false
        );
    }

    destroy() {
        native.deleteEntity(this.scriptID);
    }
}