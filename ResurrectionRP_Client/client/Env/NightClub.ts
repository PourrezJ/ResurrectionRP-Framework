import * as alt from 'alt-client';
import * as game from 'natives';

export function initialize() {
    game.requestIpl('ba_int_placement_ba_interior_0_dlc_int_01_ba_milo_');
    
    //let interiorID = game.getInteriorAtCoords(-219.3403, -296.4997, 24.46127);
    let interiorID = game.getInteriorAtCoords(-219.3403, -296.4997, 24.46127);
    game.pinInteriorInMemory(interiorID);
    if (game.isValidInterior(interiorID))
    {    
        //Name Paradise
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_clubname_09');
        //Style Elegance
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_Style03');
        //Podium Style 3
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_style03_podium');
        //Speakers with Upgrade
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_equipment_setup');
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_equipment_upgrade');
        //Security Upgrade
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_security_upgrade');
        //Turntables
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_dj03');
        //Light - Bands
        game.activateInteriorEntitySet(interiorID, 'DJ_04_Lights_03');
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_lightgrid_01');
        game.activateInteriorEntitySet(interiorID, 'DJ_01_Lights_01');

        //Bar + Dry Ice Machine
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_bar_content');
        //Trophy in VIP Room
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_trophy01');
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_trophy02');
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_trophy03');
        game.activateInteriorEntitySet(interiorID, 'Int01_ba_trophy04');

        game.refreshInterior(interiorID);

        alt.log('Nightclub loaded');
    }  
}