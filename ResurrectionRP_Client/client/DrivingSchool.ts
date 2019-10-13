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
    private Checkpoint: any;

    constructor() {
        alt.onServer("DrivingSchool_End", () => { this.begin = false; });

        alt.onServer("DriveSchool_CreateCP", (poss: string) => {
            var pos = JSON.parse(poss);

            if (!this.begin)
                this.begin = true;

            if (pos.X == undefined) {
                this.Checkpoint = undefined;
                this.begin = false;
                game.setWaypointOff();

            }
            else {
                this.Checkpoint = { PosX: pos.X, PosY: pos.Y, PosZ: pos.Z };
                game.setNewWaypoint(pos.X, pos.Y)
            }
        });
        alt.everyTick(this.onTick);
    }

    public onTick = () => {
        if (!this.begin)
            return;
        if (this.Checkpoint != undefined)

            game.drawMarker(
                1,
                this.Checkpoint.PosX,
                this.Checkpoint.PosY,
                this.Checkpoint.PosZ,
                0,
                0,
                0,
                0,
                0,
                0,
                5,
                5,
                5,
                255,
                0,
                0,
                128,
                true,
                true,
                0,
                false,
                undefined,
                undefined,
                true
            );
        if (this.lastCheck + 7000 > Date.now())
            return;
        this.lastCheck = Date.now();
        if (alt.Player.local.vehicle == null)
            return;
        var veh = alt.Player.local.vehicle;
        alt.emitServer("DrivingSchool_Checker", Math.ceil(veh.speed * (veh.speed / 20) *2));
    }
}