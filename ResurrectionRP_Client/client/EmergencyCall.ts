import * as alt from 'alt-client';
import * as game from 'natives';

class Call {

    public id: number;
    public position: alt.Vector3;
    public Blip: alt.Blip;
    public Taken: boolean = false;
    public factionName: string;
    public EveryTick: number;

    constructor(id: number, position: alt.Vector3, factionName: string, blipsprite: number, blipcolor: number) {
        this.id = id;
        this.position = position;

        this.Blip = new alt.PointBlip(position.x as number, position.y as number, position.z as number)
        this.Blip.sprite = blipsprite;
        this.Blip.color = blipcolor;
        this.Blip.name = "Appel " + factionName;
        this.Blip.scale = 1;
        this.Blip.shrinked = true;
        this.Blip.shortRange = false;


        this.EveryTick = alt.everyTick(() => {


            if (this == undefined || this == null)
                return;
            if (this.Taken && game.getDistanceBetweenCoords(this.position.x as number, this.position.y as number, this.position.z as number, alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z, false) < 15) {
                EmergencyCall.IsInMission = false;
                alt.clearEveryTick(this.EveryTick);
                alt.emitServer("InteractEmergencyCall", "release", this.id);
            }


        });
    }

}

export class EmergencyCall {

    public static Calls: object = {};

    public static IsAnyMissionAvailable: boolean = false;
    public static IsInMission: boolean = false;


    constructor() {

        alt.onServer("EC_UpdateBlipColor", this.EC_UpdateBlipColor);
        alt.onServer("EC_ReleaseCall", this.EC_ReleaseCall);
        alt.onServer("EC_EmitCall", this.EC_EmitCall);

    }


    public EC_UpdateBlipColor = (FactionName: string, callid: number, blipcolor: number) => {
        if (EmergencyCall.Calls[FactionName][callid] != undefined) {
            EmergencyCall.Calls[FactionName][callid].Taken = true;
            EmergencyCall.Calls[FactionName][callid].Blip.color = blipcolor;
        }
    }
    

    public EC_ReleaseCall = (FactionName: string, callid: number) => {
        var item: Call =
            EmergencyCall.Calls[FactionName][callid];
        if (item == undefined)
            return;

        alt.clearEveryTick(item.EveryTick);
        item.Blip.destroy();
        EmergencyCall.Calls[FactionName][callid] = undefined;

    }

    public EC_EmitCall = (FactionName: string, callid: number, positionString: string, BlipSprite: number, BlipColor: number, reason: string) => {


        var positionObject = JSON.parse(positionString);
        var position: alt.Vector3 = new alt.Vector3(positionObject.X, positionObject.Y, positionObject.Z);

        if (EmergencyCall.Calls[FactionName] == undefined)
            EmergencyCall.Calls[FactionName] = {};

        EmergencyCall.IsAnyMissionAvailable = true;

        EmergencyCall.Calls[FactionName][callid] = new Call(callid, position, FactionName, BlipSprite, BlipColor);
        if(!EmergencyCall.IsInMission)
            alt.emit("SetNotificationMessage", "CHAR_BRYONY", "Centrale", "Appel", "Nouveau appel, appuyez sur ~g~Y~w~ pour accepter, à tout moment!");
    }


}