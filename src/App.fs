module App

open Fable.Core.JsInterop
open Browser.Dom

let THREE: obj = importAll "three"

let createNew<'T> (ctor: obj) (args: obj list) : 'T =
    emitJsExpr (ctor, args) "new $0(...$1)"

// =====================================
// ====== Create Three.js objects ======
// =====================================

let scene: obj = createNew THREE?Scene []
let camera: obj = createNew THREE?PerspectiveCamera [75.0; window.innerWidth / window.innerHeight; 0.1; 1000.0]
let renderer: obj = createNew THREE?WebGLRenderer []

renderer?setSize(window.innerWidth, window.innerHeight)
document.body.appendChild(renderer?domElement) |> ignore

// ==========================
// ====== Add lighting ======
// ==========================

let light: obj = createNew THREE?AmbientLight ["#ffffff"; 1.0]
scene?add(light)

let directionalLight: obj = createNew THREE?DirectionalLight ["#ffffff"; 1.0]
directionalLight?position?set(5.0, 5.0, 5.0)
scene?add(directionalLight)

// ===========================
// ====== Load textures ======
// ===========================

let loadTexture (path: string): obj =
    let textureLoader: obj = createNew THREE?TextureLoader []
    textureLoader?load(path)

let diceTextures: obj list =
    [ "textures/face1.png"; "textures/face2.png"; "textures/face3.png"
      "textures/face4.png"; "textures/face5.png"; "textures/face6.png" ]
    |> List.map loadTexture

diceTextures |> List.iter (fun texture -> Browser.Dom.console.log(texture))


// ============================================
// ====== Create materials with textures ======
// ============================================

let diceMaterials: obj list =
    diceTextures
    |> List.map (fun texture -> createNew THREE?MeshBasicMaterial [createObj ["map" ==> texture]])

// ============================================
// ====== List to keep track of all dice ======
// ============================================

let mutable diceList: obj list = []

// ============================================
// ====== Target rotations for each dice ======
// ============================================

let mutable targetRotations: (float * float * float) list = []

// ===============================================================
// ====== Function to update rotations for newly added dice ======
// ===============================================================

let updateTargetRotations () =
    let currentDiceCount = List.length diceList
    let currentRotationCount = List.length targetRotations

    // ====== Add default rotations for new dice ======

    if currentRotationCount < currentDiceCount then
        let additionalRotations =
            List.init (currentDiceCount - currentRotationCount) (fun _ -> (0.0, 0.0, 0.0))
        targetRotations <- targetRotations @ additionalRotations

// =======================================================
// ====== Create a new dice and add it to the scene ======
// =======================================================

let createDice () =
    let geometry = createNew THREE?BoxGeometry [1.0; 1.0; 1.0]
    let dice = createNew THREE?Mesh [geometry; diceMaterials |> List.toArray]

    // ====== Generate random position and ensure no overlap ======
    let rec generatePosition () =
        let randomX = (System.Random().NextDouble() - 0.5) * 8.0 // Adjust range to avoid edge collisions
        let randomY = (System.Random().NextDouble() - 0.5) * 8.0

        // Check for collision with existing dice
        let collision = diceList |> List.exists (fun existingDice ->
            let existingX = unbox<float> (existingDice?position?x) // Explicitly cast to float
            let existingY = unbox<float> (existingDice?position?y) // Explicitly cast to float
            let distance = System.Math.Sqrt((randomX - existingX) ** 2.0 + (randomY - existingY) ** 2.0)
            distance < 1.5 // Minimum distance to avoid overlap
        )

        if collision then generatePosition () else (randomX, randomY)

    let (randomX, randomY) = generatePosition ()
    dice?position?set(randomX, randomY, 0.0)

    // ====== Add the dice to the scene and list ======
    scene?add(dice)
    diceList <- dice :: diceList // Add the dice to the list

    // ====== Update targetRotations for the new dice ======
    updateTargetRotations ()


// ==============================
// ====== Add initial dice ======
// ==============================

createDice ()

// =================================
// ====== Position the camera ======
// =================================

camera?position?set(0.0, 0.0, 10.0)
camera?lookAt(scene?position)

// ===========================================
// ====== Face rotations for dice faces ======
// ===========================================

let faceRotations: (float * float * float) list =
    [     
      (0.0, -System.Math.PI / 2.0, 0.0) // { Face 1 }
      (0.0, System.Math.PI / 2.0, 0.0)  // { Face 2 }
      (System.Math.PI / 2.0, 0.0, 0.0)  // { Face 3 }
      (-System.Math.PI / 2.0, 0.0, 0.0) // { Face 4 }
      (0.0, 0.0, 0.0)                   // { Face 5 }
      (System.Math.PI, 0.0, 0.0) ]      // { Face 6 }

// ================================
// ====== Add result display ======
// ================================

let resultDisplay: Browser.Types.HTMLElement = 
    unbox (document.createElement("div"))

resultDisplay?style?position <- "absolute"
resultDisplay?style?bottom <- "60px"
resultDisplay?style?left <- "50%"
resultDisplay?style?transform <- "translateX(-50%)"
resultDisplay?style?fontSize <- "24px"
resultDisplay?style?color <- "#333"
document.body.appendChild(resultDisplay) |> ignore

// ================================
// ====== Simulate dice roll ======
// ================================

let rollDice () =
    updateTargetRotations () // { Ensure targetRotations matches diceList }
    targetRotations <- diceList |> List.map (fun _ ->
        let result = System.Random().Next(1, 7)
        let (x, y, z) = faceRotations.[result - 1]
        let extraSpin = 2.0 * System.Math.PI * (float (System.Random().Next(1, 3)))
        (x + extraSpin, y + extraSpin, z)
    )

    resultDisplay.textContent <- $"Rolling {List.length diceList} dice!"

// ============================
// ====== Animation loop ======
// ============================

let rec animate () =
    window.requestAnimationFrame(fun _ -> animate())

    // ====== Ensure targetRotations is always synchronized ======
    updateTargetRotations ()

    List.iteri (fun i dice ->
        let (targetX, targetY, targetZ) = targetRotations.[i]
        dice?rotation?x <- dice?rotation?x + (targetX - dice?rotation?x) * 0.1
        dice?rotation?y <- dice?rotation?y + (targetY - dice?rotation?y) * 0.1
        dice?rotation?z <- dice?rotation?z + (targetZ - dice?rotation?z) * 0.1
    ) diceList
    renderer?render(scene, camera) |> ignore

animate ()

// =============================
// ====== Add roll button ======
// =============================

let rollButton = document.createElement("button")
rollButton.textContent <- "Roll Dice"
rollButton?style?position <- "absolute"
rollButton?style?top <- "20px"
rollButton?style?left <- "20px" // { Align to the top-left corner }
rollButton?style?padding <- "10px 20px" // Add some padding for better appearance
rollButton?style?backgroundColor <- "#4CAF50" // Green color
rollButton?style?color <- "white" // White text
rollButton?style?border <- "none" // No border
rollButton?style?borderRadius <- "5px" // Rounded corners
rollButton?style?cursor <- "pointer" // Change cursor on hover
rollButton?style?boxShadow <- "0px 4px 6px rgba(0, 0, 0, 0.1)" // Add subtle shadow
rollButton.onclick <- fun _ -> rollDice ()
document.body.appendChild(rollButton) |> ignore


// ==================================
// ===== Add "Add Dice" button ======
// ==================================

let addDiceButton = document.createElement("button")
addDiceButton.textContent <- "Add Dice"
addDiceButton?style?position <- "absolute"
addDiceButton?style?top <- "20px"
addDiceButton?style?left <- "120px" 
addDiceButton?style?padding <- "10px 20px"
addDiceButton?style?backgroundColor <- "#007BFF" // Blue color
addDiceButton?style?color <- "white"
addDiceButton?style?border <- "none"
addDiceButton?style?borderRadius <- "5px"
addDiceButton?style?cursor <- "pointer"
addDiceButton?style?boxShadow <- "0px 4px 6px rgba(0, 0, 0, 0.1)"
addDiceButton.onclick <- fun _ -> createDice ()
document.body.appendChild(addDiceButton) |> ignore


