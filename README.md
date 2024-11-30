# Fable 3D Dice Roller 🎲

A 3D dice roller built with **F#**, **Fable**, and **Three.js**. This project demonstrates how to create interactive 3D graphics in the browser using Fable to compile F# to JavaScript.

---

## Features ✨

- 🎲 **Realistic 3D dice rolling** with randomized rolls.
- 🖼️ **Textured dice faces** for an authentic look.
- 📸 Smooth animations and transitions.
- 🔦 Dynamic lighting and camera perspective.

---

## Installation 🚀

### Prerequisites

Ensure you have the following installed:
- [Node.js](https://nodejs.org/) (with `npm`)
- [.NET SDK](https://dotnet.microsoft.com/) (6.0 or later)

### Clone the Repository:

```bash
git clone https://github.com/your-username/Fable-3d-dice-roller.git
cd Fable-3d-dice-roller
```

### Install Dependencies:

```bash
npm install
```

### Run the Development Server:

```bash
npm start
```

Open your browser and navigate to http://localhost:8080/ to see the app in action.

## Usage 🛠️
Click the Roll Dice button to roll the dice.
The die will spin randomly and land on a face that matches the rolled number.
The result of the roll is displayed on the screen.
Project Structure 📂

````
Fable-3d-dice-roller/
│
├── public/
│   ├── index.html     # Entry point for the app
│   └── styles.css     # Optional: Custom styles
|   └── textures/      # Dice face textures (face1.png, face2.png, ...)
│
├── src/
│   ├── App.fs         # Main F# code for the app
│   ├── App.fsproj     # Fable project configuration
│   
│
├── package.json       # NPM configuration
├── webpack.config.js  # Webpack configuration
└── README.md          # Project documentation (you are here!)
````

## Technologies Used 🧰

Fable: F# to JavaScript compiler.
Three.js: 3D rendering library.
Webpack: Module bundler for modern JavaScript applications.

## How It Works 🔍

Dice Textures: Each face of the dice is represented by a PNG texture stored in the textures folder.
3D Scene: The app uses Three.js to render a 3D scene, including the dice, lights, and camera.
Roll Logic: A random number generator determines the rolled face, and the dice rotates smoothly to land on the correct face.
Additional Notes: You will need to create your own textures for a better die since mine were wiped up in five minutes with Gimp.

## Future Improvements 🚀

Add more dice types (e.g., D20, D12).
Enhance animations with physics-based rolls.
Create multiplayer support for board games.

## License 📄
This project is open-source and available under the MIT License. Feel free to use, modify, and share it!

## Acknowledgments 🙌

Fable for making F# in the browser possible.
Three.js for its powerful 3D rendering tools.
You for rolling the dice and having fun!

## Enjoy the game! 🎲✨
