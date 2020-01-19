import * as alt from 'alt-client';
import * as game from 'natives';
import * as enums from '../Utils/Enums/Enums';

let loaded = false;
let opened = false;
let hidden = false;
let buffer = new Array<string>();
let view = new alt.WebView('http://resource/client/cef/chat/index.html', true);

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

    alt.onServer('ChatMessage', pushMessage);

    alt.onServer('EmptyChat', () => {
        view.emit('emptyChat');
    });

    alt.on('keyup', (key) => {
        if (!loaded || hidden)
            return;
        
        const rank: enums.AdminRank = alt.Player.local.getMeta("LevelRank");

        if (rank == null || rank == enums.AdminRank.Player)
            return;

        if (!opened && (key == 0x54 || key == 0xBF) && alt.gameControlsEnabled()) {
            opened = true;
            view.emit('openChat', false);
            alt.emit('chatOpened');
            alt.toggleGameControls(false);
        }
        else if (opened && key == 0x1B) {
            opened = false;
            view.emit('closeChat');
            alt.emit('chatClosed');
            alt.toggleGameControls(true);
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

export function isHidden() {
    return hidden;
}

export function hide(hide: boolean) {
    hidden = hide;
    view.emit('hideChat', hidden);
}

export function isOpened(): boolean {
    return opened;
}
