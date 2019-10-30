import * as alt from 'alt';
import * as game from 'natives';
import * as utils from '../Utils/Utils';

export function initialize()
{
    let trains: Array<Train> = [];

    game.deleteAllTrains();

    alt.onServer("LoadsAllTrains", async (strtrainList: string) => {

        if (!game.hasModelLoaded(game.getHashKey("freight")))
            await utils.loadModelAsync(game.getHashKey("freight"));

        if (!game.hasModelLoaded(game.getHashKey("freightcar")))
            await utils.loadModelAsync(game.getHashKey("freightcar"));

        if (!game.hasModelLoaded(game.getHashKey("freightgrain")))
            await utils.loadModelAsync(game.getHashKey("freightgrain"));

        if (!game.hasModelLoaded(game.getHashKey("freightcont1")))
            await utils.loadModelAsync(game.getHashKey("freightcont1"));

        if (!game.hasModelLoaded(game.getHashKey("freightcont2")))
            await utils.loadModelAsync(game.getHashKey("freightcont2"));

        if (!game.hasModelLoaded(game.getHashKey("freighttrailer")))
            await utils.loadModelAsync(game.getHashKey("freighttrailer"));


        trains = JSON.parse(strtrainList);
        alt.log("debug train: " + JSON.stringify(trains));

        trains.forEach((data) => {
            trains.push(new Train(data.Type, data.CurrentPos));
        });
    });


    class Train
    {
        public Handle: number;
        public Type: number;
        public CurrentPos: Vector3;

        constructor(type: number, currentPos: Vector3)
        {
            this.CurrentPos = currentPos;
            this.Type = type;


            alt.log("loading train " + JSON.stringify(this));
            this.Handle = game.createMissionTrain(this.Type, this.CurrentPos.x, this.CurrentPos.y, this.CurrentPos.z, true);
            game.setEntityAsMissionEntity(this.Handle, true, true);
            game.addBlipForEntity(this.Handle);
            alt.log("debug train: handle " + this.Handle);

            alt.setInterval(() => {
                this.CurrentPos = game.getEntityCoords(this.Handle, true);
                alt.log(JSON.stringify(this.CurrentPos));
            }, 15);
        }
    }
}