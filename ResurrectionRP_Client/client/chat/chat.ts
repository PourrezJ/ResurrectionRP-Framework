import * as alt from 'alt';
import * as game from 'natives';

let loaded = false;
let opened = false;
let hidden = false;
let buffer = new Array<string>();
let view = new alt.WebView('http://resources/resurrectionrp/client/cef/chat/index.html');

function addMessage(text: string): void {
    view.emit('addString', text);
}

export function initialize() {
    view.on('chatLoaded', () => {
        for (const msg of buffer) {
            addMessage(msg);
        }

        loaded = true;
    });

    view.on('chatmessage', (text) => {
        alt.emitServer('chatmessage', text);

        if (text !== undefined && text.length >= 1)
            alt.emit('messageSent', text);

        opened = false;
        alt.emit('chatClosed');
        alt.toggleGameControls(true);
    });

    alt.onServer('chatmessage', pushMessage);

    alt.on('toggleChat', (state: boolean = null) => {
        if (!opened && state == null || state ) {
            hidden = state == null ? !hidden : state;
            view.emit('hideChat', state == null ? hidden : state);
        } else {
            hidden = !hidden;
            view.emit('hideChat', hidden);
        }
    });

    alt.on("isChatOpen", () => {
        return !hidden
    });

    alt.on('keyup', (key) => {
        if (!loaded)
            return;

        if (!opened && key === 0x54 && alt.gameControlsEnabled()) {
            opened = true;
            view.emit('openChat', false);
            alt.emit('chatOpened');
            alt.toggleGameControls(false);
        }
        else if (!opened && key === 0xBF && alt.gameControlsEnabled()) {
            opened = true;
            view.emit('openChat', true);
            alt.emit('chatOpened');
            alt.toggleGameControls(false);
        }
        else if (opened && key == 0x1B) {
            opened = false;
            view.emit('closeChat');
            alt.emit('chatClosed');
            alt.toggleGameControls(true);
        }

        if (key == 0x76) {
            hidden = !hidden;
            game.displayHud(!hidden);
            game.displayRadar(!hidden);
            view.emit('hideChat', hidden);
        }
    });
}

export function pushMessage(text: string): void {
    if (!loaded) {
        buffer.push(text);
    } else {
        addMessage(text);
    }
}

export function isHidden(): boolean {
    return hidden;
}

export function isOpened(): boolean {
    return opened;
}
