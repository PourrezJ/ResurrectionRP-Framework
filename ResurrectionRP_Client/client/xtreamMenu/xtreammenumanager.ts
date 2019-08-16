import * as alt from 'alt';
import * as game from 'natives';

let xmenuData;

export function init() {
    alt.onServer('XMenuManager_OpenMenu', (args: any[]) => {
        xmenuData = new Array();
        xmenuData = JSON.parse(args[0]);

        alt.toggleGameControls(false);
        alt.showCursor(true);
        let browser = new alt.WebView('http://resources/resurrectionrp/client/cef/xtrem/playerMenu.html?params=' + args[0]);
    });
}