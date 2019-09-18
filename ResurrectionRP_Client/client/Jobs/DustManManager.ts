import * as alt from 'alt';
import * as game from 'natives';
import { Trash } from './Data/Trash';

export class DustManManager {

    public blip: alt.PointBlip;
    public Trash: Trash[] = [];
    public active: boolean;
    public roadDepot: boolean;
    public actualTrash: number;
    public truck: alt.Vehicle;

    constructor() {
        alt.onServer("Jobs_Dustman", this.init);
    }

    public init = (pos, trashliste) => {
        pos = JSON.parse(pos);
        this.active = true;
        var trashlist: any[] = JSON.parse(trashliste);

        this.blip = new alt.PointBlip(pos.X, pos.Y, pos.Z);
        this.blip.sprite = 398;
        this.blip.name = "[JOB] Zone de ramassage";
        this.blip.scale = 0.5;
        this.blip.color = 0;

        this.truck = alt.Player.local.vehicle;

        trashlist.forEach((item, index) => {
            this.Trash[index] = new Trash(item);
        })

        alt.everyTick(this.update);
        alt.on("keydown", this.onKeyDown);
    };

    public update = () => {
        if (!this.active)
            return;
        if (!this.roadDepot) {
            if (this.Trash.length == 0) {
                alt.emit("Display_subtitle", "Vous avez fini, rentrez au dépôt ! ", 7000);
                this.blip.destroy();
                alt.emitServer("Jobs_Dustman_Depot");
                this.roadDepot = true;
            }

            this.Trash.forEach((item: Trash, index) => {
                if (game.getDistanceBetweenCoords(item.Position.X, item.Position.Y, item.Position.Z, alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, false) <= 3 &&
                    alt.Player.local.vehicle == null &&
                    this.actualTrash == null) {
                    alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour prendre la poubelle!", 100);
                }
            });
        }

        if (this.roadDepot)
            return;
        if (game.getDistanceBetweenCoords(this.truck.pos.x, this.truck.pos.y, this.truck.pos.z, alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, false) <= 3) {
            if (this.Trash == null || this.actualTrash == null)
                return;
            alt.emit("Display_Help", "Appuyez sur ~INPUT_CONTEXT~ pour mettre la poubelle dans le camion!", 100);

        }
    }

    public onKeyDown = (key: number) => {
        if (!this.active)
            return;
        if (key != 69)
            return;
        if (!this.roadDepot) {

            this.Trash.forEach((item: Trash, index) => {
                if (game.getDistanceBetweenCoords(item.Position.X, item.Position.Y, item.Position.Z, alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, false) <= 3 &&
                    alt.Player.local.vehicle == null &&
                    this.actualTrash == null) {
                    item.Take();
                    this.actualTrash = index;
                }
            });
        }
        if (this.roadDepot)
            return;
        if (game.getDistanceBetweenCoords(this.truck.pos.x, this.truck.pos.y, this.truck.pos.z, alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, false) <= 3) {
            if (this.Trash == null)
                return;
            game.detachEntity(this.Trash[this.actualTrash].Object, true, true);
            game.deleteObject(this.Trash[this.actualTrash].Object);
            delete this.Trash[this.actualTrash];
            this.actualTrash = null;
            var temp = [];
            this.Trash.forEach((item, index) => {
                if (item != undefined)
                    temp.push(item);
            });
            this.Trash = temp;


        }
    }
}