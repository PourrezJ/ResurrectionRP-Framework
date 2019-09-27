import * as alt from 'alt';
import * as game from 'natives';

export class InputBox
{
    private inputView: alt.WebView;
    private inputMaxLength: number;
    private inputValue: string;

    public Callback: (text: string) => void;

    constructor(inputMaxLength: number = 25, inputValue: string = "")
    {
        this.inputMaxLength = inputMaxLength;
        this.inputValue = inputValue;

        this.inputView = new alt.WebView("http://resource/client/cef/userinput/input.html");
        this.inputView.focus();
        alt.showCursor(true);
        alt.toggleGameControls(false);
        //Interaction.Interaction.SetCanClose(false);

        this.inputView.emit('Input_Data', this.inputMaxLength, this.inputValue);

        this.inputView.on('Input_Submit', (text) => {
            this.inputView.destroy();
            //Interaction.Interaction.SetCanClose(true);
            this.Callback(text);
        });
    }
}