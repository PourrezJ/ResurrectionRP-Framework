import * as alt from 'alt';
import * as NativeUI from '../NativeUIMenu/NativeUI.js';
import * as chat from '../chat/chat';
import * as game from 'natives';

enum InputType {
    Text,
    Number,
    UNumber,
    Float,
    UFloat
}

enum MenuAnchor {
    TopLeft = 0,
    TopCenter = 1,
    TopRight = 2,
    MiddleLeft = 3,
    MiddleCenter = 4,
    MiddleRight = 6,
    BottomLeft = 7,
    BottomCenter = 8,
    BottomRight = 9,
}

var menu = null;
var menuItems = null;
var menuData = null;
var backPressed = false;
var exitEscapeMenu = false;
var newMenu = true;

var inputView = null;
var inputItem = null;
var inputIndex = -1;

export default () => {
    alt.onServer('MenuManager_OpenMenu', (data) => {
        menuItems = new Array();
        menuData = JSON.parse(data);

        if (menuData.Id == undefined) {
            menuData.Id = "";
        }

        if (menuData.Title == undefined || menuData.Title == "") {
            menuData.Title = " ";
        }

        if (menuData.SubTitle == undefined || menuData.SubTitle == "") {
            menuData.SubTitle = " ";
        }

        if (menu != null) {
            newMenu = false;
            menu.AUDIO_BACK = "";
            menu.Close();
        } else {
            newMenu = true;
        }

        let pos = null;

        const screen = game.getActiveScreenResolution(0, 0);
        let width = 1080 * screen[1] / screen[2] - 431;

        if (menuData.Anchor == MenuAnchor.TopLeft) {
            pos = new NativeUI.Point(menuData.PosX, menuData.PosY);
        } else if (menuData.Anchor == MenuAnchor.TopCenter) {
            pos = new NativeUI.Point(menuData.PosX + width / 2, menuData.PosY);
        } else if (menuData.Anchor == MenuAnchor.TopRight) {
            pos = new NativeUI.Point(menuData.PosX + width, menuData.PosY);
        } else if (menuData.Anchor == MenuAnchor.MiddleLeft) {
            pos = new NativeUI.Point(menuData.PosX, menuData.PosY + 260);
        } else if (menuData.Anchor == MenuAnchor.MiddleCenter) {
            pos = new NativeUI.Point(menuData.PosX + width / 2, menuData.PosY + 260);
        } else if (menuData.Anchor == MenuAnchor.MiddleRight) {
            pos = new NativeUI.Point(menuData.PosX + width, menuData.PosY + 260);
        } else if (menuData.Anchor == MenuAnchor.BottomLeft) {
            pos = new NativeUI.Point(menuData.PosX, menuData.PosY + 520);
        } else if (menuData.Anchor == MenuAnchor.BottomCenter) {
            pos = new NativeUI.Point(menuData.PosX + width / 2, menuData.PosY + 520);
        } else if (menuData.Anchor == MenuAnchor.BottomRight) {
            pos = new NativeUI.Point(menuData.PosX + width, menuData.PosY + 520);
        }

        if (menuData.BannerSprite != undefined) {
            menu = new NativeUI.Menu(menuData.Title, menuData.SubTitle, pos, undefined, menuData.BannerSprite.Dict, menuData.BannerSprite.Name);
        } else {
            if (menuData.BannerColor != undefined) {
                let color = new NativeUI.Color(menuData.BannerColor.red, menuData.BannerColor.green, menuData.BannerColor.blue, menuData.BannerColor.alpha);
                menu = new NativeUI.Menu(menuData.Title, menuData.SubTitle, pos, color);
            } else {
                menu = new NativeUI.Menu(menuData.Title, menuData.SubTitle, pos);
            }
        }

        if (!newMenu) {
            menu.AUDIO_BACK = "";
        }

        for (let i = 0; i < menuData.Items.length; i++) {
            let item = menuData.Items[i];
            let menuItem;

            if (item.Id == undefined) {
                item.Id = '';
            }

            if (item.Text == undefined) {
                item.Text = '';
            }

            if (item.Description == undefined) {
                item.Description = '';
            }

            if (item.Type == 0 || item.Type == 2) {
                if (item.Type == 2) {
                    let background = hexToRgb(item.BackgroundColor);
                    let hightlight = hexToRgb(item.HighlightColor);
                    menuItem = new NativeUI.UIMenuItem(item.Text, item.Description, new NativeUI.Color(background.r, background.g, background.b), new NativeUI.Color(hightlight.r, hightlight.g, hightlight.b));
                } else {
                    menuItem = new NativeUI.UIMenuItem(item.Text, item.Description);
                }

                if (item.RightBadge != undefined) {
                    menuItem.SetRightBadge(eval(item.RightBadge));
                }

                if (item.RightLabel != undefined) {
                    menuItem.SetRightLabel(item.RightLabel);
                }
            }
            else if (item.Type == 1) {
                menuItem = new NativeUI.UIMenuCheckboxItem(item.Text, item.Checked, item.Description)
            }
            else if (item.Type == 3) {
                let listItems = [];

                for (let j = 0; j < item.Items.length; j++) {
                    listItems.push(item.Items[j]);
                }

                menuItem = new NativeUI.UIMenuListItem(item.Text, item.Description, new NativeUI.ItemsCollection(listItems), item.SelectedItem);
            }

            if (item.LeftBadge != undefined) {
                menuItem.SetLeftBadge(eval(item.LeftBadge));
            }

            menu.AddItem(menuItem);
            menuItems[i] = menuItem;
        }

        menu.IndexChange.on((index) => {
            if (menuData.OnIndexChange != undefined) {
                eval(menuData.OnIndexChange);
            }

            if (menuData.CallbackOnIndexChange) {
                alt.emitServer('MenuManager_IndexChanged', index);
            }
        });

        menu.CheckboxChange.on((item, checked) => {
            let index = getIndexOfMenuItem(item);
            let menuItem = menuData.Items[index];
            menuItem.Checked = checked;

            if (menuData.OnCheckboxChange != undefined) {
                eval(menuData.OnCheckboxChange);
            }

            if (menuItem.ExecuteCallback) {
                let data = saveData();
                alt.emitServer('MenuManager_ExecuteCallback', index, false, JSON.stringify(data));
            }
        });

        menu.ListChange.on((item, index) => {
            let itemIndex = getIndexOfMenuItem(item);
            let listItem = menuData.Items[itemIndex];
            listItem.SelectedItem = index;

            if (listItem.OnListChange != undefined) {
                eval(menuData.OnListChange);
            }

            if (listItem.ExecuteCallbackListChange) {
                alt.emitServer('MenuManager_ListChanged', itemIndex, index);
            }
        });

        menu.ItemSelect.on((item, index) => {
            if (inputView != null)
                return;

            let menuItem = menuData.Items[index];

            if (menuItem.InputMaxLength > 0) {
                if (menuItem.InputValue == undefined) {
                    menuItem.InputValue = "";
                }

                inputIndex = index;
                inputItem = item;

                inputView = new alt.WebView("http://resource/client/cef/userinput/input.html");
                inputView.focus();
                alt.showCursor(true);
                alt.toggleGameControls(false);
                alt.emit("canClose", false);
                inputView.emit('Input_Data', menuItem.InputMaxLength, menuItem.InputValue);

                inputView.on('Input_Submit', (text) => {
                    saveInput(text);
                    alt.emit("canClose", true);
                    alt.showCursor(false);
                });
            }

            if (inputView == null && menuData.OnItemSelect != undefined) {
                eval(menuData.OnItemSelect);
            }

            if (inputView == null && menuItem.ExecuteCallback) {
                let data = saveData();
                alt.emitServer('MenuManager_ExecuteCallback', index, false, JSON.stringify(data));
            }
        });

        menu.MenuClose.on(() => {
            if (backPressed == false) {
                return;
            }

            backPressed = false;
            if (menuData.NoExit) {
                menu.Visible = true;
            } else if (menuData.BackCloseMenu) {
                CloseMenu();
                alt.emitServer('MenuManager_ClosedMenu');
            } else {
                menu.Visible = true;

                if (menuData.OnBackKey != undefined) {
                    eval(menuData.OnBackKey);
                }

                alt.emitServer('MenuManager_BackKey');
            }
        });

        if (menuData.SelectedIndex == -1)
            menuData.SelectedIndex = 0;

        menu.CurrentSelection = menuData.SelectedIndex;

        if (menuData.OnMenuOpen) {
            eval(menuData.OnMenuOpen);
        }

        menu.Open();

        if (!newMenu) {
            menu.AUDIO_BACK = 'BACK';
        }

        alt.emit('MenuManager_MenuOpened');
    });

    alt.onServer('MenuManager_ForceCallback', () => {
        let data = saveData();
        alt.emitServer("MenuManager_ExecuteCallback", menu.CurrentSelection, true, JSON.stringify(data));
    });

    alt.onServer('MenuManager_CloseMenu', () => {
        CloseMenu();
    });
};

export function hasMenuOpen() {
    return menu != null;
}

function CloseMenu() {
    if (menu != null) {
        if (menuData.OnMenuClose != undefined) {
            eval(menuData.OnMenuClose);
        }

        menu.Close();
        menu = null;
        alt.emit('MenuManager_MenuClosed');
    }
}

function saveInput(inputText) {
    if (inputItem != null) {
        let menuItem = menuData.Items[inputIndex];
        let valid = true;

        if (menuItem.InputType == InputType.Number) {
            inputText = inputText.trim();

            if (inputText.length != 0) {
                let con_inputText = parseInt(inputText);

                if (isNaN(con_inputText)) {
                    valid = false;
                } else {
                    inputText = inputText.toString();
                }
            }
        } else if (menuItem.InputType == InputType.UNumber) {
            inputText = inputText.trim();

            if (inputText.length != 0) {
                let con_inputText = parseInt(inputText);

                if (isNaN(con_inputText) || con_inputText < 0) {
                    valid = false;
                } else {
                    inputText = inputText.toString();
                }
            }
        } else if (menuItem.InputType == InputType.Float) {
            inputText = inputText.trim();

            if (inputText.length != 0) {
                let con_inputText = parseFloat(inputText);

                if (isNaN(con_inputText)) {
                    valid = false;
                } else {
                    inputText = inputText.toString();
                }
            }
        } else if (menuItem.InputType == InputType.UFloat) {
            inputText = inputText.trim();

            if (inputText.length != 0) {
                let con_inputText = parseFloat(inputText);

                if (isNaN(con_inputText) || con_inputText < 0) {
                    valid = false;
                } else {
                    inputText = inputText.toString();
                }
            }
        }

        if (valid) {
            menuItem.InputValue = inputText;

            if (menuItem.InputSetRightLabel) {
                inputItem.SetRightLabel(inputText);
            }
        }

        if (menuData.OnItemSelect != undefined) {
            eval(menuData.OnItemSelect);
        }

        if (menuItem.ExecuteCallback) {
            let data = saveData();
            alt.emitServer('MenuManager_ExecuteCallback', inputIndex, false, JSON.stringify(data));
        }

        inputItem = null;
        inputIndex = -1;
        alt.showCursor(false);
        alt.toggleGameControls(true);

        inputView.destroy();
        alt.setTimeout(resetInputView, 1000);
    }
}

function resetInputView() {
    inputView = null;
}

function getIndexOfMenuItem(menuItem) {
    for (let i = 0; i < menuItems.length; i++) {
        if (menuItems[i] == menuItem) {
            return i;
        }
    }

    return -1;
}

function saveData() {
    var data = new Object();

    for (let i = 0; i < menuData.Items.length; i++) {
        var menuItem = menuData.Items[i];

        if (menuItem.Type == 1) {
            data[menuItem.Id] = menuItem.Checked;
        } else if (menuItem.Type == 3) {
            data[menuItem.Id] = new Object();
            data[menuItem.Id]['Index'] = menuItem.SelectedItem;
            data[menuItem.Id]['Value'] = menuItem.Items[menuItem.SelectedItem];
        } else if (menuItem.InputMaxLength > 0 && menuItem.InputValue != undefined && menuItem.InputValue.length > 0) {
            data[menuItem.Id] = menuItem.InputValue;
        }
    }

    return data;
}

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

alt.on('keydown', (e) => {
    if (menu != null && e == 8) {
        backPressed = true;
    }
    else if (menu != null && e == 27) {
        exitEscapeMenu = true;
    }
});

alt.on('keyup', (e) => {
    if (menu != null && exitEscapeMenu == true && e == 27) {
        exitEscapeMenu = false;
        menu.Visible = true;
    }
});
