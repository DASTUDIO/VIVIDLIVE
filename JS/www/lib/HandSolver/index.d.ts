import { Results, THand } from "../Types";
/** Class representing hand solver. */
export declare class HandSolver {
    constructor();
    /**
     * Calculates finger and wrist as euler rotations
     * @param {Array} lm : array of 3D hand vectors from tfjs or mediapipe
     * @param {String} side: "Left" or "Right"
     */
    static solve(lm: Results, side?: "Left" | "Right"): THand<typeof side> | undefined;
}
