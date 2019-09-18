import Point from "../utils/Point.js";
import Size from "../utils/Size.js";
import Rectangle from "../modules/Rectangle.js";
import { Screen } from "../utils/Screen.js";
import * as game from 'natives';

export default class ResRectangle extends Rectangle {
    constructor(pos, size, color) {
        super(pos, size, color);
    }
    Draw(pos, size, color) {
        if (!pos)
            pos = new Size();
        if (pos && !size && !color) {
            pos = new Point(this.pos.X + pos.Width, this.pos.Y + pos.Height);
            size = this.size;
            color = this.color;
        }
        const screenw = Screen.width;
        const screenh = Screen.height;
        const height = 1080.0;
        const ratio = screenw / screenh;
        const width = height * ratio;
        const w = size.Width / width;
        const h = size.Height / height;
        const x = pos.X / width + w * 0.5;
		const y = pos.Y / height + h * 0.5;
		game.drawRect(x, y, w, h, color.R, color.G, color.B, color.A);
    }
}
