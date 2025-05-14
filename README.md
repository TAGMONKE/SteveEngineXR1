# SteveEngine Documentation

Welcome to **SteveEngine**!  
SteveEngine is a flexible, Lua-driven 3D game engine built on .NET 6 and OpenTK.  
This guide will help you get started, create your first game, and understand the workflow for both development and release.

---

## 🚀 Getting Started

### 1. **Download and Run**
- Download the `net6.0` build folder (typically found at `SteveEngine\bin\Debug\net6.0`).
- No need to modify the engine source code—everything can be done via Lua scripting!

### 2. **Your Main Script: `game.lua`**
- The engine loads and runs the `game.lua` file found in the same folder as the executable.
- This script controls your game’s objects, logic, and behaviors.

---

## 📝 Editing Your Game

### **How to Script**
- Open `game.lua` in your favorite text editor.
- Use the provided Lua API to create and control GameObjects, components, and more.

#### **Example: A Rotating Cube**
```lua
function onStart()
    print("SteveEngine game started!")
    local cube = engine:CreateGameObject("Cube")
    cube:SetPosition(0, 0, 0)
    local meshRenderer = cube:AddComponent("MeshRenderer")
    local col = cube:AddComponent("Collider")
end
function onUpdate(deltaTime)
    local cube = GetGameObjectByName("Cube")
    if cube and cube.Transform and cube.Transform.Rotation then
        cube.Transform.Rotation.X = cube.Transform.Rotation.X + deltaTime
        cube.Transform.Rotation.Y = cube.Transform.Rotation.Y + deltaTime * 2
    end
end
```


### **Hot Reload**
- Save your changes to `game.lua` and restart the engine to see updates.

---

## 🧩 Components & GameObjects

- **GameObject**: The basic entity in your scene.
- **Components**: Add features to GameObjects (e.g., `MeshRenderer`, `Collider`, `Rigidbody`, `CharacterController`).

#### **Common Lua API Methods**
- `engine:CreateGameObject(name)`
- `gameObject:AddComponent("ComponentName")`
- `gameObject.Transform.Position` / `.Rotation` / `.Scale`
- `engine:CreateMaterial("shaderName")`
- `engine:LoadTexture("texturePath")`

#### **Input Example**
```lua
if Input.IsKeyDown(Keys.W) then
    local camPos = Camera.GetCPosition()
    camPos.Z = camPos.Z - 0.1
    Camera.SetPosition(camPos.X, camPos.Y, camPos.Z)
end
```

---

## 🖼️ Assets

- Place textures, models, and other assets in the same folder as your executable or reference them by path.
- Use `engine:LoadTexture("filename.png")` and assign to materials.

---

## ⚙️ Configuration: `config.json`

In the same folder as your `game.lua` and the executable, you’ll find (or can create) a `config.json` file. This file lets you tweak engine settings without modifying code.

### 🧾 Example `config.json`:
```json
{
  "WindowTitle": "My SteveEngine Game",
  "WindowWidth": 1280,
  "WindowHeight": 720,
  "State": "Maximized",
  "CameraPosition": "0, 1, -5",
  "CameraFov": 75,
  "CameraYaw": -90.0,
  "CameraPitch": 10.0,
  "DebugMode": true,
  "isXR": false,
  "VSync": true
}
```

### 🔧 Available Settings:

| Key               | Description                                                                 |
|-------------------|-----------------------------------------------------------------------------|
| `WindowTitle`     | Title of the game window.                                                  |
| `WindowWidth`     | Initial window width (in pixels).                                          |
| `WindowHeight`    | Initial window height (in pixels).                                         |
| `State`           | Window state on launch: `"Normal"`, `"Maximized"`, or `"Fullscreen"`.      |
| `CameraPosition`  | Default camera position as a string: `"x, y, z"` (e.g., `"0, 1, -5"`).     |
| `CameraFov`       | Field of view for the camera in degrees.                                   |
| `CameraYaw`       | Horizontal angle the camera faces by default.                              |
| `CameraPitch`     | Vertical angle the camera faces by default.                                |
| `DebugMode`       | If `true`, enables extra debug output in the console.                      |
| `isXR`            | Enables experimental VR mode if set to `true`. (Doesnt work atm            |
| `VSync`           | Enables vertical sync if `true`.                                           |

> 💡 This file is optional. If not present, default settings are used.

---

## 🛠️ Customizing the Engine

- You **can** modify the C# source code for advanced features, but it’s not required.
- The engine is designed to be fully usable out-of-the-box with just Lua scripting.

---

## 📦 Releasing Your Game

1. **Build and Test** your game using `game.lua` and your assets.
2. **Zip the entire `net6.0` folder** (or wherever your core files are).
3. **Distribute** the zip to your players—they just extract and run!

---

## 🧑‍💻 Advanced Usage

- You can add new components, systems, or features by editing the C# codebase.
- VR is work in progress and isnt finished, you may impliment it yourself (and please make a pull request? It makes my life easier!)
- For editor tools, build your own.

---

## ❓ FAQ

**Q: Do I need to know C#?**  
A: No! All gameplay can be done in Lua. C# is only needed for engine modifications.

**Q: Where do I put my assets?**  
A: In the same folder as the executable, or reference them by path in your Lua scripts.

**Q: How do I reset my game?**  
A: Edit and save `game.lua`, then restart the engine.

---

## 📚 Resources

- Example projects and scripts: See the `game.lua` in your build folder.
- For more advanced scripting, see the Lua API in `LuaBindings.cs`.

---

**Enjoy building with SteveEngine!**
