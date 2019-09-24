import * as alt from 'alt';
import * as game from 'natives';
import * as utils from './Utils/utils';
import * as ui from './Helpers/UiHelper';

class LSPDCall
{

    public ID: string;
    public BlessePlayer: alt.Player;
    public Position: alt.Vector3;
    public Blip: alt.Blip;

    constructor(player: alt.Player, id: string, position: alt.Vector3, message: string)
    {
        this.ID = id;
        this.BlessePlayer = player;
        this.Position = position;

        this.Blip = new alt.PointBlip(parseFloat(position.x + ""), parseFloat(position.y + ""), parseFloat(position.z + ""))
        this.Blip.sprite = 280;
        this.Blip.color = 1;
        this.Blip.name = id;
        this.Blip.scale = 1;
        this.Blip.shrinked = true;
        this.Blip.shortRange = false;

        alt.emit("Display_subtitle", "~r~URGENCE: ~w~" + message, 30000);

        alt.setInterval(() => {
            this.Blip.destroy();
        }, 10 * 60000);
    }
}

export class LSPDManager
{
    constructor()
    {
        alt.onServer("LSPD_Call", (player: alt.Player, name: string, position: any, message: string) => {
            position = JSON.parse(position);
            new LSPDCall(player, name, new alt.Vector3(position.X, position.Y, position.Z), message);
        });
    }
}