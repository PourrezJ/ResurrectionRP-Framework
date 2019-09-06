import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';
import * as utils from 'client/Utils/Utils';

export class RadioManager
{
    private view: alt.WebView;
    private favoris: object;
    private frequence: number;
    private lastcheck: number;
    private pressed: boolean;
    private status: RadioModes;

    constructor()
    {
        alt.onServer('OpenRadio', (favoris: string, frequence: number, status: RadioModes) =>
        {
            if (chat.isOpened() || game.isPauseMenuActive())
                return;

            this.favoris = JSON.parse(favoris);
            this.frequence = frequence;
            this.status = status;

            if (this.view == null) {
                this.view = new alt.WebView("http://resource/client/cef/radio/index.html");       
            }
            else
                this.view.emit('unhide');

            this.view.focus();
            alt.showCursor(true);
            
            /*
             * Events
             */
            this.view.on('ChangeFrequence', (frequence: number) => {
                alt.emitServer('RadioManager','ChangeFrequence', frequence);
                game.playSoundFrontend(-1, "Start_Squelch", "CB_RADIO_SFX", true);
            });

            this.view.on('SaveFrequence', (channel: number, frequence: number) => {
                alt.emitServer('RadioManager', 'SaveFrequence', channel, frequence);
            });
   
            this.view.on('GetFavoris', () => {
                alt.setTimeout(() => {
                    this.view.emit('loadFavoris', this.favoris, (this.status == RadioModes.OFF) ? null : this.frequence);
                }, 100);
            });

            this.view.on('RadioOnOff', (on: boolean) => {
                this.status = on ? RadioModes.LISTENING : RadioModes.OFF;
                alt.emitServer('RadioManager', 'OnOff', on);
            });
        });

        alt.onServer('HideRadio', (favoris: string, frequence: number) => {
            if (this.view == null)
                return;

            this.view.emit('hide');
            this.view.unfocus();
            alt.showCursor(false);
            alt.toggleGameControls(true);
        });

        alt.onServer('CloseRadio', (favoris: string, frequence: number) => {
            if (this.view == null)
                return;

            this.CloseRadio();
        });

        alt.on("update", () =>
        {
            //if (this.lastcheck > alt.getMsPerGameMinute()) {

            //}
            if (this.view != null)
            {
                utils.DisEnableControls(false);
            }
        });
    }

    public CloseRadio() {
        if (this.view == null)
            return;
        this.view.destroy();
        this.view = null;

        alt.toggleGameControls(true);
        alt.showCursor(false);
        alt.emit("toggleChatAdminRank");
    }
}

enum RadioModes {
    OFF = 0,
    LISTENING = 1,
    SPEAKING = 2,
}