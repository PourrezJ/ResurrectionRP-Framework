import * as alt from 'alt';
import * as game from 'natives';
import * as utils from '../Utils/Utils';
import * as ui from '../Helpers/UiHelper';

export class Admin {

    private noclip: boolean;
    private loopID: number;

    private speed: number = 1;
    private fspeed: number = 4;

    constructor() {
        alt.onServer('TeleportToWaypoint', async () => {
            game.doScreenFadeOut(10);
            let pos: alt.Vector3 = await utils.ForceGroundZ(utils.GetWaypointPos());

            if (alt.Player.local.vehicle != null)
                game.setEntityCoordsNoOffset(alt.Player.local.vehicle.scriptID, parseInt(pos.x.toString()), parseInt(pos.y.toString()), parseInt(pos.z.toString()), false, false, false);
            else
                game.setEntityCoordsNoOffset(alt.Player.local.scriptID, parseInt(pos.x.toString()), parseInt(pos.y.toString()), parseInt(pos.z.toString()), false, false, false);
            game.doScreenFadeIn(50);
        });

        alt.onServer('ToggleNoclip', () => {
            this.noclip = !this.noclip;
            alt.log(this.noclip)
            let id = alt.Player.local.scriptID;
            game.setEntityInvincible(id, this.noclip);
            game.freezeEntityPosition(id, false);
            game.setEntityCollision(id, !this.noclip, !this.noclip);
            game.setEntityHasGravity(id, !this.noclip);
       
            if (!this.noclip)
            {
                alt.clearEveryTick(this.loopID);
                game.setEntityMaxSpeed(id, 10);
            }
            else
                this.loopID = alt.everyTick(this.onTick.bind(this));
        });

        alt.onServer('SetInvincible', (statut: boolean) => {
            game.setEntityInvincible(alt.Player.local.scriptID, statut);
        }); 

        alt.onServer('AnnonceGlobal', (text: string, title: string, othertitle: string) => {
            ui.WeazelNews(text, title, othertitle);
        }); 
    }

    onTick() {

        let pos: alt.Vector3 = alt.Player.local.pos;
        let dir = utils.GetCameraDirection();

        let x = pos.x as number;
        let y = pos.y as number;
        let z = pos.z as number;

        let dx = dir.x as number;
        let dy = dir.y as number;
        let dz = dir.z as number;

        game.setEntityVelocity(alt.Player.local.scriptID, 0.0001, 0.0001, 0.0001);

        if (game.isControlPressed(0, 32) && !game.isControlPressed(0, 21)) // W
        {
            x = x + this.speed * dx;
            y = y + this.speed * dy;
            z = z + this.speed * dz;
        }
        else if (game.isControlPressed(0, 32) && game.isControlPressed(0, 21))
        {
            x = x + this.fspeed * dx;
            y = y + this.fspeed * dy;
            z = z + this.fspeed * dz;
        }
        else if (game.isControlPressed(0, 269)) {
            x = x - this.speed * dx;
            y = y - this.speed * dy;
            z = z - this.speed * dz;
        }

        game.setEntityCoordsNoOffset(alt.Player.local.scriptID, x, y, z, false, false, false);
    }
}