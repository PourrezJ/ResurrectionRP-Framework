import * as alt from 'alt-client';
import * as game from 'natives';
import Scaleforms from '../Helpers/Scaleform';

export function EnableHuds(hud = true, radar = true) {
    game.displayRadar(radar);
    game.displayHud(hud);
}

export function SendNotification(msg, flash, textCol = -1, bgCol = -1, flashCol = null) {
    if (textCol > -1)
        game.setColourOfNextTextComponent(4160189315227336364) // 0x39BBF623FC803EAC to int
    if (bgCol > -1)
        game.thefeedSetNextPostBackgroundColor(10588202547000613000) // 0x92F0DA1E27DB96DC

    if (flash) {
        if (flashCol == null)
            flashCol = [77, 77, 77, 255]
        game.thefeedSetAnimpostfxColor(flashCol[0], flashCol[1], flashCol[2], flashCol[3])

    }

    game.beginTextCommandThefeedPost("STRING");
    game.addTextComponentSubstringTextLabel(msg);
    game.endTextCommandThefeedPostTicker(false, true);
}

export function SendPicNotification(title, sender, msg, notifPic, icon = 0, flash = false, textCol = -1, bgCol = -1, flashCol = null) {
    if (textCol > -1)
        game.setColourOfNextTextComponent(4160189315227336364) // 0x39BBF623FC803EAC to int
    if (bgCol > -1)
        game.thefeedSetNextPostBackgroundColor(10588202547000613000) // 0x92F0DA1E27DB96DC

    if (flash) {
        if (flashCol == null)
            flashCol = [77, 77, 77, 255]
        game.thefeedSetAnimpostfxColor(flashCol[0], flashCol[1], flashCol[2], flashCol[3])

    }

    game.beginTextCommandThefeedPost("CELL_EMAIL_BCON");
    game.beginTextCommandThefeedPost(msg);

    game.endTextCommandThefeedPostMessagetext(notifPic, notifPic, flash, icon, title, sender)
    game.endTextCommandThefeedPostTicker(false, true);

}

export function SetNotificationPicture(message: string, dict: string, img: string, flash: boolean, iconType: number, sender: string, subject: string) {
    game.beginTextCommandThefeedPost("STRING");
    game.addTextComponentSubstringPlayerName(message);

    //game.setNotificationMessage2(dict, img, flash, iconType, sender, subject);
    game.endTextCommandThefeedPostMessagetextWithCrewTag(img.toUpperCase(), img.toUpperCase(), false, 4, subject, sender, 1.0, '');
    game.endTextCommandThefeedPostTicker(false, false);

}

export function ShowSubTitle(subtitle, duration = 5000, drawImmediatly = true) {
    game.beginTextCommandPrint("STRING")
    game.addTextComponentSubstringTextLabel(subtitle);
    game.endTextCommandPrint(duration, drawImmediatly);
}

export function SetMaxTextComponentSubstringMessage(msg) {
    let maxLength = 99;
    for (let i = 0; i < msg.length; i += maxLength)
        game.addTextComponentSubstringTextLabel(msg.Substring(i, Math.min(maxLength, msg.length - i)))
}

export function LoadingPromptText(text) {
    game.setNoLoadingScreen(true);
    game.busyspinnerOff();
}

export function DrawText3d(
    msg,
    x,
    y,
    z,
    scale,
    fontType,
    r,
    g,
    b,
    a,
    useOutline = true,
    useDropShadow = true,
    layer = 0
) {
    game.setDrawOrigin(x, y, z, 0);
    game.setScriptGfxDrawOrder(layer);
    game.beginTextCommandDisplayText('STRING');
    game.addTextComponentSubstringPlayerName(msg);
    game.setTextFont(fontType);
    game.setTextScale(1, scale);
    game.setTextWrap(0.0, 1.0);
    game.setTextCentre(true);
    game.setTextColour(r, g, b, a);

    if (useOutline) game.setTextOutline();

    if (useDropShadow) game.setTextDropShadow();

    game.endTextCommandDisplayText(0, 0, 0);
    game.clearDrawOrigin();
}

export function DrawText2d(
    msg,
    x,
    y,
    scale,
    fontType,
    r,
    g,
    b,
    a,
    useOutline = true,
    useDropShadow = true,
    layer = 0,
    align = 0
) {
    game.setScriptGfxDrawOrder(layer);
    game.beginTextCommandDisplayText('STRING');
    game.addTextComponentSubstringPlayerName(msg);
    game.setTextFont(fontType);
    game.setTextScale(1, scale);
    game.setTextWrap(0.0, 1.0);
    game.setTextCentre(true);
    game.setTextColour(r, g, b, a);
    game.setTextJustification(align);

    if (useOutline) game.setTextOutline();

    if (useDropShadow) game.setTextDropShadow();

    game.endTextCommandDisplayText(x, y, 0);
}

export function WeazelNews(text: string, title: string, othertitle: string)
{
    alt.emit("hideHud", true);
/*
    let annonce = new Scaleforms("BREAKING_NEWS") TODO
    annonce.call("SET_TEXT", title, text);
    annonce.call("SET_SCROLL_TEXT", 0, 0, othertitle);
    annonce.call("DISPLAY_SCROLL_TEXT", 0, 0);*/
    alt.emit("Display_subtitle", "~r~" + title + " \n ~w~" + text, 30000);
    alt.setTimeout(() => {
        //annonce = null;
        alt.emit("hideHud", false);
    }, 30000);
}