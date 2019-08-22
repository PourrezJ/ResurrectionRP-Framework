import * as alt from 'alt';
import * as NativeUI from 'client/includes/NativeUIMenu/NativeUI';
/*
NativeUI.Menu menu = null;
var menuItems = null;
var menuData = null;
var backPressed = false;
var exitEscapeMenu = false;
var forceHide = false;
var savedChatStatus = undefined;

export function initialize() {
    alt.onServer('MenuManager_OpenMenu', (jsonData) => {
        menuItems = new Array();
        menuData = JSON.parse(jsonData);

        if (menuData.Id === undefined) {
            menuData.Id = "";
        }
        if (menuData.Title === undefined || menuData.Title === "") {
            menuData.Title = " ";
        }

        if (menuData.SubTitle === undefined || menuData.SubTitle === "") {
            menuData.SubTitle = " ";
        }

        menu = new NativeUI.Menu(menuData.Title, menuData.SubTitle, menuData.PosX, menuData.PosY, menuData.Anchor, menuData.EnableBanner);

        if (menuData.BannerSprite !== undefined) {
            menu. .setMenuBannerSprite(menu, menuData.BannerSprite.Dict, menuData.BannerSprite.Name);
        } else if (menuData.BannerTexture !== undefined) {
            API.setMenuBannerTexture(menu, menuData.BannerTexture);
        } else if (menuData.BannerColor !== undefined) {
            API.setMenuBannerRectangle(menu, menuData.BannerColor.alpha, menuData.BannerColor.red, menuData.BannerColor.green, menuData.BannerColor.blue);
        }

        for (let i = 0; i < menuData.Items.length; i++) {
            let item = menuData.Items[i];
            let menuItem;

            if (item.Id === undefined) {
                item.Id = "";
            }

            if (item.Text === undefined) {
                item.Text = "";
            }

            if (item.Description === undefined) {
                item.Description = "";
            }

            if (item.Type === 0 || item.Type === 2) {

                if (item.Type === 2) {
                    menuItem = API.createColoredItem(item.Text, item.Description, item.BackgroundColor, item.HighlightColor);
                } else {
                    menuItem = API.createMenuItem(item.Text, item.Description);
                }

                if (item.RightBadge !== undefined) {
                    menuItem.SetRightBadge(eval(item.RightBadge));
                }

                if (item.RightLabel !== undefined) {
                    menuItem.SetRightLabel(item.RightLabel);
                }
            }
            else if (item.Type === 1) {
                menuItem = API.createCheckboxItem(item.Text, item.Description, item.Checked);
            }
            else if (item.Type === 3) {
                let listItems = new List(String);

                for (let j = 0; j < item.Items.length; j++) {
                    listItems.Add(item.Items[j]);
                }

                menuItem = API.createListItem(item.Text, item.Description, listItems, item.SelectedItem);
            }

            if (item.LeftBadge !== undefined) {
                menuItem.SetLeftBadge(eval(item.LeftBadge));
            }

            menu.AddItem(menuItem);
            menuItems[i] = menuItem;
        }

        if (menuData.NoExit) {
            menu.ResetKey(menuControl.Back);
        }

        menu.OnIndexChange.connect((sender, index) => {
            if (menuData.OnIndexChange !== undefined) {
                eval(menuData.OnIndexChange);
            }
        });

        menu.OnCheckboxChange.connect((sender, item, checked) => {
            let index = getIndexOfMenuItem(item);
            let menuItem = menuData.Items[index];
            menuItem.Checked = checked;

            if (menuData.OnCheckboxChange !== undefined) {
                eval(menuData.OnCheckboxChange);
            }

            if (menuItem.ExecuteCallback) {
                let data = saveData();
                alt.emitServer('MenuManager_ExecuteCallback', menuData.Id, menuItem.Id, index, false, API.toJson(data));
            }
        });

        menu.OnListChange.connect((sender, item, index) => {
            menuData.Items[getIndexOfMenuItem(item)].SelectedItem = index;

            if (menuData.OnListChange !== undefined) {
                eval(menuData.OnListChange);
            }
        });

        menu.OnItemSelect.connect((sender, item, index) => {
            let menuItem = menuData.Items[index];

            if (menuItem.InputMaxLength > 0) {
                if (menuItem.InputValue === undefined) {
                    menuItem.InputValue = "";
                }

                let input = API.getUserInput(menuItem.InputValue, menuItem.InputMaxLength);
                let valid = true;

                if (menuItem.InputType === 1) {
                    input = input.trim();

                    if (input.length !== 0) {
                        input = parseInt(input);

                        if (isNaN(input)) {
                            valid = false;
                        } else {
                            input = input.toString();
                        }
                    }
                } else if (menuItem.InputType === 2) {
                    input = input.trim();

                    if (input.length !== 0) {
                        input = parseInt(input);

                        if (isNaN(input) || input < 0) {
                            valid = false;
                        } else {
                            input = input.toString();
                        }
                    }
                } else if (menuItem.InputType === 3) {
                    input = input.trim();

                    if (input.length !== 0) {
                        input = parseFloat(input);

                        if (isNaN(input)) {
                            valid = false;
                        } else {
                            input = input.toString();
                        }
                    }
                }

                if (valid) {
                    menuItem.InputValue = input;

                    if (menuItem.InputSetRightLabel) {
                        item.SetRightLabel(input);
                    }
                }
            }

            if (menuData.OnItemSelect !== undefined) {
                eval(menuData.OnItemSelect);
            }

            if (menuItem.ExecuteCallback) {
                let data = saveData();
                alt.emitServer('MenuManager_ExecuteCallback', menuData.Id, menuItem.Id, index, false, API.toJson(data));
            }
        });

        menu.OnMenuClose.connect(() => {
            if (backPressed === false) {
                return;
            }

            if (menuData.BackCloseMenu === false) {
                menu.Visible = true;
            } else {
                API.setCanOpenChat(savedChatStatus);
            }

            if (menuData.OnMenuClose !== undefined) {
                eval(menuData.OnMenuClose);
            }

            backPressed = false;
            alt.emitServer('MenuManager_ClosedMenu');
        });

        if (menuData.SelectedIndex === -1)
            menuData.SelectedIndex = 0;

        menu.CurrentSelection = menuData.SelectedIndex;

        if (menuData.OnMenuOpen !== undefined) {
            eval(menuData.OnMenuOpen);
        }

        savedChatStatus = API.getCanOpenChat();
        API.setCanOpenChat(false);

        if (!forceHide)
            menu.Visible = true;
    });

    alt.onServer('MenuManager_ForceCallback', () => {
        let data = saveData();
        alt.emitServer('MenuManager_ExecuteCallback', menuData.Id, '', menu.CurrentSelection, true, API.toJson(data));
    });

    alt.onServer('MenuManager_CloseMenu', () => {
            menu.Visible = false;
            menu = null;
            API.setCanOpenChat(savedChatStatus);
    });

    /*
    API.onKeyDown.connect(function (sender, e) {
        if (menu !== null && e.KeyCode === Keys.Back) {
            backPressed = true;
        }
        else if (menu !== null && e.KeyCode === Keys.Escape) {
            exitEscapeMenu = true;
        }
    });

    API.onKeyUp.connect(function (sender, e) {
        if (menu !== null && exitEscapeMenu === true && e.KeyCode === Keys.Escape) {
            exitEscapeMenu = false;
            menu.Visible = true;
        }
    });
    */
/*}

function getIndexOfMenuItem(menuItem) {
    for (let i = 0; i < menuItems.length; i++) {
        if (menuItems[i] === menuItem) {
            return i;
        }
    }

    return -1;
}

function hideMenu() {
    forceHide = true;

    if (menu !== null) {
        menu.Visible = false;

        if (savedChatStatus !== undefined) {
            API.setCanOpenChat(savedChatStatus);
        }
    }
}

function saveData() {
    let data = new Array();

    for (let i = 0; i < menuData.Items.length; i++) {
        var menuItem = menuData.Items[i];

        if (menuItem.Type === 1) {
            data[menuItem.Id] = menuItem.Checked;
        } else if (menuItem.Type === 3) {
            data[menuItem.Id] = new Array();
            data[menuItem.Id]["Index"] = menuItem.SelectedItem;
            data[menuItem.Id]["Value"] = menuItem.Items[menuItem.SelectedItem];
        } else if (menuItem.InputMaxLength > 0 && menuItem.InputValue !== undefined && menuItem.InputValue.length > 0) {
            data[menuItem.Id] = menuItem.InputValue;
        }
    }

    return data;
}

function showMenu() {
    forceHide = false;

    if (menu !== null) {
        menu.Visible = true;

        if (savedChatStatus !== undefined) {
            API.setCanOpenChat(false);
        }
    }
}
*/