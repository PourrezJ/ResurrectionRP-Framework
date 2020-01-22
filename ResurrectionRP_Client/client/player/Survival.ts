import * as alt from 'alt-client';
import * as game from 'natives';

export class Survival {

    public static Hunger: number = 100;
    public static Thirst: number = 100;

    constructor(hunger: number, thirst: number) {
        Survival.Hunger = hunger;
        Survival.Thirst = thirst;

        alt.on("UpdateHungerThirst", this.UpdateHungerThirst.bind(this));
        alt.onServer("UpdateHungerThirst", this.UpdateHungerThirst.bind(this));
    }

    UpdateHungerThirst = (hunger: number, thirst: number) => {
        Survival.Hunger = hunger;
        Survival.Thirst = thirst;
    } 

}