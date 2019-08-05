import * as alt from 'alt';
import * as game from 'natives';

export class Chat {
    private _loaded: boolean;
    private _opened: boolean;
    private _hidden: boolean;
    private _buffer: Array<string>;
    private _view: alt.WebView;

    constructor() {
        this._loaded = false;
        this._opened = false;
        this._hidden = false;
        this._buffer = new Array<string>();
        this._view = new alt.WebView('http://resources/resurrectionrp/client/cef/chat/index.html');

        this._view.on('chatloaded', () => {
            for (const msg of this._buffer) {
                this.addMessage(msg);
            }

            this._loaded = true;
        });

        this._view.on('chatmessage', (text) => {
            alt.emitServer('chatmessage', text);

            if (text !== undefined && text.length >= 1)
                alt.emit('messageSent', text);

            this._opened = false;
            alt.emit('chatClosed');
            alt.toggleGameControls(true);
        });

        alt.onServer('chatmessage', (msg) => this.pushMessage(msg));

        alt.on('keyup', (key) => {
            if (!this._loaded)
                return;
            
            if (!this._opened && key === 0x54 && alt.gameControlsEnabled()) {
                this._opened = true;
                this._view.emit('openChat', false);
                alt.emit('chatOpened');
                alt.toggleGameControls(false);
            }
            else if (!this._opened && key === 0xBF && alt.gameControlsEnabled()) {
                this._opened = true;
                this._view.emit('openChat', true);
                alt.emit('chatOpened');
                alt.toggleGameControls(false);
            }
            else if (this._opened && key == 0x1B) {
                this._opened = false;
                this._view.emit('closeChat');
                alt.emit('chatClosed');
                alt.toggleGameControls(true);
            }

            if (key == 0x76) {
                this._hidden = !this._hidden;
                game.displayHud(!this._hidden);
                game.displayRadar(!this._hidden);
                this._view.emit('hideChat', this._hidden);
            }
        });
    }

    addMessage = (text: string) : void => {
        this._view.emit('addString', text);
    }

    pushMessage(text: string): void {
        if (!this._loaded) {
            this._buffer.push(text);
        } else {
            this.addMessage(text);
        }
    }

    get hidden(): boolean {
        return this._hidden;
    }

    get opened(): boolean {
        return this._opened;
    }
}
