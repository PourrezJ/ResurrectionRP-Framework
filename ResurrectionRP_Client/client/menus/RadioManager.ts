import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';

export class RadioManager
{
    public view: alt.WebView;
    public favoris: object;
    public frequence: number;

    constructor()
    {
        alt.onServer('OpenRadio', (favoris: string, frequence: number) =>
        {
            if (chat.isOpened() || game.isPauseMenuActive())
                return;

            this.favoris = JSON.parse(favoris);
            this.frequence = frequence;

            alt.log(favoris);

            if (this.view == null) {
                this.view = new alt.WebView("http://resources/resurrectionrp/client/cef/radio/index.html"); 
                this.view.emit('loadFavoris', favoris, this.frequence);
            }
            else
                this.view.emit('unhide');

            this.view.focus();
            alt.showCursor(true);
            alt.toggleGameControls(false);

            /*
             * Events
             */
            this.view.on('ConnectFrequence', (frequence: number) => {
                alt.emitServer('TurnON_Radio', frequence);
                game.playSoundFrontend(-1, "Start_Squelch", "CB_RADIO_SFX", true);
            });

            this.view.on('DisconnectFrequence', () => {
                alt.emitServer('TurnOFF_Radio');
                game.playSoundFrontend(-1, "End_Squelch", "CB_RADIO_SFX", true);
            });

            this.view.on('SaveFrequence', (channel: number, frequence: number) => {
                alt.emitServer('RadioManager', 'SaveFrequence', channel, frequence);
            });

            /*
            this.view.on('', () => {

            });

            this.view.on('', () => {

            });*/
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