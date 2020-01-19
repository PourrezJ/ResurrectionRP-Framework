import * as alt from 'alt-client';
import * as game from 'natives';
import * as ui from '../Helpers/UiHelper';

export class Dock
{
    private text: string = "";
    private ontick: number;

    constructor() {
        alt.log("dock init");

        alt.onServer("InitDockOrder", () => {
            alt.log("load dock menu");
            this.text = "";
            this.ontick = alt.everyTick(this.onTick.bind(this));
        });

        alt.onServer("UpdateDockOrder", (itemPrice: number, itemTotal: number, orderPrice: number) => {
            alt.log("load dock menu");
            this.text = `Prix Item : $${itemPrice.toString()}\nTotal Items : $${itemTotal.toString()}\nTotal Panier : $${orderPrice.toString()}`;
        });

        alt.onServer("EndDockOrder", () => {
            alt.clearEveryTick(this.ontick);
        });
    }

    onTick() {
        ui.DrawText2d(this.text, 0.2, 0.2, 2, 1, 255, 255, 255, 255, true, true, 99);
    }
}