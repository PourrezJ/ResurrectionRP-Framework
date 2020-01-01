import * as alt from 'alt-client';
import * as game from 'natives';

const alpha = "abcdefghi";

export class Weedlabs
{
    constructor()
    {
        alt.onServer('Weedlabs_Enter', this.Weedlabs_Enter);
        alt.onServer('Weedlabs_Update', this.Weedlabs_Update);
    }

    private Weedlabs_Enter(json: string, advanced: boolean)
    {
        let weedZoneList = JSON.parse(json);
        let interiorID = game.getInteriorAtCoords(1051.491, -3196.536, -39.14842);

        if (game.isValidInterior(interiorID))
        {
            game.deactivateInteriorEntitySet(interiorID, "weed_drying");
            game.deactivateInteriorEntitySet(interiorID, "weed_production");
            game.deactivateInteriorEntitySet(interiorID, "weed_security_upgrade");
            game.deactivateInteriorEntitySet(interiorID, "weed_standard_equip");
            game.deactivateInteriorEntitySet(interiorID, "weed_upgrade_equip");


            game.activateInteriorEntitySet(interiorID, "weed_chairs");
            game.activateInteriorEntitySet(interiorID, "weed_drying");
            game.activateInteriorEntitySet(interiorID, "weed_production");

            if (advanced) 
                game.activateInteriorEntitySet(interiorID, "weed_upgrade_equip");
            else
                game.activateInteriorEntitySet(interiorID, "weed_standard_equip");

            for (let i = 0; i < weedZoneList.length; i++) {

                game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[i] + "_stage1");
                game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[i] + "_stage2");
                game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[i] + "_stage3");
                game.deactivateInteriorEntitySet(interiorID, "weed_hose" + alpha[i]);

                if (weedZoneList[i].GrowingState > 0) {
                    // Croissance
                    game.activateInteriorEntitySet(interiorID, "weed_growth" + alpha[i] + "_stage" + weedZoneList[i].GrowingState);

                    // Arrosage
                    if (weedZoneList[i].Spray) {
                        game.activateInteriorEntitySet(interiorID, "weed_hose" + alpha[i]);
                    }
                }
            }
            game.refreshInterior(interiorID);
        }
    }

    private Weedlabs_Update(json : string) {
        let lab = JSON.parse(json);
        let id = lab.ID;
        let interiorID = game.getInteriorAtCoords(1051.491, -3196.536, -39.14842);

        alt.log(`Weedlabs_Update ${json}`);

        if (game.isValidInterior(interiorID)) {

            game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[id] + "_stage1");
            game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[id] + "_stage2");
            game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[id] + "_stage3");
            game.deactivateInteriorEntitySet(interiorID, "weed_growth" + alpha[id] + "_stage" + lab.GrowingState);

            var state = `weed_growth${alpha[lab.ID]}_stage${lab.GrowingState}`;

            // Croissance
            game.activateInteriorEntitySet(interiorID, state);

            // Arrosage
            if (lab.Spray) 
                game.activateInteriorEntitySet(interiorID, "weed_hose" + alpha[id]);

            game.refreshInterior(interiorID);
        }
    }
}