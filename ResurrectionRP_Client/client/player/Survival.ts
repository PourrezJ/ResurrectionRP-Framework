import * as alt from 'alt-client';
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


        alt.on("UpdateHungerThirst", this.UpdateHungerThirst);
        alt.onServer("UpdateHungerThirst", this.UpdateHungerThirst);

    }

    UpdateHungerThirst = (Hunger: number, Thirst: number) => {
        Survival.Hunger = Hunger;
        Survival.Thirst = Thirst;
    } 

}