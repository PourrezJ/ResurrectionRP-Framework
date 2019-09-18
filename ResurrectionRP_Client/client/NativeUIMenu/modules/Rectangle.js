import Point from "../utils/Point.js";
import Size from "../utils/Size.js";
import IElement from "../modules/IElement.js";
import * as game from 'natives';

export default class Rectangle extends IElement {
    constructor(pos, size, color) {
        super();
        this.enabled = true;
        this.pos = pos;
        this.size = size;
        this.color = color;
    }
    Draw(pos, size, color) {
        if (!pos)
            pos = new Size(0, 0);
        if (!size && !color) {
            pos = new Point(this.pos.X + pos.Width, this.pos.Y + pos.Height);
            size = this.size;
            color = this.color;
        }
        const w = size.Width / 1280.0;
        const h = size.Height / 720.0;
        const x = pos.X / 1280.0 + w * 0.5;
        const y = pos.Y / 720.0 + h * 0.5;
		game.drawRect(x, y, w, h, color.R, color.G, color.B, color.A);
    }
}
