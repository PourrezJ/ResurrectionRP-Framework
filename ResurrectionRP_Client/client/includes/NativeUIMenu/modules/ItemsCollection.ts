import ListItem from "includes/NativeUIMenu/modules/ListItem";
export default class ItemsCollection {
    public items;

    constructor(items) {
        if (items.length === 0)
            throw new Error("ItemsCollection cannot be empty");
        this.items = items;
    }
    length() {
        return this.items.length;
    }
    getListItems() {
        const items = [];
        for (const item of this.items) {
            if (item instanceof ListItem) {
                items.push(item);
            }
            else if (typeof item == "string") {
                items.push(new ListItem(item.toString()));
            }
        }
        return items;
    }
}
