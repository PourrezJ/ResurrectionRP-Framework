import * as alt from 'alt';
import * as game from 'natives';

export class Streamer {
    public EntityList: any[] = [];
    constructor() {
        alt.on("onStreamIn", this.onStreamIn);
        alt.on("onStreamOut", this.onStreamOut);
        alt.on("onStreamDataChange", this.onStreamDataChange);
        alt.on("disconnect", this.unloadStream);

        alt.on("update", () => {
            this.EntityList.forEach((item, index) => {
                if (item != null && item["Text"] != null) {
                    displayTextLabel(item);
                }
            });
        });
    }



    onStreamIn = async (entity: object) => {
        //alt.log("EVENT STREAM IN : " + JSON.stringify(entity));

        switch (entity["data"]["entityType"]["intValue"]) {
            case 0:
                await alt.loadModelAsync(entity["data"]["model"]["uintValue"]);
                this.streamPed(
                    entity["data"]["id"]["intValue"],
                    entity["data"]["type"]["intValue"],
                    entity["data"]["model"]["uintValue"],
                    entity["position"]["x"],
                    entity["position"]["y"],
                    entity["position"]["z"],
                    entity["data"]["heading"]
                );
                break;
            case 1:
                await alt.loadModelAsync(entity["data"]["model"]["uintValue"]);
                await this.streamObject(
                    entity["data"]["id"]["intValue"],
                    entity["data"]["model"]["uintValue"],
                    entity["position"]["x"],
                    entity["position"]["y"],
                    entity["position"]["z"]
                );
                break;
            case 2:
                await this.streamTextLabel(
                    entity["data"]["id"]["intValue"],
                    entity["data"]["text"]["stringValue"],
                    entity["position"]["x"],
                    entity["position"]["y"],
                    entity["position"]["z"],
                    entity["data"]["font"]["intValue"],
                    { r: entity["data"]["r"]["intValue"], g: entity["data"]["g"]["intValue"], b: entity["data"]["b"]["intValue"], a: entity["data"]["a"]["intValue"],}
                );
                break;
        }

    }
    private onStreamOut = async (entity: object) => {
        //alt.log("EVENT STREAM OUT : " + JSON.stringify(entity));

        this.EntityList.forEach((item, index) => {
            if (entity["data"]["Text"])
                return;
            if (index != entity["data"]["id"]["intValue"])
                return;
            if (entity["data"]["entityType"]["intValue"] == 0 || entity["data"]["entityType"]["intValue"] == 1) {
                game.deleteEntity(item);
            }
            this.EntityList[index] = null;
        });

    }

    private unloadStream = async () => {
        this.EntityList.forEach((item, index) => {
            game.deleteEntity(item);
            this.EntityList[index] = null;
        });
    }

    private streamPed = async (id: number,type: number, model: any, x: number, y: number, z: number, heading: number) => {
        var entityId = game.createPed(type, model, x,y,z, heading, false, true);
        this.EntityList[id] = entityId;

    }
    private streamObject = async (id: number, model: any, x: number, y: number, z: number) => {
        var entityId = game.createObject(model, x, y, z, false, true, false);
        this.EntityList[id] = entityId;
    }
    private streamTextLabel = async (id: number, text: string, x: number, y: number, z: number, font: number, rgba: object) => {
        this.EntityList[id] = {PosX: x, PosY: y, PosZ: z, Text: text, Font: font, Color: rgba};
    }

    public onStreamDataChange(entity: object, data: object) {

    }

}
function displayTextLabel(textLabel) {
    //alt.log(textLabel["PosX"] + " " +  textLabel["PosY"] + " " +  textLabel["PosZ"])
    const [bol, _x, _y] = game.getScreenCoordFromWorldCoord(textLabel["PosX"], textLabel["PosY"], textLabel["PosZ"],0,0);
        const camCord = game.getGameplayCamCoords();
    const dist = game.getDistanceBetweenCoords(camCord.x, camCord.y, camCord.z, textLabel["PosX"], textLabel["PosY"], textLabel["PosZ"], true);

        let scale = (4.00001 / dist) * 0.3
        if (scale > 0.2)
            scale = 0.2;

        const fov = (1 / game.getGameplayCamFov()) * 100;
        scale = scale * fov;

        if (bol) {
            game.setTextScale(scale, scale);
            game.setTextFont(textLabel["Font"]);
            game.setTextProportional(true);
            game.setTextColour(textLabel["Color"]["r"], textLabel["Color"]["g"], textLabel["Color"]["b"], textLabel["Color"]["a"]);
            game.setTextDropshadow(0, 0, 0, 0, 255);
            game.setTextEdge(2, 0, 0, 0, 150);
            game.setTextDropShadow();
            game.setTextOutline();
            game.setTextCentre(true);
            game.beginTextCommandDisplayText("STRING");
            game.addTextComponentSubstringPlayerName(textLabel["Text"]);
            game.endTextCommandDisplayText(_x, _y + 0.025);
        }
}