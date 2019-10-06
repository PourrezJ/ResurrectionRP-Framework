import * as game from 'natives';

export function drawTextLabel(text: string, posx: number, posy: number, posz: number, font: number, colorR: number, colorG: number, colorB: number, colorA: number)
{
    const [bol, _x, _y] = game.getScreenCoordFromWorldCoord(posx, posy, posz, 0, 0);
    const camCord = game.getGameplayCamCoords();
    const dist = game.getDistanceBetweenCoords(camCord.x, camCord.y, camCord.z, posx, posy, posz, true);

    let scale = (4.00001 / dist) * 0.5
    if (scale > 0.2)
        scale = 0.2;
    if (scale < 0.1)
        scale = 0;

    const fov = (1 / game.getGameplayCamFov()) * 100;
    scale = scale * fov;

    if (bol) {
        game.setTextScale(scale, scale);
        game.setTextFont(font);
        game.setTextProportional(true);
        game.setTextColour(colorR, colorG, colorB, colorA);
        game.setTextDropshadow(0, 0, 0, 0, 255);
        game.setTextEdge(2, 0, 0, 0, 150);
        game.setTextDropShadow();
        game.setTextOutline();
        game.setTextCentre(true);
        game.beginTextCommandDisplayText("STRING");
        game.addTextComponentSubstringPlayerName(text);
        game.endTextCommandDisplayText(_x, _y + 0.025, 0);
    }
}