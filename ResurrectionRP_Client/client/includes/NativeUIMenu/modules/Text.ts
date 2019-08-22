import Color from "includes/NativeUIMenu/utils/Color";
import Point from "includes/NativeUIMenu/utils/Point";
import IElement from "includes/NativeUIMenu/modules/IElement";
import * as alt from 'alt';
import * as game from 'natives';
export default class Text extends IElement {
    public caption;
    public pos;
    public scale;
    public color;
    public font;
    public centered;

    constructor(caption, pos, scale, color, font, centered) {
        super();
        this.caption = caption;
        this.pos = pos;
        this.scale = scale;
        this.color = color || new Color(255, 255, 255, 255);
        this.font = font || 0;
        this.centered = centered || false;
    }
    Draw(caption, pos, scale, color, font, centered) {
        if (caption && !pos && !scale && !color && !font && !centered) {
            pos = new Point(this.pos.X + caption.Width, this.pos.Y + caption.Height);
            scale = this.scale;
            color = this.color;
            font = this.font;
            centered = this.centered;
        }
        const x = pos.X / 1280.0;
        const y = pos.Y / 720.0;
		game.setTextFont(parseInt(font));
		game.setTextScale(scale, scale);
		game.setTextColour(color.R, color.G, color.B, color.A);
		game.setTextCentre(centered);
		game.beginTextCommandDisplayText("STRING");
		Text.AddLongString(caption);
		game.endTextCommandDisplayText(x, y);
    }
    static AddLongString(str) {
        const strLen = 99;
        for (var i = 0; i < str.length; i += strLen) {
            const substr = str.substr(i, Math.min(strLen, str.length - i));
            game.addTextComponentSubstringPlayerName(substr);
        }
    }
}
export { Text };
