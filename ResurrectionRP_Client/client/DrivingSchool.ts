import * as alt from 'alt';
import * as game from 'natives';

enum SchoolStep {
    Stop, Start,
    Wait,
    Ready
}
class Trajet {
    Position: alt.Vector3;
    Speed: number;
}

export class DrivingSchool {

    public id: number;
    private begin: boolean = false;
    private veh: alt.Vehicle;
    private lastCheck: number;
    private advert: number;
    

    constructor() {
        alt.onServer("DrivingSchool_End", () => { this.begin = false; });
        alt.onServer("DrivingSchool_Start", this.start);

        alt.on("update", this.onTick);
    }

    public start = (vehicle) => {
        this.begin = true;

    }

    public onTick = () => {
        if (!this.begin)
            return;
        if (this.lastCheck + 7000 > Date.now())
            return;
        this.lastCheck = Date.now();
        if (alt.Player.local.vehicle == null)
            return;
        var veh = alt.Player.local.vehicle;
        alt.emitServer("DrivingSchool_Checker", Math.ceil(veh.speed * (veh.speed / 20) *2));
    }
}