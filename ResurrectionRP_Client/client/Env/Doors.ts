import * as alt from 'alt-client';
import * as game from 'natives';

class Door {

    public ID: number;
    public Hash: number;
    public Position: alt.Vector3;
    public Locked: boolean;
    public Hide: boolean;

    constructor(id: number, hash: number, x: number, y: number, z, number, locked: boolean = false) {
        this.ID = id;
        this.Hash = hash;
        this.Position = new alt.Vector3(x,y,z);
        this.Locked = locked;
    }

    public setDoorLockStatus = (locked: boolean) => {
        game.doorControl(this.Hash, this.Position.x as number, this.Position.y as number, this.Position.z as number, locked, 0, 0, 0);
    }
}

export class Doors {
    public DoorsList: Door[] = [];

    constructor() {
        alt.onServer("SetAllDoorStatut", (items: string) => {
            var tDoorsList = JSON.parse(items);

            tDoorsList.forEach((item: any) => {
                let door = new Door(item.ID, item.Hash, item.Position.X, item.Position.Y, item.Position.Z, item.Locked);
                door.setDoorLockStatus(item.Locked);
                this.DoorsList.push(door);
            });
        });

        alt.onServer("SetDoorLockState", this.setDoorLockState);
    }

    public setDoorLockState = (id: number, locked: boolean) => {
        try {
            var door = this.GetDoorWithID(id);
            if (door != null)
                door.setDoorLockStatus(locked);
        } catch (ex) {
            alt.logError(ex);
        }
    }

    public GetDoorWithID = (id: number) => this.DoorsList.find(p => p.ID == id);
}