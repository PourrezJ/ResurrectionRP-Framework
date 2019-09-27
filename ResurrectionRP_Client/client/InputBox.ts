import * as alt from 'alt';
import * as game from 'natives';

export class InputBox
{
    public inputView: alt.WebView;
    private inputMaxLength: number;
    private inputValue: string;

    public Callback: (text: string) => void;

    constructor(inputMaxLength: number = 25, inputValue: string = "")
    {
        alt.log("open inputbox")
        this.inputMaxLength = inputMaxLength;
        this.inputValue = inputValue;

        this.inputView = new alt.WebView("http://resource/client/cef/userinput/input.html");
        this.inputView.focus();

        alt.emit('canClose', false);
        alt.showCursor(true);
        alt.toggleGameControls(false);

        this.inputView.emit('Input_Data', this.inputMaxLength, this.inputValue);

        this.inputView.on('Input_Submit', (text) => {
            alt.emit('canClose', true);
            alt.toggleGameControls(true);
            this.inputView.destroy();
            this.Callback(text);
        });
    }
}