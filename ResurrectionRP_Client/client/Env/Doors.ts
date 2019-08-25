import * as alt from 'alt';
import * as game from 'natives';

class Door {

    public ID: number;
    public Hash: number;
    public Position: alt.Vector3;
    public Locked: boolean;
    public Hide: boolean;

    constructor(id: number, hash: number, position: alt.Vector3, locked: boolean = false) {
        this.ID = id;
        this.Hash = hash;
        this.Position = position;
        this.Locked = locked;
    }

    public setDoorLockStatus = (locked: boolean) => {
        game.doorControl(this.Hash, Number.parseInt(this.Position.x + ""), Number.parseInt(this.Position.y + ""), Number.parseInt(this.Position.z + ""), locked, 0, 0, 0);
    }

    public setDoorOpenStatus = (angle: number) => {
        if (!this.Locked)
            game.setStateOfClosestDoorOfType(this.Hash, Number.parseInt(this.Position.x + ""), Number.parseInt(this.Position.y + ""), Number.parseInt(this.Position.z + ""), this.Locked, angle, false);
    }
}

export class Doors {
    public DoorsList: Door[] = [];

    constructor() {
        alt.onServer("SetAllDoorStatut", (items: string) => {
            var tDoorsList = JSON.parse(items);
            tDoorsList.forEach((item: Door, index) => {
                alt.log(item);
                this.DoorsList[index] = new Door(item.ID, item.Hash, item.Position, item.Locked);
                this.DoorsList[index].setDoorLockStatus(item.Locked);
            });
        });

        alt.onServer("SetDoorLockState", this.setDoorLockState);
        alt.on("SetDoorLockState", this.setDoorLockState);



        alt.onServer("SetDoorOpenState", this.setDoorOpenState);
        alt.on("SetDoorOpenState", this.setDoorOpenState);


    }

    public setDoorOpenState = (first: number, second: number) => {
        try {
            var id = first;
            var angle = second;
            var door = this.GetDoorWithID(id);
            if (door != null)
                door.setDoorOpenStatus(angle);
        } catch (ex) {
            alt.logError(ex);
        }
    }
    public setDoorLockState = (first: number, second: boolean) => {
        try {
            var id = first;
            var locked: boolean = second;
            var door = this.GetDoorWithID(id);
            if (door != null)
                door.setDoorLockStatus(locked);
        } catch (ex) {
            alt.logError(ex);
        }
    }

    public GetDoorWithID = (id: number) => this.DoorsList.find(p => p.ID == id);
}