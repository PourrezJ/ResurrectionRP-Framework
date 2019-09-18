import Font from "../enums/Font.js";
import ItemsCollection from "../modules/ItemsCollection.js";
import ListItem from "../modules/ListItem.js";
import ResText, { Alignment } from "../modules/ResText.js";
import Sprite from "../modules/Sprite.js";
import Color from "../utils/Color.js";
import LiteEvent from "../utils/LiteEvent.js";
import Point from "../utils/Point.js";
import Size from "../utils/Size.js";
import StringMeasurer from "../modules/StringMeasurer.js";
import UIMenuItem from "../items/UIMenuItem.js";

export default class UIMenuListItem extends UIMenuItem {
    constructor(text, description = "", collection = new ItemsCollection([]), startIndex = 0) {
        super(text, description);
        this.currOffset = 0;
        this.collection = [];
        this.ScrollingEnabled = true;
        this.HoldTimeBeforeScroll = 200;
        this.OnListChanged = new LiteEvent();
        this._index = 0;
        let y = 0;
        this.Collection = collection.getListItems();
        this.Index = startIndex;
        this._arrowLeft = new Sprite("commonmenu", "arrowleft", new Point(110, 105 + y), new Size(30, 30));
        this._arrowRight = new Sprite("commonmenu", "arrowright", new Point(280, 105 + y), new Size(30, 30));
        this._itemText = new ResText("", new Point(290, y + 104), 0.35, Color.White, Font.ChaletLondon, Alignment.Right);
    }
    get Collection() {
        return this.collection;
    }
    set Collection(v) {
        if (!v)
            throw new Error("The collection can't be null");
        this.collection = v;
    }
    set SelectedItem(v) {
        const idx = this.Collection.findIndex(li => li.Id === v.Id);
        if (idx > 0)
            this.Index = idx;
        else
            this.Index = 0;
    }
    get SelectedItem() {
        return this.Collection.length > 0 ? this.Collection[this.Index] : null;
    }
    get SelectedValue() {
        return this.SelectedItem == null
            ? null
            : this.SelectedItem.Data == null
                ? this.SelectedItem.DisplayText
                : this.SelectedItem.Data;
    }
    get ListChanged() {
        return this.OnListChanged.expose();
    }
    get Index() {
        if (this.Collection == null)
            return -1;
        if (this.Collection != null && this.Collection.length == 0)
            return -1;
        return this._index % this.Collection.length;
    }
    set Index(value) {
        if (this.Collection == null)
            return;
        if (this.Collection != null && this.Collection.length == 0)
            return;
        this._index = 100000 - (100000 % this.Collection.length) + value;
        const caption = this.Collection.length >= this.Index
            ? this.Collection[this.Index].DisplayText
            : " ";
        this.currOffset = StringMeasurer.MeasureString(caption);
    }
    setCollection(collection) {
        this.Collection = collection.getListItems();
    }
    setCollectionItem(index, item, resetSelection = true) {
        if (index > this.Collection.length)
            throw new Error("Index out of bounds");
        if (typeof item === "string")
            item = new ListItem(item);
        this.Collection.splice(index, 1, item);
        if (resetSelection)
            this.Index = 0;
    }
    SetVerticalPosition(y) {
        this._arrowLeft.pos = new Point(300 + this.Offset.X + this.Parent.WidthOffset, 147 + y + this.Offset.Y);
        this._arrowRight.pos = new Point(400 + this.Offset.X + this.Parent.WidthOffset, 147 + y + this.Offset.Y);
        this._itemText.pos = new Point(300 + this.Offset.X + this.Parent.WidthOffset, y + 147 + this.Offset.Y);
        super.SetVerticalPosition(y);
    }
    SetRightLabel(text) {
        return this;
    }
    SetRightBadge(badge) {
        return this;
    }
    Draw() {
        super.Draw();
        const caption = this.Collection.length >= this.Index
            ? this.Collection[this.Index].DisplayText
            : " ";
        const offset = this.currOffset;
        this._itemText.color = this.Enabled
            ? this.Selected
                ? this.HighlightedForeColor
                : this.ForeColor
            : new Color(163, 159, 148);
        this._itemText.caption = caption;
        this._arrowLeft.color = this.Enabled
            ? this.Selected
                ? this.HighlightedForeColor
                : this.ForeColor
            : new Color(163, 159, 148);
        this._arrowRight.color = this.Enabled
            ? this.Selected
                ? this.HighlightedForeColor
                : this.ForeColor
            : new Color(163, 159, 148);
        this._arrowLeft.pos = new Point(375 - offset + this.Offset.X + this.Parent.WidthOffset, this._arrowLeft.pos.Y);
        if (this.Selected) {
            this._arrowLeft.Draw();
            this._arrowRight.Draw();
            this._itemText.pos = new Point(405 + this.Offset.X + this.Parent.WidthOffset, this._itemText.pos.Y);
        }
        else {
            this._itemText.pos = new Point(420 + this.Offset.X + this.Parent.WidthOffset, this._itemText.pos.Y);
        }
        this._itemText.Draw();
    }
}
