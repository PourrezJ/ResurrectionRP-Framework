import Color from "includes/NativeUIMenu/utils/Color";
import { Screen } from "includes/NativeUIMenu/utils/Screen";
import * as alt from 'alt';
import * as game from 'natives';
export default class Sprite {
    private _textureDict;
    public TextureName;
    public pos;
    public size;
    public heading;
    public color;
    public visible;

   constructor(textureDict, textureName, pos, size, heading = 0, color = new Color(255, 255, 255)) {
        this.TextureDict = textureDict;
        this.TextureName = textureName;
        this.pos = pos;
        this.size = size;
        this.heading = heading;
        this.color = color;
        this.visible = true;
    }
    LoadTextureDictionary() {       
        this.requestTextureDictPromise(this._textureDict).then((succ) => { });
    }
    requestTextureDictPromise(textureDict) {
        return new Promise((resolve, reject) => {    
            game.requestStreamedTextureDict(textureDict, true);
            let inter = alt.setInterval(() => {
                if (game.hasStreamedTextureDictLoaded(textureDict)) {
                    alt.clearInterval(inter);
                    return resolve(true);
                }
            }, 10);
        });
    }    
    set TextureDict(v) {
        this._textureDict = v;
        if (!this.IsTextureDictionaryLoaded)
            this.LoadTextureDictionary();
    }
    get TextureDict() {
        return this._textureDict;
    }
    get IsTextureDictionaryLoaded() {
        return game.hasStreamedTextureDictLoaded(this._textureDict);
    }
    Draw(textureDictionary, textureName, pos, size, heading, color, loadTexture) {
        textureDictionary = textureDictionary || this.TextureDict;
        textureName = textureName || this.TextureName;
        pos = pos || this.pos;
        size = size || this.size;
        heading = heading || this.heading;
        color = color || this.color;
		loadTexture = loadTexture || true;
			if (loadTexture) {
				if (!game.hasStreamedTextureDictLoaded(textureDictionary))
					game.requestStreamedTextureDict(textureDictionary, true);
			}
			const screenw = Screen.width;
			const screenh = Screen.height;
			const height = 1080.0;
			const ratio = screenw / screenh;
			const width = height * ratio;
			const w = this.size.Width / width;
			const h = this.size.Height / height;
			const x = this.pos.X / width + w * 0.5;
			const y = this.pos.Y / height + h * 0.5;
			game.drawSprite(textureDictionary, textureName, x, y, w, h, heading, color.R, color.G, color.B, color.A);
    }
}
