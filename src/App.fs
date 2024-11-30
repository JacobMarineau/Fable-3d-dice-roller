module App

open Fable.Core.JsInterop
open Browser.Dom

let THREE: obj = importAll "three"

let createNew<'T> (ctor: obj) (args: obj list) : 'T =
    emitJsExpr (ctor, args) "new $0(...$1)"

// Create Three.js objects
let scene: obj = createNew THREE?Scene []
let camera: obj = createNew THREE?PerspectiveCamera [75.0; window.innerWidth / window.innerHeight; 0.1; 1000.0]
let renderer: obj = createNew THREE?WebGLRenderer []

renderer?setSize(window.innerWidth, window.innerHeight)
document.body.appendChild(renderer?domElement) |> ignore

// Add a basic red cube for testing
let testCube: obj =
    let geometry = createNew THREE?BoxGeometry [1.0; 1.0; 1.0]
    let material = createNew THREE?MeshBasicMaterial [createObj ["color" ==> "#ff0000"]] // Red cube
    createNew THREE?Mesh [geometry; material]

scene?add(testCube)

// Position the camera
camera?position?set(0.0, 0.0, 5.0)
camera?lookAt(scene?position)

// Lighting (if needed)
let light: obj = createNew THREE?AmbientLight ["#ffffff"; 1.0]
scene?add(light)

// Animation loop
let rec animate () =
    window.requestAnimationFrame(fun _ -> animate())
    testCube?rotation?x <- testCube?rotation?x + 0.02
    testCube?rotation?y <- testCube?rotation?y + 0.02
    renderer?render(scene, camera) |> ignore

animate ()
