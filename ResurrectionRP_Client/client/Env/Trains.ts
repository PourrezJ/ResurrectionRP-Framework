﻿import * as alt from 'alt';
import * as game from 'natives';
import * as utils from '../Utils/Utils';

let trains: Array<Train> = [];

class Train {
    public NetworkID: number;
    public Handle: number;
    public Type: number;
    public CurrentPos: alt.Vector3;
    public YouOwn: boolean = false;

    constructor(id: number, type: number, currentPos: alt.Vector3) {
        this.NetworkID = id;
        this.CurrentPos = currentPos;
        this.Type = type;

        this.Handle = game.createMissionTrain(this.Type, this.CurrentPos.x as number, this.CurrentPos.y as number, this.CurrentPos.z as number, true);
        game.setEntityAsMissionEntity(this.Handle, true, true);
        game.addBlipForEntity(this.Handle);

        if (!game.hasModelLoaded(game.getHashKey("s_m_m_lsmetro_01")))
            alt.loadModel(game.getHashKey("s_m_m_lsmetro_01"));

        game.createPedInsideVehicle(this.Handle, 26, game.getHashKey("s_m_m_lsmetro_01"), -1, false, true);

        alt.setInterval(() => {
            this.CurrentPos = game.getEntityCoords(this.Handle, true);
            if (this.YouOwn) {
                alt.emitServer("TrainManager_PosUpdate", this.NetworkID, this.CurrentPos.x, this.CurrentPos.y, this.CurrentPos.z);
            }
        }, 75);

        trains.push(this);
    }
}

export async function initialize()
{
    game.deleteAllTrains();

    await utils.Wait(100);

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

        if (!game.hasModelLoaded(game.getHashKey("metrotrain")))
            await utils.loadModelAsync(game.getHashKey("metrotrain"));

        if (!game.hasModelLoaded(game.getHashKey("tankercar")))
            await utils.loadModelAsync(game.getHashKey("tankercar"));

        let trainList = JSON.parse(strtrainList);
        trainList.forEach((data) => {
            new Train(data.NetworkID, data.Type, new alt.Vector3(data.CurrentPos.X, data.CurrentPos.Y, data.CurrentPos.Z));
        });
    });

    alt.onServer("Train_OwnUpdate", async (networkID: number, own: boolean) => {
        while (trains.length == 0)
            await utils.Wait(25);

        // (dé)attribution du train
        let train = trains.find(p => p.NetworkID == networkID);

        if (train == null) {
            alt.log(`train id ${networkID} is null | count: ${trains.length}`);
            return;
        }

        train.YouOwn = own;

        if (own)
            alt.log("Attribution du train: " + networkID);
    });

    alt.onServer("Train_PosUpdate", (networkID: number, pos: alt.Vector3) =>
    {
        let train = trains.find(p => p.NetworkID == networkID);

        if (train == null) {
            alt.log(`train id ${networkID} is null`);
            return;
        }

        let currentPos = game.getEntityCoords(train.Handle, true);

        if (utils.Distance(currentPos, pos) > 1.2)
            game.setMissionTrainCoords(train.Handle, pos.x as number, pos.y as number, pos.z as number);
    }); 
}