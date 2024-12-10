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

type DiceInfo = { Mesh: obj; mutable Value: int }
let mutable diceList: DiceInfo list = []


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

    if dice <> null then
        let rec generatePosition () =
            let randomX = (System.Random().NextDouble() - 0.5) * 8.0
            let randomY = (System.Random().NextDouble() - 0.5) * 8.0

            let collision = diceList |> List.exists (fun existingDice ->
                let existingX = unbox<float> (existingDice.Mesh?position?x)
                let existingY = unbox<float> (existingDice.Mesh?position?y)
                let distance = System.Math.Sqrt((randomX - existingX) ** 2.0 + (randomY - existingY) ** 2.0)
                distance < 1.5
            )

            if collision then generatePosition () else (randomX, randomY)

        let (randomX, randomY) = generatePosition ()
        dice?position?set(randomX, randomY, 0.0)

        // Add the new dice to the list and the scene
        scene?add(dice)
        diceList <- { Mesh = dice; Value = 0 } :: diceList
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

// ==========================================
// ====== Determine Visible Face of Dice ====
// ==========================================

let getVisibleFace (dice: obj) =
    // Use tolerance to manage float rounding issues
    let tolerance = 0.1

    let xRot = unbox<float> dice?rotation?x % (2.0 * System.Math.PI)
    let yRot = unbox<float> dice?rotation?y % (2.0 * System.Math.PI)

    // Compare rotations with faceRotations
    faceRotations
    |> List.mapi (fun i (rx, ry, _) ->
        let isVisible =
            System.Math.Abs(xRot - rx) < tolerance && System.Math.Abs(yRot - ry) < tolerance
        if isVisible then Some(i + 1) else None
    )
    |> List.choose id
    |> List.tryHead
    |> Option.defaultValue 0

// ==========================================
// ========= Count Total Face Value =========
// ==========================================

let countTotal () =
    let total = diceList |> List.sumBy (fun dice -> dice.Value)
    totalDisplay.textContent <- $"Total: {total}"





// ================================
// ====== Simulate Dice Roll ======
// ================================

let rollDice () =
    updateTargetRotations () 

    // Update dice rotations and track face values
    targetRotations <- diceList |> List.mapi (fun i diceInfo ->
        let result = System.Random().Next(1, 7)
        let (x, y, z) = faceRotations.[result - 1]

        // Reset rotation visually
        diceInfo.Mesh?rotation?set(0.0, 0.0, 0.0)
        diceInfo.Value <- result

        (x + System.Math.PI * 2.0, y + System.Math.PI * 2.0, z)
    )

    resultDisplay.textContent <- $"Rolling {List.length diceList} dice!"
    window.setTimeout(fun _ -> countTotal (), 2000) |> ignore


// ============================
// ====== Animation loop ======
// ============================

let rec animate () =
    window.requestAnimationFrame(fun _ -> animate())

    // Synchronize target rotations
    updateTargetRotations ()

    List.iteri (fun i diceInfo ->
        if diceInfo.Mesh <> null then
            let (targetX, targetY, targetZ) = targetRotations.[i]
            
            // Smoothly rotate the dice toward the target angles
            diceInfo.Mesh?rotation?x <- diceInfo.Mesh?rotation?x + (targetX - diceInfo.Mesh?rotation?x) * 0.1
            diceInfo.Mesh?rotation?y <- diceInfo.Mesh?rotation?y + (targetY - diceInfo.Mesh?rotation?y) * 0.1
            diceInfo.Mesh?rotation?z <- diceInfo.Mesh?rotation?z + (targetZ - diceInfo.Mesh?rotation?z) * 0.1
    ) diceList
    
    renderer?render(scene, camera) |> ignore


animate ()

let ensureElementById (id: string) =
    match document.getElementById(id) with
    | null ->
        let element: Browser.Types.HTMLElement = unbox (document.createElement("div"))
        element?id <- id
        document.body.appendChild(element) |> ignore
        element
    | existingElement -> unbox<Browser.Types.HTMLElement> existingElement

// ==================================
// ====== Create Display Element ======
// ==================================

let createDisplayElement (id: string) (bottomOffset: string) =
    let element = ensureElementById id
    element?style?position <- "absolute"
    element?style?bottom <- bottomOffset
    element?style?left <- "50%"
    element?style?transform <- "translateX(-50%)"
    element?style?fontSize <- "24px"
    element?style?color <- "#333"
    element?style?padding <- "10px"
    element?style?backgroundColor <- "#f9f9f9"
    element?style?borderRadius <- "8px"
    element?style?boxShadow <- "0px 4px 6px rgba(0, 0, 0, 0.1)"
    element

// =============================
// ===== Create DOM Displays =====
// =============================
let resultDisplay = createDisplayElement "result-display" "60px"
let totalDisplay = createDisplayElement "total-display" "100px"


// ==================================
// ====== Button Styling Helper ======
// ==================================

let styleButton (button: Browser.Types.HTMLElement) text (color: string) left =
    button.textContent <- text
    button?style?position <- "absolute"
    button?style?top <- "20px"
    button?style?left <- left
    button?style?padding <- "12px 24px"
    button?style?background <- $"linear-gradient(150deg, {color}, #cea800)"
    button?style?color <- "black"
    button?style?border <- "none"
    button?style?borderRadius <- "8px"
    button?style?cursor <- "pointer"
    button?style?boxShadow <- "0px 4px 6px rgba(0, 0, 0, 0.1)"


    // Add hover effect
    button?addEventListener("mouseenter", fun _ ->
        button?style?transform <- "scale(1.1)"
    )

    button?addEventListener("mouseleave", fun _ ->
        button?style?transform <- "scale(1)"
    )

    button?addEventListener("mousedown", fun _ ->
        button?style?transform <- "scale(0.95)"
        button?style?boxShadow <- "0px 2px 4px rgba(0, 0, 0, 0.2)"
    )

    button?addEventListener("mouseup", fun _ ->
        button?style?transform <- "scale(1)"
        button?style?boxShadow <- "0px 4px 6px rgba(0, 0, 0, 0.2)"
    )

// =============================
// ===== Create Buttons ========
// =============================

let rollButton = document.createElement("button")
let addDiceButton = document.createElement("button")

// Apply button styles
styleButton rollButton "Roll Dice" "#c0c0c0" "20px"
styleButton addDiceButton "Add Dice" "#B08d57" "140px"

// Assign button actions
rollButton.onclick <- fun _ -> rollDice ()
addDiceButton.onclick <- fun _ -> createDice ()

// Attach buttons to the DOM
document.body.appendChild(rollButton) |> ignore
document.body.appendChild(addDiceButton) |> ignore
