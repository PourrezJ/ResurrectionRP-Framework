import * as alt from 'alt';
import * as game from 'natives';

let xmenuData = null;
let inputMenu = null;
let browser = null;
let inputselected = "";

export function init() {
    alt.onServer('XMenuManager_OpenMenu', (menu) => {

        if (browser != null)
            closeMenu();

        xmenuData = new Array();
        xmenuData = JSON.parse(menu);
        alt.toggleGameControls(false);
        alt.showCursor(true);

        browser = new alt.WebView('http://resources/resurrectionrp/client/cef/xtrem/playerMenu.html?params=' + menu);
        browser.focus();

        browser.on('XMenuManager_Callback', (index) =>
        {
            let menuItem = xmenuData.Items[index];

            if (menuItem.InputMaxLength > 0) {
                if (menuItem.InputValue === null) {
                    menuItem.InputValue = "";
                }
                /*
                inputOpen = true;
                inputMenu = mp.browsers.new('package://Resurrection/cef/userinput/index.html');
                inputselected = menuItem;
                mp.gui.chat.activate(false);
                mp.game.controls.disableAllControlActions(0);
                if (xtremMenu.xmenu !== undefined) {
                    xtremMenu.xmenu.destroy();
                    xtremMenu.xmenu = undefined;
                }*/
            }
            else {
                alt.emitServer("XMenuManager_ExecuteCallback", index, JSON.stringify(xmenuData));
            }  
            closeMenu();
        });
    });
}

function closeMenu() {
    if (browser != null) {
        browser.destroy();
        alt.toggleGameControls(true);
        alt.showCursor(false);
    }
}

function getIndexOfMenuItem(menuItem) {
    for (let i = 0; i < xmenuData.Items.length; i++) {
        if (xmenuData.Items[i].Text === menuItem.Text) {
            return i;
        }
    }

    return -1;
}
