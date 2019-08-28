import * as alt from 'alt';
import * as game from 'natives';

export class Trash {

    public Object: number;
    private Marker;
    private Blip: alt.RadiusBlip;
    public Position;

    constructor(position) {
        var hash = game.getHashKey("prop_cs_rub_binbag_01");
        this.Object = game.createObject(hash, position.X, position.Y, position.Z, false, true, true);
        game.setActivateObjectPhysicsAsSoonAsItIsUnfrozen(this.Object, true);
        game.freezeEntityPosition(this.Object, true);
        game.setEntityInvincible(this.Object, true);
        game.placeObjectOnGroundProperly(this.Object);
        this.Position = position;
/*        this.Marker = game.drawMarker(2, parseFloat(position.x + ""), parseFloat(position.y + ""), parseFloat("" + position.z), 0, 0, 0,
            0, 0, 0,
            1, 1, 1,
            255, 255, 255, 100, true,
            true, 0, false, undefined, undefined, false);*/

        this.Blip = new alt.PointBlip(position.X, position.Y, position.Z);
        this.Blip.sprite = 398;
        this.Blip.shortRange = true;
        this.Blip.name = "~r~Poubelle à récupérer";
        this.Blip.color = 1;
        this.Blip.scale = 0.5;
        alt.on("disconnect", this.unloadStream);
    }


    public Take = () => {
        game.attachEntityToEntity(
            this.Object,
            alt.Player.local.scriptID,
            game.getPedBoneIndex(alt.Player.local.scriptID, 6286),
            0, 0, 0, 0, -120, 0,
            true, true, false, false, 0, true
        );
        this.Blip.destroy();

    };

    public unloadStream = () => {
        if (this.Object != null)
            game.deleteObject(this.Object);
    }

}