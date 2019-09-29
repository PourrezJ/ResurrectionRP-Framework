import * as alt from 'alt';
import * as game from 'natives';
import * as utils from '../Utils/Utils'


export class Effects
{
    constructor() {
        alt.onServer("AlcoholDrink", this.AlcoholDrink.bind);
    }

    private AlcoholDrink() : void {

        
    }
}