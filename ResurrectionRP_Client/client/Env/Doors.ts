import * as alt from 'alt';
import * as game from 'natives';

class Door {

    public ID: number;
    public Hash: number;
    public Position: Vector3;
    public Locked: boolean;
    public Hide: boolean;

    constructor(id: number, hash: number, position: Vector3, locked: boolean = false) {
        this.ID = id;
        this.Hash = hash;
        this.Position = position;
        this.Locked = locked;
    }

    public setDoorLockStatus = (locked: boolean) => {
        game.doorControl(this.Hash, this.Position.x, this.Position.y, this.Position.z, locked, 0, 0, 0);
    }

    public setDoorOpenStatus = (angle: number) => {
        if (!this.Locked)
            game.setStateOfClosestDoorOfType(this.Hash, this.Position.x, this.Position.y, this.Position.z, this.Locked, angle, false);
    }
}

export class Doors {
    public DoorsList: Door[] = [];

    constructor() {
        alt.onServer("SetAllDoorStatut", (items: string) => {
            var tDoorsList = JSON.parse(items);

            tDoorsList.forEach((item: any) => {

                let pos = new alt.Vector3(item.Position.X, item.Position.Y, item.Position.Z);

                let door = new Door(item.ID, item.Hash, pos as Vector3, item.Locked);
                door.setDoorLockStatus(item.Locked);
                this.DoorsList.push(door);
            });
        });

        alt.onServer("SetDoorLockState", this.setDoorLockState);
        alt.onServer("SetDoorOpenState", this.setDoorOpenState);
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