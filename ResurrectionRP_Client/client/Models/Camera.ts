import * as alt from 'alt-client';
import * as game from 'natives';

enum CameraMoveType {
    Up,
    Down,
}

export class Camera {
    Handle: number;

    constructor(pos: Vector3, rot: Vector3, fov: number = 50) {
        this.Pos = pos;
        this.Rot = rot;
        this.Fov = fov;
        this.Handle = game.createCameraWithParams(game.getHashKey("DEFAULT_SCRIPTED_CAMERA"), this.Pos.x, this.Pos.y, this.Pos.z, this.Rot.x, this.Rot.y, this.Rot.z, this.Fov, true, 2);
    }

    Pos: Vector3;
    Rot: Vector3;
    Fov: number;

    SetActiveCamera(active: boolean) {
        game.setCamActive(this.Handle, active);
        game.renderScriptCams(active, false, 0, true, false, 0)
        if (active)
            game.setFocusPosAndVel(this.Pos.x, this.Pos.y, this.Pos.z, 100, 100, 1000);
        else
            game.clearFocus();
    }
    Destroy() {
        this.SetActiveCamera(false);
        game.destroyCam(this.Handle, true);
    }

    MoveToAir(moveTo: CameraMoveType, switchType: number, delay: number = 10000) {
        switch (moveTo) {
            case CameraMoveType.Up:
                game.displayHud(false);
                game.displayRadar(false);
                game.switchOutPlayer(game.playerId(), 0, switchType)
                break;
            case CameraMoveType.Down:
                break;
        }
    }

}