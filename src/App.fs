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

// Add lighting
let light: obj = createNew THREE?AmbientLight ["#ffffff"; 1.0]
scene?add(light)

let directionalLight: obj = createNew THREE?DirectionalLight ["#ffffff"; 1.0]
directionalLight?position?set(5.0, 5.0, 5.0)
scene?add(directionalLight)

// Load textures
let loadTexture (path: string): obj =
    let textureLoader: obj = createNew THREE?TextureLoader []
    textureLoader?load(path)

let diceTextures: obj list =
    [ "textures/face1.png"; "textures/face2.png"; "textures/face3.png"
      "textures/face4.png"; "textures/face5.png"; "textures/face6.png" ]
    |> List.map loadTexture

diceTextures |> List.iter (fun texture -> Browser.Dom.console.log(texture))

// Create materials with textures
let diceMaterials: obj list =
    diceTextures
    |> List.map (fun texture -> createNew THREE?MeshBasicMaterial [createObj ["map" ==> texture]])

// Create the dice
let dice: obj =
    let geometry = createNew THREE?BoxGeometry [1.0; 1.0; 1.0]
    let mesh = createNew THREE?Mesh [geometry; diceMaterials |> List.toArray]
    scene?add(mesh)
    mesh

dice?position?set(0.0, 0.0, 0.0)

// Position the camera
camera?position?set(0.0, 0.0, 5.0)
camera?lookAt(scene?position)

// Face rotations for dice faces
// Screw this thing, took me like an hour to fix it.
let faceRotations: (float * float * float) list =
    [     
      (0.0, -System.Math.PI / 2.0, 0.0) //Face 1
      (0.0, System.Math.PI / 2.0, 0.0) // Face 2
      (System.Math.PI / 2.0, 0.0, 0.0) // Face 3
      (-System.Math.PI / 2.0, 0.0, 0.0) // Face 4
      (0.0, 0.0, 0.0)     // Face 5
      (System.Math.PI, 0.0, 0.0) ]      // Face 6

// Smooth rolling logic
let mutable targetRotationX = 0.0
let mutable targetRotationY = 0.0
let mutable targetRotationZ = 0.0

// Add result display
let resultDisplay: Browser.Types.HTMLElement = 
    unbox (document.createElement("div"))

resultDisplay?style?position <- "absolute"
resultDisplay?style?bottom <- "60px"
resultDisplay?style?left <- "50%"
resultDisplay?style?transform <- "translateX(-50%)"
resultDisplay?style?fontSize <- "24px"
resultDisplay?style?color <- "#333"
document.body.appendChild(resultDisplay) |> ignore

// Simulate dice roll
let rollDice () =
    // Generate a random number between 1 and 6
    let result = System.Random().Next(1, 7)
    let (x, y, z) = faceRotations.[result - 1] // Get the rotation for the face

    // Set target rotations
    targetRotationX <- x + 2.0 * System.Math.PI * (float (System.Random().Next(1, 3))) // Add extra spins for realism
    targetRotationY <- y + 2.0 * System.Math.PI * (float (System.Random().Next(1, 3)))
    targetRotationZ <- z

    // Update result display
    resultDisplay.textContent <- $"You rolled: {result}"


// Animation loop
let rec animate () =
    window.requestAnimationFrame(fun _ -> animate())
    dice?rotation?x <- dice?rotation?x + (targetRotationX - dice?rotation?x) * 0.1
    dice?rotation?y <- dice?rotation?y + (targetRotationY - dice?rotation?y) * 0.1
    dice?rotation?z <- dice?rotation?z + (targetRotationZ - dice?rotation?z) * 0.1
    renderer?render(scene, camera) |> ignore

animate ()

// Add roll button
let rollButton = document.createElement("button")
rollButton.textContent <- "Roll Dice"
rollButton.onclick <- fun _ -> rollDice ()
document.body.appendChild(rollButton) |> ignore
