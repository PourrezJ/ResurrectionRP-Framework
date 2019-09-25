import * as alt from 'alt';

let xmenuData = null;
let inputMenu = null;
let browser = null;
let inputselected = "";
let callbackTime = Date.now();

export function init()
{
    alt.onServer('XMenuManager_OpenMenu', (menu) => {
        if (browser !== null)
            closeMenu(false);

        xmenuData = new Array();
        xmenuData = JSON.parse(menu);
        alt.toggleGameControls(false);
        alt.showCursor(true);

        browser = new alt.WebView('http://resource/client/cef/xtrem/playerMenu.html?params=' + menu);
        browser.focus();

        browser.on('XMenuManager_Callback', (index) =>
        {
            const time = Date.now() - callbackTime;

            if (time < 100) {
                alt.logWarning('Double call XMenuManager_Callback: ' + time + 'ms');
                return;
            }
                
            callbackTime = Date.now();
            let menuItem = xmenuData.Items[index];

            if (menuItem.InputMaxLength > 0) {
                if (menuItem.InputValue === null) {
                    menuItem.InputValue = "";
                }

                let inputView = new alt.WebView("http://resource/client/cef/userinput/input.html");
                inputView.focus();
                alt.showCursor(true);
                alt.toggleGameControls(false);

                inputView.emit('Input_Data', menuItem.InputMaxLength, menuItem.InputValue);

                inputView.on('Input_Submit', (text) => {
                    xmenuData.Items[index].InputValue = text;
                    alt.emitServer("XMenuManager_ExecuteCallback", index, JSON.stringify(xmenuData));
                    xmenuData = null;
                    inputView.destroy();
                    browser.destroy();
                    browser = null;
                    alt.showCursor(false);
                    alt.toggleGameControls(true);
                });   
            }
            else {
                alt.emitServer("XMenuManager_ExecuteCallback", index, JSON.stringify(xmenuData));
            }

            closeMenu();
        });
    });

    alt.onServer('XMenuManager_CloseMenu', (menu) => {
        alt.log("XMenuManager_CloseMenu");
        closeMenu();
    });
}

function closeMenu(enableControls: boolean = true) {
    if (browser != null) {
        browser.destroy();
        browser = null;
    }

    if (enableControls) {
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
