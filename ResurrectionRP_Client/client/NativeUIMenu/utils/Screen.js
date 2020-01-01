
import * as alt from 'alt-client';
import * as game from 'natives';

const screen = game.getActiveScreenResolution();

export const Screen = {
    width: screen[1],
    height: screen[2]
};
