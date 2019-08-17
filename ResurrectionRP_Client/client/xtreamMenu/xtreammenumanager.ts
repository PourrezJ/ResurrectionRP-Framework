import * as alt from 'alt';
import * as game from 'natives';

let xmenuData;
let inputselected;
let inputMenu;

export function init() {
    alt.onServer('XMenuManager_OpenMenu', (menu) => {
        alt.log(menu);
        xmenuData = menu;

        alt.toggleGameControls(false);
        alt.showCursor(true);
        let browser = new alt.WebView('http://resources/resurrectionrp/client/cef/xtrem/playerMenu.html?params=' + menu);
    });

    alt.onServer('XMenuManager_Callback', (args: any[]) => {
        alt.log("XTreamMenu Callback");
        let id = args[0];
        let index = args[1];
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
            let data = saveData();
            alt.emitServer("XMenuManager_ExecuteCallback", index, JSON.stringify(data));
        }

    });
}

function saveData() {
    let data = {};
    let menuItem;
    for (let i = 0; i < xmenuData.Items.length; i++) {
        menuItem = xmenuData.Items[i];
        if (menuItem.InputMaxLength > 0 && menuItem.InputValue !== undefined && menuItem.InputValue.length > 0) {
            data[menuItem.Id] = menuItem.InputValue;
        }
    }
    return data;
}

function getIndexOfMenuItem(menuItem) {
    for (let i = 0; i < xmenuData.Items.length; i++) {
        if (xmenuData.Items[i].Text === menuItem.Text) {
            return i;
        }
    }

    return -1;
}
