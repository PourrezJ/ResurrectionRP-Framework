import * as alt from 'alt';
import * as native from 'natives';

alt.log(`test ...`);
alt.on('playerConnect', (player) => {
  chat.broadcast(`{5555AA}${player.name} {FFFFFF}connected`);
  alt.log(`${player.name} connected`);
});