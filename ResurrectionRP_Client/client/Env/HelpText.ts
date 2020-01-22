import * as game from 'natives';

export class HelpText {
    text: string;
    time: number;
    static helpText: HelpText = null;

    constructor(text, time) {
        this.text = text;
        this.time = Date.now() + time;
        HelpText.helpText = this
    }

    Draw() {
        if (this.time < Date.now()) {
            HelpText.helpText = null;
        }

        game.beginTextCommandDisplayHelp('STRING');
        game.addTextComponentSubstringPlayerName(this.text);
        game.endTextCommandDisplayHelp(0, false, true, 0);
    }
}
