import * as alt from 'alt';
import * as game from 'natives';

export class Survival {

    public Hunger: number = 100;
    public Thirst: number = 100;

    private _RefreshHunger: number;
    public get RefreshHunger() {
        return this._RefreshHunger;
    };

    private _RefreshThirst: number;
    public get RefreshThirst() {
        return this._RefreshThirst;
    };

    constructor(hunger: number, thirst: number) {
        var playerId = alt.Player.local.scriptID;
        this.Hunger = hunger;
        this.Thirst = thirst;

        this._RefreshHunger = Date.now() + 1000 * 60 * 3;
        this._RefreshThirst = Date.now() + 1000 * 60 * 3;

        alt.on("UpdateHungerThirst", this.UpdateHungerThirst);

        alt.on("update", () => {
            if (this.RefreshHunger < Date.now()) {
                if (this.Hunger > 0) this.Hunger--;
                if (this.Hunger <= 0) {
                    var health = game.getEntityHealth(playerId);
                    game.setEntityHealth(playerId, health - 25);
                    if (health <= -1) {
                        alt.emit("Display_Help", "Vous êtes mort de faim!", 10000);
                        this.Hunger = 100;
                    }
                    this._RefreshHunger = Date.now();

                } else {
                    this._RefreshHunger = Date.now() + 1000 * 60 * 3;
                }
            }
            if (this.RefreshThirst < Date.now()) {
                if (this.Thirst > 0) this.Thirst--;
                if (this.Thirst <= 0) {
                    var health = game.getEntityHealth(playerId);
                    game.setEntityHealth(playerId, health - 25);
                    if (health <= -1) {
                        alt.emit("Display_Help", "Vous êtes mort de faim!", 10000);
                        this.Thirst = 100;
                    }
                    this._RefreshThirst = Date.now();

                } else {
                    this._RefreshThirst = Date.now() + 1000 * 60 * 3;
                }
            }
            alt.emitServer("UpdateHungerThirst", this.Hunger, this.Thirst);
        });
    }

    private UpdateHungerThirst(Thirst: number, Hunger: number) {
        this.Hunger = Hunger;
        this.Thirst = Thirst;
    } 

}