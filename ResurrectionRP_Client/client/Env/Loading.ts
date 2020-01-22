import * as alt from 'alt-client';
import * as game from 'natives';

export class Loading {
    text: string;
    type: any;
    toggled: boolean;
    time: number;

    static loading: Loading = null;

    constructor(text, time, type, toggled) {
        this.text = text;
        this.type = type;
        this.toggled = toggled;

        if (time !== null && time !== undefined) {
            this.time = Date.now() + time;
        }

        Loading.loading = this;
        game.busyspinnerOff();
        game.beginTextCommandBusyspinnerOn('STRING');
        game.addTextComponentSubstringPlayerName(this.text);
        game.endTextCommandBusyspinnerOn(this.type);
    }

    Draw() {
        if (this.time < Date.now()) {
            Loading.loading = undefined;
            game.busyspinnerOff();
        }

        if (this.toggled !== null && this.toggled !== undefined && !this.toggled) {
            Loading.loading = null;
            game.busyspinnerOff();
        }
    }
}