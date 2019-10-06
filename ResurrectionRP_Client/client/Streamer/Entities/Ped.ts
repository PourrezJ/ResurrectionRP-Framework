import * as alt from 'alt';
import * as game from 'natives';

export default class Ped 
{
    public Handle: number;
    public RemoteID: number;

    constructor(id: number, model: number, position: alt.Vector3, heading: number)
    {
        this.RemoteID = id;
        this.Handle = game.createPed(26, model, position.x as number, position.y as number, position.z as number -1, heading, false, true);
    }

    public Freeze(status: boolean) {
        game.taskSetBlockingOfNonTemporaryEvents(this.Handle, true);
        game.setEntityInvincible(this.Handle, true);
        game.freezeEntityPosition(this.Handle, true);
    }

    public TaskGoTo(position: alt.Vector3, speed: number = 1, timeout: number = 10000) {
        game.taskGoStraightToCoord(this.Handle, position.x as number, position.y as number, position.z as number, speed, timeout, 0.0, 0.0); 
    }
}