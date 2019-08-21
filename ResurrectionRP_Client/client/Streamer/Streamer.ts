import * as alt from 'alt';
import * as game from 'natives';

export class Streamer {
    constructor() {

        alt.on("onStreamIn", this.onStreamIn);
        alt.on("onStreamOut", this.onStreamOut);
        alt.on("onStreamDataChange", this.onStreamDataChange);

    }



    private onStreamIn(entity: object) {
        alt.log("EVENT STREAM IN : " + JSON.stringify(entity));

    }
    private onStreamOut(entity: object) {
        alt.log("EVENT STREAM OUT : " + JSON.stringify(entity));

    }
    private onStreamDataChange(entity: object, data: object) {

    }
}