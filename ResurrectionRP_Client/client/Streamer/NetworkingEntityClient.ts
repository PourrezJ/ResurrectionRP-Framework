﻿import * as alt from 'alt';
import * as game from 'natives';
import * as utils from '../Utils/Utils';

export class NetworkingEntityClient {

    webview: alt.WebView;
    defaultToken: boolean;
    defaultWebView: boolean;
    streamedInEntities: {};
    interval: number;
    public static EntityList: any[] = [];
    public static StaticEntityList: any[] = [];

    constructor() {
        alt.log("Chargement controleur du streamer ...");
        this.webview = new alt.WebView("http://resource/client/Streamer/index.html");
        this.defaultToken = true;
        this.defaultWebView = true;
        this.streamedInEntities = {};

        alt.everyTick(() => {
            NetworkingEntityClient.EntityList.forEach((item, index) => {
                if (item != null && item.Text != null) {
                    this.displayTextLabel(item);
                } else if (item != null && item.scalex != null) {
                    game.drawMarker(
                        item.type,
                        item.PosX,
                        item.PosY,
                        item.PosZ,
                        0, 0, 0, 0, 0, 0,
                        item.scalex,
                        item.scaley,
                        item.scalez,
                        item.Color.r,
                        item.Color.g,
                        item.Color.b,
                        item.Color.a, false, false, 0, false, undefined, undefined, false)
                }
            });
        });

        alt.on("disconnect", this.unloadStream.bind(this));

        alt.onServer('createStaticEntity', this.createStaticEntity.bind(this));
        alt.onServer("deleteStaticEntity", this.deleteStaticEntity.bind(this));
        alt.onServer("setStaticEntityBlipRoute", this.setStaticEntityBlipRoute.bind(this));

        alt.onServer("deleteObject", this.deleteObject.bind(this));

        alt.on("interactWithPickableObject", this.interactPickup);

        this.webview.on("streamIn", (entities) => {
            for (const entity of JSON.parse(entities)) {
                this.streamedInEntities[entity.id] = entity;
                this.onStreamIn(entity);
            }
        });

        this.webview.on("streamInBuffer", (entityBuffer) => {
        });

        this.webview.on("streamOut", (entities) => {
            for (const entity of JSON.parse(entities)) {
                const currEntity = this.streamedInEntities[entity.id];
                if (currEntity) {
                    this.onStreamOut(currEntity);
                    delete this.streamedInEntities[entity.id];
                } else {
                    this.onStreamOut(entity);
                }
            }
        });

        this.webview.on("dataChange", (entityAndNewData) => {
            const entityAndNewDataParsed = JSON.parse(entityAndNewData);
            const currEntity = this.streamedInEntities[entityAndNewDataParsed.entity.id];

            if (currEntity) {
                currEntity.data = entityAndNewDataParsed.entity.data;
                this.onDataChange(currEntity, entityAndNewDataParsed.data);
            } else {
                this.onDataChange(entityAndNewDataParsed.entity, entityAndNewDataParsed.data);
            }
        });

        if (this.defaultToken) {
            alt.onServer("streamingToken", this.tokenCallback.bind(this));
        }
        alt.log("Streamer Initialisé");
    }

    init(url, token) {
        this.webview.emit("entitySetup", url, token);
        const localPlayer = alt.Player.local;
        let pos;
        this.interval = alt.setInterval(() => {
            pos = localPlayer.pos;
            this.webview.emit("playerPosition",
                pos.x,
                pos.y,
                pos.z)
        }, 100);
        alt.log("Initialisation du streamer...");
    }

    destroy() {
        alt.log(`Network: destroy`);
        this.webview.emit("entityDestroy");
        alt.clearInterval(this.interval);

        if (this.defaultToken) {
            alt.offServer("streamingToken", this.tokenCallback);
        }
    }

    tokenCallback(url: string, token: any) {
        alt.log(`Network: token: ${url} ${token}`);
        this.init(url, token);
    }

    onDataChange = async (entity: any, data: any) => {
        let count = 0;

        // Creating an entity can take some time so wait until it is created before updating it
        const interval = alt.setInterval(() => {
            if (NetworkingEntityClient.EntityList[entity.id] == undefined || NetworkingEntityClient.EntityList[entity.id] == null) {
                count++;

                if (count == 500) {
                    alt.clearTimeout(interval);
                }

                return;
            }

            alt.clearInterval(interval);

            switch (entity.data.entityType.intValue) {
                case 0:
                    game.deletePed(NetworkingEntityClient.EntityList[entity.id])
                    this.onStreamIn(entity);
                    break;
                case 1:
                   // game.deleteObject(this.EntityList[entity.id])
                    this.onStreamIn(entity);
                    break;
                case 2:
                    NetworkingEntityClient.EntityList[entity.id] = undefined;
                    this.onStreamIn(entity);
                    break;
                case 3:
                    NetworkingEntityClient.EntityList[entity.id] = undefined;
                    this.onStreamIn(entity);
                    break;
                case 4:
                    NetworkingEntityClient.StaticEntityList[entity.id].destroy();
                    break;
            }
        }, 10);


        if (entity.data.entityType.intValue == 1) {
            alt.log(`entity: ${JSON.stringify(entity)}`);

            let obj = NetworkingEntityClient.EntityList[entity.id];

            if (entity.data.attach.stringValue != null && game.isEntityAttached(obj)) {
                game.detachEntity(obj, false, false);
            }

            game.setEntityCoordsNoOffset(obj, entity.position.x, entity.position.y, entity.position.z, false, false, false);
            game.setEntityRotation(obj, entity.data.rotation.x, entity.data.rotation.y, entity.data.rotation.z, 0, false);
            game.placeObjectOnGroundProperly(obj);
        }
    }

    onStreamOut = async (entity: any) => {
        NetworkingEntityClient.EntityList.forEach((item, index) => {
           if (entity.data.Text)
                return;

            if (index != entity.id)
                return;

            if (entity.data.entityType.intValue == 0 || entity.data.entityType.intValue == 1) {
                game.deleteEntity(item);
            }

            NetworkingEntityClient.EntityList[index] = null;
        });

    }

    onStreamIn = async (entity: any) => {
        switch (entity.data.entityType.intValue) {
            case 0:
                await utils.loadModelAsync(entity.data.model.uintValue);
                this.streamPed(
                    entity.id,
                    entity.data.model.uintValue,
                    entity.position.x,
                    entity.position.y,
                    entity.position.z,
                    entity.data.heading.doubleValue
                );
                break;
            case 1:
                await utils.loadModelAsync(game.getHashKey(entity.data.model.stringValue));
                let object = await this.streamObject(
                    entity.id,
                    entity.data.model.intValue,
                    entity.position.x,
                    entity.position.y,
                    entity.position.z,
                    entity.data.freeze.boolValue
                );

                if (JSON.parse(entity.data.attach.stringValue) != null)
                    this.objectAttach(entity.id, JSON.parse(entity.data.attach.stringValue));
                break;
            case 2:
                await this.streamTextLabel(
                    entity.id,
                    entity.data.text.stringValue,
                    entity.position.x,
                    entity.position.y,
                    entity.position.z,
                    entity.data.font.intValue,
                    { r: entity.data.r.intValue, g: entity.data.g.intValue, b: entity.data.b.intValue, a: entity.data.a.intValue }
                );
                break;
            case 3:
                await this.streamMarker(
                    entity.id,
                    entity.data.type.intValue,
                    entity.position.x,
                    entity.position.y,
                    entity.position.z,
                    entity.data.scalex.doubleValue,
                    entity.data.scaley.doubleValue,
                    entity.data.scalez.doubleValue,
                    { r: entity.data.r.intValue, g: entity.data.g.intValue, b: entity.data.b.intValue, a: entity.data.a.intValue }
                );
                break;
        }
    }

    private deleteObject = (entityid: number) => {
        if (NetworkingEntityClient.EntityList[entityid] == undefined)
            return;

        game.deleteObject(NetworkingEntityClient.EntityList[entityid]);
    }


    private displayTextLabel(textLabel) {
        const [bol, _x, _y] = game.getScreenCoordFromWorldCoord(textLabel["PosX"], textLabel["PosY"], textLabel["PosZ"], 0, 0);
        const camCord = game.getGameplayCamCoords();
        const dist = game.getDistanceBetweenCoords(camCord.x, camCord.y, camCord.z, textLabel["PosX"], textLabel["PosY"], textLabel["PosZ"], true);

        let scale = (4.00001 / dist) * 0.5
        if (scale > 0.2)
            scale = 0.2;
        if (scale < 0.1)
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
            game.endTextCommandDisplayText(_x, _y + 0.025, 0);
        }
    }

    private streamPed = async (id: number, model: any, x: number, y: number, z: number, heading: number) => {
        var entityId = game.createPed(4, model, x, y, z - 1, heading, false, true); 

        if (entityId != 0) {
            game.taskSetBlockingOfNonTemporaryEvents(entityId, true);
            game.setEntityInvincible(entityId, true);
            game.freezeEntityPosition(entityId, true);
        }

        //alt.logWarning("Streaming in new ped, entity ID " + entityId + " | id global : " + id + "| heading : " + JSON.stringify(heading));
        //alt.logWarning(`Model : ${model}`);
        NetworkingEntityClient.EntityList[id] = entityId;
    }

    private streamObject = (id: number, model: any, x: number, y: number, z: number, freeze: boolean) : number  => {
        let entityId : number = null;

        if (NetworkingEntityClient.EntityList[id] == undefined)
            entityId = game.createObject(model, x, y, z, false, true, false);
        else
            entityId = NetworkingEntityClient.EntityList[id];


        game.freezeEntityPosition(entityId, freeze);
        NetworkingEntityClient.EntityList[id] = entityId;
        return entityId;
    }

    private objectAttach = (entityId: number, attach: any) => {
        switch (attach.Type) {
            case 5:
/*
                var vehicle = Entities.Vehicles.All.Find(p => p.RemoteId == attach.RemoteID);
                if (vehicle != null && vehicle.Exists) {
                    try {
                        int bone = 0;

                        bone = vehicle.GetBoneIndexByName(attach.Bone);
                        if (Game.Instance.IsDebug)
                            Utils.LogInfo("Vehicle bone: " + bone.ToString());

                        RAGE.Game.Entity.AttachEntityToEntity(@object.Handle, vehicle.Handle, bone, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z, true, false, false, false, 0, true);
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError("Vehicle bone: " + ex.ToString());
                    }
                }*/
                var veh: alt.Vehicle = alt.Vehicle.all.find(p => p.id == attach.RemoteID);

                var bone = game.getEntityBoneIndexByName(veh.scriptID, attach.Bone);
                game.attachEntityToEntity(NetworkingEntityClient.EntityList[entityId], veh.scriptID, bone, attach.PositionOffset.X, attach.PositionOffset.Y, attach.PositionOffset.Z, attach.RotationOffset.X, attach.RotationOffset.Y, attach.RotationOffset.Z, true, false, false, false, 0, true);
                break;
            default:
                alt.logError("Entity attached not coded");
                break;
        }
    }

    private interactPickup = (OID: number ) => {
        NetworkingEntityClient.EntityList.forEach((item, index) => {
            if (item == OID)
                alt.emitServer("ObjectManager_InteractPickup", index);
        });
    }

    private streamTextLabel = async (id: number, text: string, x: number, y: number, z: number, font: number, rgba: object) => {
        NetworkingEntityClient.EntityList[id] = { PosX: x, PosY: y, PosZ: z, Text: text, Font: font, Color: rgba };
    }

    private streamMarker = async (id: number, type: number, x: number, y: number, z: number, scalex: number, scaley: number, scalez: number, rgba: object) => {
        NetworkingEntityClient.EntityList[id] = { PosX: x, PosY: y, PosZ: z, Color: rgba, type: type, scalex: scalex, scaley: scaley, scalez: scalez };
    }

    private streamBlip = async (id: number, x: number, y: number, z: number, sprite: number, color: number, name: string, scale: number, shortRange: boolean) => {

        var test = new alt.PointBlip(x, y, z);
        test.sprite = sprite;
        test.color = color;
        test.name = name;
        test.scale = scale;
        test.shrinked = true;
        test.shortRange = shortRange;
        NetworkingEntityClient.StaticEntityList[id] = test;
    }

    private deleteStaticEntity = (entityid: number, type: number) => {
        if (NetworkingEntityClient.StaticEntityList[entityid] == undefined)
            return;
        switch (type) {
            case 4:
                NetworkingEntityClient.StaticEntityList[entityid].destroy();
                break;
        }
    }
    private setStaticEntityBlipRoute = (entityId: number, state: boolean, color: number) => {
        // BUG V??? seems not to work properly ? RouteColor what is it its referral?
        NetworkingEntityClient.StaticEntityList[entityId].routeColor = 4;
        NetworkingEntityClient.StaticEntityList[entityId].route = state;
    }
    
    private createStaticEntity = (data: any[]) => {
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
    }

    private unloadStream = async () => {
        NetworkingEntityClient.EntityList.forEach((item, index) => {
            game.deleteEntity(item);
            game.deletePed(item);
            NetworkingEntityClient.EntityList[index] = null;
        });
    }
}