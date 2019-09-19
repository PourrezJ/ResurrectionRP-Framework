import * as alt from 'alt';
import * as game from 'natives';
import * as chat from '../chat/chat';
import * as utils from '../Utils/Utils';
import { VoiceChat } from '../Voice/VoiceChat';

export class RadioManager {

    private view: alt.WebView;
    private favoris: object;
    private frequence: number;
    private lastcheck: number;
    private pressed: boolean;
    private status: RadioModes;
    private volume: number = 10;
    private muted: boolean = false;
    private tick: number;

    constructor() {
        alt.onServer('OpenRadio', (favoris: string, frequence: number, status: RadioModes, volume: number) =>
        {
            if (chat.isOpened() || game.isPauseMenuActive())
                return;

            this.favoris = JSON.parse(favoris);
            this.frequence = frequence;
            this.status = status;
            this.volume = volume;
            this.muted = false;

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

            this.view.on('volumeUP', () => {
                VoiceChat.radioVolume = (this.volume + 1 > 10) ? 10 : this.volume + 1;
                this.volume = VoiceChat.radioVolume;
                alt.emitServer('RadioManager', 'ChangeVolume', VoiceChat.radioVolume);
                alt.emit("Display_Help", "Volume: " + this.volume, 5000);
            });

            this.view.on('volumeDown', () => {
                VoiceChat.radioVolume = (this.volume - 1 < 0) ? 0 : this.volume - 1 ;
                this.volume = VoiceChat.radioVolume;
                alt.emit("Display_Help", "Volume: " + this.volume, 5000);
                alt.emitServer("RadioManager", 'ChangeVolume', VoiceChat.radioVolume);
            });

            this.view.on('volumeMuted', () => {
                VoiceChat.radioVolume = (this.muted) ? 10 : 0;
                this.volume = VoiceChat.radioVolume;
                alt.emit("Display_Help", "Volume: " + this.volume, 5000);
                alt.emitServer("RadioManager", 'ChangeVolume', VoiceChat.radioVolume);
                this.muted = !this.muted;
            })
   
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

        alt.onServer('CloseRadio', () => {
            if (this.view == null)
                return;
            this.CloseRadio();
        });

        this.tick = alt.everyTick( () =>
        {
            if (this.view != null)
            {
                utils.DisEnableControls(false);
            }
        });
    }

    public CloseRadio() {
        if (this.view == null)
            return;
        this.view.unfocus();
        this.view.destroy();
        this.view = null;
        alt.clearEveryTick(this.tick);
        alt.showCursor(false);
        alt.toggleGameControls(true);
        alt.emit("toggleChatAdminRank");
    }
}

enum RadioModes {
    OFF = 0,
    LISTENING = 1,
    SPEAKING = 2,
}