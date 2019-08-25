import * as alt from 'alt';
import * as game from 'natives';

export class Streamer {
    public StaticEntityList: any[] = [];
    public EntityList: any[] = [];
    constructor() {
        alt.on("onStreamIn", this.onStreamIn);
        alt.on("onStreamOut", this.onStreamOut);
        alt.on("onStreamDataChange", this.onStreamDataChange);
        alt.on("disconnect", this.unloadStream);

        alt.onServer('createStaticEntity', (data: any[]) => {
            switch (data["entityType"]) {
                case 4:
                    this.streamBlip(
                        data["id"],
                        data["posx"],
                        data["posy"],
                        data["posz"],
                        data["sprite"],
                        data["color"],
                        data["name"],
                        data["scale"],
                        data["shortRange"]
                    );
                    break;
            }
        });


        alt.on("update", () => {
            this.EntityList.forEach((item, index) => {
                if (item != null && item["Text"] != null) {
                    displayTextLabel(item);
                } else if (item != null && item["scalex"] != null) {
                    game.drawMarker(
                        item["type"],
                        item["PosX"],
                        item["PosY"],
                        item["PosZ"],
                        0, 0, 0, 0, 0, 0,
                        item["scalex"],
                        item["scaley"],
                        item["scalez"],
                        item["Color"]["r"],
                        item["Color"]["g"],
                        item["Color"]["b"],
                        item["Color"]["a"], false, false, 0, false, undefined, undefined, false)
                }
            });
        });
    }



    onStreamIn = async (entity: object) => {

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
                    { r: entity["data"]["r"]["intValue"], g: entity["data"]["g"]["intValue"], b: entity["data"]["b"]["intValue"], a: entity["data"]["a"]["intValue"]}
                );
                break;
            case 3:
                await this.streamMarker(
                    entity["data"]["id"]["intValue"],
                    entity["data"]["type"]["intValue"],
                    entity["position"]["x"],
                    entity["position"]["y"],
                    entity["position"]["z"],
                    entity["data"]["scalex"]["doubleValue"],
                    entity["data"]["scaley"]["doubleValue"],
                    entity["data"]["scalez"]["doubleValue"],
                    { r: entity["data"]["r"]["intValue"], g: entity["data"]["g"]["intValue"], b: entity["data"]["b"]["intValue"], a: entity["data"]["a"]["intValue"] }
                );
                break;
        }

    }
    private onStreamOut = async (entity: object) => {

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
    private streamMarker = async (id: number, type: number, x: number, y: number, z: number, scalex: number, scaley: number, scalez: number, rgba: object) => {
        this.EntityList[id] = { PosX: x, PosY: y, PosZ: z, Color: rgba , type: type, scalex: scalex, scaley: scaley, scalez: scalez};
    }
    private streamBlip = async (id: number, x: number, y: number, z: number, sprite: number, color:number, name: string, scale: number, shortRange: boolean) => {

        var test = new alt.PointBlip(x,y,z);
        test.sprite = sprite;
        test.color = color;
        test.name = name;
        test.scale = scale;
        test.shrinked = true;
        test.shortRange = shortRange;
        this.StaticEntityList[id] = test;
    }

    public onStreamDataChange = (entity: object, data: object) => {
        switch (entity["data"]["entityType"]["intValue"]) {
            case 0:
                game.deletePed(this.EntityList[entity["data"]["id"]["intValue"]])
                break;
            case 1:
                game.deleteObject(this.EntityList[entity["data"]["id"]["intValue"]])
                break;
            case 2:
                this.EntityList[entity["data"]["id"]["intValue"]] = undefined;
                break;
            case 3:
                this.EntityList[entity["data"]["id"]["intValue"]] = undefined;
                break;
        }
         this.onStreamIn(entity);
    }

}

function displayTextLabel(textLabel) {
    //alt.log(textLabel["PosX"] + " " +  textLabel["PosY"] + " " +  textLabel["PosZ"])
    const [bol, _x, _y] = game.getScreenCoordFromWorldCoord(textLabel["PosX"], textLabel["PosY"], textLabel["PosZ"],0,0);
        const camCord = game.getGameplayCamCoords();
    const dist = game.getDistanceBetweenCoords(camCord.x, camCord.y, camCord.z, textLabel["PosX"], textLabel["PosY"], textLabel["PosZ"], true);

        let scale = (4.00001 / dist) * 0.5
        if (scale > 0.2)
            scale = 0.2;
        if(scale < 0.1)
            scale = 0;

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