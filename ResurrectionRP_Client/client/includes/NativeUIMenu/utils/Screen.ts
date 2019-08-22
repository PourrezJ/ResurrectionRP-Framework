
import * as alt from 'alt';
import * as game from 'natives';

//const [_, x, y] = game.getActiveScreenResolution();
var width;
var height;
const screen = game.getActiveScreenResolution(width, height);

export const Screen = {
    width: width,
    height: height
};
