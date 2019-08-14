import * as alt from 'alt';
import * as game from 'natives';
import * as chat from 'client/chat/chat';

let inLogin = false;

export function init() {
    alt.on('OpenLogin', (args: any[]) => {
        try {
            let social = game.getSocialclubNickname();
            game.setPlayerInvincible(game.playerId(), true);
            game.displayRadar(false);
            game.displayHud(false);
            alt.emit('toggleChat');
            alt.toggleGameControls(false);
            alt.showCursor(true);
            inLogin = true;
            
            let browser = new alt.WebView('http://resources/resurrectionrp/client/cef/login/index.html')
            browser.emit('callEvent', social);
            browser.focus();
        } catch(ex) {
            alt.log(ex);
        }
    });

}