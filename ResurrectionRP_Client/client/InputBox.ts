﻿import * as alt from 'alt-client';
import * as game from 'natives';

export class InputBox
{
    private inputView: alt.WebView;

    public Callback: (text: string) => void;

    constructor(inputMaxLength: number = 25, inputValue: string = "")
    {
        alt.log("open inputbox")
        inputMaxLength = inputMaxLength;
        inputValue = inputValue;

        this.inputView = new alt.WebView("http://resource/client/cef/userinput/input.html", true);
        this.inputView.focus();

        alt.emit('canClose', false);
        alt.showCursor(true);
        alt.toggleGameControls(false);

        this.inputView.emit('Input_Data', inputMaxLength, inputValue);
   
        this.inputView.on('Input_Submit', (text) => {
            alt.emit('canClose', true);
            //alt.toggleGameControls(true);
            if (this.inputView != null)
                this.inputView.destroy();
            this.Callback(text);
        });
    }

    public destroy() {
        if (this.inputView != null) {
            this.inputView.destroy();
            this.inputView = null;
        }
    }
}