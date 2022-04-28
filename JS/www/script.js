import * as Kalidokit from "./lib";
import * as build from 'fast-json-stringify'

const stringify = build({
    title: 'data',
    type: 'object',
    properties: {
        face: {
            title: 'face',
            type: 'object',
            properties: {
                eye: { l: { type: 'number' }, r: { type: 'number' } },
                mouth: { x: { type: 'number' }, y: { type: 'number' }, shape: { A: { type: 'number' }, I: { type: 'number' }, U: { type: 'number' }, E: { type: 'number' }, O: { type: 'number' }, } },
                head: {
                    x: { type: 'number' }, y: { type: 'number' }, z: { type: 'number' }, width: { type: 'number' }, height: { type: 'number' },
                    position: { x: { type: 'number' }, y: { type: 'number' }, z: { type: 'number' } },
                    normalized: { x: { type: 'number' }, y: { type: 'number' }, z: { type: 'number' } },
                    degrees: { x: { type: 'number' }, y: { type: 'number' }, z: { type: 'number' } },
                },
                pupil: { x: { type: 'number' }, y: { type: 'number' } },
                brow: { type: 'number' },
            }
        },
        pose: {
            title: 'pose',
            type: 'array',
            properties: {
                anyOf: { x: { type: 'number' }, y: { type: 'number' }, z: { type: 'number' }, visibility: { type: 'number' } }
            }
        },
        type: { type: 'number' },
        zcode: { type: 'string' }
    }
});

window.onload = function () { window.sock = new WebSocket("ws://" + "127.0.0.1:8087" + "/"); };

document.getElementById("run").onclick = () => {
    let zdata = {};
    zdata["type"] = 3;
    zdata["zcode"] = document.getElementById("zcode").value;
    window.sock.send(stringify(zdata));
};

const SendResult = (results) => {

    let data = {};
    let riggedFace;
    let hasData = false;
    let hasFace = false;

    const faceLandmarks = results.faceLandmarks;
    const pose3DLandmarks = results.ea;

    if (faceLandmarks) {
        riggedFace = Kalidokit.Face.solve(faceLandmarks, {
            runtime: "mediapipe",
            video: videoElement,
        });

        data["type"] = 1;

        data["face"] = riggedFace;

        hasData = true;

        hasFace = true;
    }

    if (pose3DLandmarks) {

        if (hasFace) { data["type"] = 0; } else { data["type"] = 2; }

        data["pose"] = pose3DLandmarks;

        hasData = true;
    }

    if (hasData) if (window.sock != null) window.sock.send(stringify(data));

};

let videoElement = document.querySelector(".input_video"),
    guideCanvas = document.querySelector("canvas.guides");

const onResults = (results) => {
    drawResults(results);
    SendResult(results);
};

const holistic = new Holistic({
    locateFile: (file) => {
        return `./Holistic/${file}`;
    },
});

holistic.setOptions({
    modelComplexity: 1,
    smoothLandmarks: true,
    minDetectionConfidence: 0.7,
    minTrackingConfidence: 0.7,
    refineFaceLandmarks: true,
});

holistic.onResults(onResults);

const drawResults = (results) => {
    guideCanvas.width = videoElement.videoWidth;
    guideCanvas.height = videoElement.videoHeight;
    let canvasCtx = guideCanvas.getContext("2d");
    canvasCtx.save();
    canvasCtx.clearRect(0, 0, guideCanvas.width, guideCanvas.height);
    
    drawConnectors(canvasCtx, results.poseLandmarks, POSE_CONNECTIONS, {
        color: "#00cff7",
        lineWidth: 4,
    });
    drawLandmarks(canvasCtx, results.poseLandmarks, {
        color: "#ff0364",
        lineWidth: 2,
    });
    drawConnectors(canvasCtx, results.faceLandmarks, FACEMESH_TESSELATION, {
        color: "#C0C0C070",
        lineWidth: 1,
    });
    if (results.faceLandmarks && results.faceLandmarks.length === 478) {
        
        drawLandmarks(canvasCtx, [results.faceLandmarks[468], results.faceLandmarks[468 + 5]], {
            color: "#ffe603",
            lineWidth: 2,
        });
    }
    drawConnectors(canvasCtx, results.leftHandLandmarks, HAND_CONNECTIONS, {
        color: "#eb1064",
        lineWidth: 5,
    });
    drawLandmarks(canvasCtx, results.leftHandLandmarks, {
        color: "#00cff7",
        lineWidth: 2,
    });
    drawConnectors(canvasCtx, results.rightHandLandmarks, HAND_CONNECTIONS, {
        color: "#22c3e3",
        lineWidth: 5,
    });
    drawLandmarks(canvasCtx, results.rightHandLandmarks, {
        color: "#ff0364",
        lineWidth: 2,
    });
};

const camera = new Camera(videoElement, {
    onFrame: async () => {
        await holistic.send({ image: videoElement });
    },
    width: 640,
    height: 480,
});
camera.start();
