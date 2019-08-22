import ResRectangle from "includes/NativeUIMenu/modules/ResRectangle";
import Sprite from "includes/NativeUIMenu/modules/Sprite";
import Color from "includes/NativeUIMenu/utils/Color";
import Point from "includes/NativeUIMenu/utils/Point";
import Size from "includes/NativeUIMenu/utils/Size";
import UIMenuItem from "includes/NativeUIMenu/items/UIMenuItem";
export default class UIMenuSliderItem extends UIMenuItem {
    private _index;
    private _items;
    private _arrowLeft;
    private _arrowRight;
    private _rectangleBackground;
    private _rectangleSlider;
    private _rectangleDivider;

    get Index() {
        return this._index % this._items.length;
    }
    set Index(value) {
        this._index = 100000000 - (100000000 % this._items.length) + value;
    }
    constructor(text, items, index, description = "", divider = false) {
        super(text, description);
        const y = 0;
        this._items = items;
        this._arrowLeft = new Sprite("commonmenutu", "arrowleft", new Point(0, 105 + y), new Size(15, 15));
        this._arrowRight = new Sprite("commonmenutu", "arrowright", new Point(0, 105 + y), new Size(15, 15));
        this._rectangleBackground = new ResRectangle(new Point(0, 0), new Size(150, 9), new Color(4, 32, 57, 255));
        this._rectangleSlider = new ResRectangle(new Point(0, 0), new Size(75, 9), new Color(57, 116, 200, 255));
        if (divider) {
            this._rectangleDivider = new ResRectangle(new Point(0, 0), new Size(2.5, 20), Color.WhiteSmoke);
        }
        else {
            this._rectangleDivider = new ResRectangle(new Point(0, 0), new Size(2.5, 20), Color.Transparent);
        }
        this.Index = index;
    }
    SetVerticalPosition(y) {
        this._rectangleBackground.pos = new Point(250 + this.Offset.X + this.Parent.WidthOffset, y + 158.5 + this.Offset.Y);
        this._rectangleSlider.pos = new Point(250 + this.Offset.X + this.Parent.WidthOffset, y + 158.5 + this.Offset.Y);
        this._rectangleDivider.pos = new Point(323.5 + this.Offset.X + this.Parent.WidthOffset, y + 153 + this.Offset.Y);
        this._arrowLeft.pos = new Point(235 + this.Offset.X + this.Parent.WidthOffset, 155.5 + y + this.Offset.Y);
        this._arrowRight.pos = new Point(400 + this.Offset.X + this.Parent.WidthOffset, 155.5 + y + this.Offset.Y);
        super.SetVerticalPosition(y);
    }
    IndexToItem(index) {
        return this._items[index];
    }
    Draw() {
        super.Draw();
        this._arrowLeft.color = this.Enabled
            ? this.Selected
                ? Color.Black
                : Color.WhiteSmoke
            : new Color(163, 159, 148);
        this._arrowRight.color = this.Enabled
            ? this.Selected
                ? Color.Black
                : Color.WhiteSmoke
            : new Color(163, 159, 148);
        let offset = ((this._rectangleBackground.size.Width -
            this._rectangleSlider.size.Width) /
            (this._items.length - 1)) *
            this.Index;
        this._rectangleSlider.pos = new Point(250 + this.Offset.X + offset + +this.Parent.WidthOffset, this._rectangleSlider.pos.Y);
        if (this.Selected) {
            this._arrowLeft.Draw();
            this._arrowRight.Draw();
        }
        this._rectangleBackground.Draw();
        this._rectangleSlider.Draw();
        this._rectangleDivider.Draw();
    }
    SetRightBadge(badge) { }
    SetRightLabel(text) { }
}
