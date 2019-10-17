import * as alt from 'alt';
import * as game from 'natives';
import * as chat from '../chat/chat';
import * as utils from '../Utils/Utils';
import { VoiceChat } from '../Voice/VoiceChat';

export class RadioManager {

    private view: alt.WebView;
    private favoris: object;
    private channel: number;
    private status: RadioModes;
    private volume: number = 10;
    private muted: boolean = false;
    private callbackTime: number = Date.now(); 

    constructor() {
        alt.onServer('OpenRadio', (favoris: string, channel: number, status: RadioModes, volume: number) =>
        {
            if (this.isOpen() || chat.isOpened() || game.isPauseMenuActive())
                return;

            this.favoris = JSON.parse(favoris);
            this.channel = channel;
            this.status = status;
            this.volume = volume;
            this.muted = false;

            if (this.view == null) {
                this.view = new alt.WebView("http://resource/client/cef/radio/index.html");
            } else {
                this.view.emit('unhide');
            }

            this.view.focus();
            alt.showCursor(true);  
            
            /*
             * Events
             */
            this.view.on('ChangeChannel', (channel: number) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                this.channel = channel;
                game.playSoundFrontend(-1, "Start_Squelch", "CB_RADIO_SFX", true);
                alt.emitServer('RadioManager', 'ChangeChannel', channel);
            });

            this.view.on('SaveFrequence', (channel: number, frequence: number) => {
                alt.emitServer('RadioManager', 'ChangeChannel', channel, frequence);
            });

            this.view.on('volumeUP', () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                VoiceChat.radioVolume = (this.volume + 1 > 10) ? 10 : this.volume + 1;
                this.volume = VoiceChat.radioVolume;
                alt.emitServer('RadioManager', 'ChangeVolume', VoiceChat.radioVolume);
                alt.emit("Display_Help", "Volume: " + this.volume, 5000);
            });

            this.view.on('volumeDown', () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                VoiceChat.radioVolume = (this.volume - 1 < 0) ? 0 : this.volume - 1 ;
                this.volume = VoiceChat.radioVolume;
                alt.emit("Display_Help", "Volume: " + this.volume, 5000);
                alt.emitServer("RadioManager", 'ChangeVolume', VoiceChat.radioVolume);
            });

            this.view.on('volumeMuted', () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                VoiceChat.radioVolume = (this.muted) ? 10 : 0;
                this.volume = VoiceChat.radioVolume;
                alt.emit("Display_Help", "Volume: " + this.volume, 5000);
                alt.emitServer("RadioManager", 'ChangeVolume', VoiceChat.radioVolume);
                this.muted = !this.muted;
            })
   
            this.view.on('initialize', () => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                alt.setTimeout(() => {
                    this.view.emit('initialize', this.favoris, (this.status == RadioModes.OFF) ? null : this.channel);
                }, 100);
            });

            this.view.on('RadioOnOff', (on: boolean) => {
                if (this.CheckMultipleCallbak()) {
                    return;
                }

                this.status = on ? RadioModes.LISTENING : RadioModes.OFF;

                if (this.status == RadioModes.LISTENING) {
                    this.view.emit('setChannel', this.channel);
                    game.playSoundFrontend(-1, "Start_Squelch", "CB_RADIO_SFX", true);
                }

                alt.emitServer('RadioManager', 'OnOff', on);
            });
        });

        alt.onServer('CloseRadio', () => {
            if (this.view == null)
                return;

            this.CloseRadio();
        });

        alt.everyTick(this.onTick.bind(this));
    }

    private CheckMultipleCallbak() {
        const time = Date.now() - this.callbackTime;

        if (time < 100) {
            alt.logWarning('Phone multiple callback: ' + time + 'ms');
            return true;
        }

        this.callbackTime = Date.now();
        return false;
    }

    public isOpen() {
        return this.view != null;
    }

    public onTick() {
        if (this.view != null)
            utils.DisEnableControls(false);
    }

    public CloseRadio() {
        if (this.view == null)
            return;

        this.view.unfocus();
        this.view.destroy();
        this.view = null;
        alt.showCursor(false);
        alt.toggleGameControls(true);
        chat.hide(false);
    }
}

enum RadioModes {
    OFF = 0,
    LISTENING = 1,
    SPEAKING = 2,
}