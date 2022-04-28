import Vector from "../utils/vector";
import { Results } from "../Types";
/**
 * Calculates arm rotation as euler angles
 * TODO: Make angles more accurate in all rotation axis
 * @param {Results} lm : array of 3D pose vectors from tfjs or mediapipe
 */
export declare const calcLegs: (lm: Results) => {
    UpperLeg: {
        r: Vector;
        l: Vector;
    };
    LowerLeg: {
        r: Vector;
        l: Vector;
    };
    Unscaled: {
        UpperArm: {
            r: Vector;
            l: Vector;
        };
        LowerLeg: {
            r: Vector;
            l: Vector;
        };
    };
};
/**
 * Converts normalized rotation values into radians clamped by human limits
 * @param {Object} UpperLeg : normalized rotation values
 * @param {Object} LowerLeg : normalized rotation values
 * @param {String} side : "Left" or "Right"
 */
export declare const rigLeg: (UpperLeg: Vector, LowerLeg: Vector, side?: string) => {
    UpperLeg: Vector;
    LowerLeg: Vector;
};
