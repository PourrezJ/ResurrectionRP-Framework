import { Player, Vector3 } from "alt";
import * as native from 'natives';

interface RaycastResultInterface {
    isHit: boolean;
    pos: Vector3;
    hitEntity: number;
    entityType: number;
    entityHash: number;
}

class Raycast {
    public static readonly player = Player.local;

    public static line(scale: number, flags: number, ignoreEntity: number) {
        let playerForwardVector = native.getEntityForwardVector(this.player.scriptID);
        playerForwardVector.x *= scale;
        playerForwardVector.y *= scale;
        playerForwardVector.z *= scale;

        let ray = native.startShapeTestRay(
            this.player.pos.x,
            this.player.pos.y,
            this.player.pos.z,
            this.player.pos.x + playerForwardVector.x,
            this.player.pos.y + playerForwardVector.y,
            this.player.pos.z + playerForwardVector.z,
            flags,
            ignoreEntity,
            undefined
        );

        return this.result(ray);
    }
    
    private static result(ray: any): RaycastResultInterface {
        let result = native.getShapeTestResult(ray, undefined, undefined, undefined, undefined);
        let hitEntity = result[4];
        return {
            isHit: result[1],
            pos: new Vector3(result[2].x, result[2].y, result[2].z),
            hitEntity,
            entityType: native.getEntityType(hitEntity),
            entityHash: native.getEntityModel(hitEntity)
        }
    }
}

export default Raycast;