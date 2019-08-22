import UUIDV4 from "includes/NativeUIMenu/utils/UUIDV4";
export default class ListItem {
    public Id;
    public DisplayText;
    public Data;

    constructor(text = "", data = null) {
        this.Id = UUIDV4();
        this.DisplayText = text;
        this.Data = data;
    }
}
