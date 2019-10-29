import * as alt from 'alt';
import * as game from 'natives';

export class Survival {

    public static Hunger: number = 100;
    public static Thirst: number = 100;

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
        Survival.Hunger = hunger;
        Survival.Thirst = thirst;

        this._RefreshHunger = Date.now() + (1000 * 60 * 3) ;
        this._RefreshThirst = Date.now() +( 1000 * 60 * 3 /2 ) ;

        alt.on("UpdateHungerThirst", this.UpdateHungerThirst);
        alt.onServer("UpdateHungerThirst", this.UpdateHungerThirst);

        alt.everyTick(() => {
            if (this.RefreshHunger < Date.now()) {
                if (Survival.Hunger > 0) Survival.Hunger--;
                if (Survival.Hunger <= 0) {
                    var health = game.getEntityHealth(playerId);
                    game.setEntityHealth(playerId, health - 25, 0);
                    if (health <= -1) {
                        alt.emit("Display_Help", "Vous êtes mort de faim!", 10000);
                        Survival.Hunger = 100;
                    }
                    this._RefreshHunger = Date.now();

                } else {
                    this._RefreshHunger = Date.now() + 1000 * 60 * 3 ;
                }
                alt.emitServer("UpdateHungerThirst", Survival.Hunger, Survival.Thirst);
            }

            if (this.RefreshThirst < Date.now()) {
                if (Survival.Thirst > 0) Survival.Thirst--;
                if (Survival.Thirst <= 0) {
                    var health = game.getEntityHealth(playerId);
                    game.setEntityHealth(playerId, health - 25, 0);
                    if (health <= -1) {
                        alt.emit("Display_Help", "Vous êtes mort de faim!", 10000);
                        Survival.Thirst = 100;
                    }
                    this._RefreshThirst = Date.now();

                } else {
                    this._RefreshThirst = Date.now() + 1000 * 60 * 3 / 2 ;
                }
                alt.emitServer("UpdateHungerThirst", Survival.Hunger, Survival.Thirst);
            }
        });
    }

    UpdateHungerThirst = (Hunger: number, Thirst: number) => {
        Survival.Hunger = Hunger;
        Survival.Thirst = Thirst;
    } 

}