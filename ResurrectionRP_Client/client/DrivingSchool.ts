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
    private speed: number;
    private Checkpoint: any;
    private Ticker: any;

    constructor() {
        alt.onServer("DrivingSchool_End", () => { this.begin = false; });

        alt.onServer("DriveSchool_CreateCP", (poss: string, maxspeed: number) => {
            var pos = JSON.parse(poss);
            if (maxspeed > 20)
                this.speed = maxspeed;
            if (!this.begin)
                this.begin = true;

            if (pos.X == undefined) {
                this.Checkpoint = undefined;
                this.begin = false;
                game.setWaypointOff();
                alt.clearEveryTick(this.Ticker);

            }
            else {
                this.Checkpoint = { PosX: pos.X, PosY: pos.Y, PosZ: pos.Z };
                game.setNewWaypoint(pos.X, pos.Y)
            }
        });
        this.Ticker = alt.everyTick(this.onTick);
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
        if (this.lastCheck + 4000 > Date.now())
            return;
        this.lastCheck = Date.now();
        if (alt.Player.local.vehicle == null)
            return;
        var veh = alt.Player.local.vehicle;
        if (this.speed < Math.ceil(veh.speed * 3.6))
            alt.emitServer("DrivingSchool_Avert");
    }
}