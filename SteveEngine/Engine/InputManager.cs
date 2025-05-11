using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class InputManager
    {
        private Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> previousKeyStates = new Dictionary<Keys, bool>();
        
        private Dictionary<MouseButton, bool> mouseButtonStates = new Dictionary<MouseButton, bool>();
        private Dictionary<MouseButton, bool> previousMouseButtonStates = new Dictionary<MouseButton, bool>();
        
        private Vector2 mousePosition = Vector2.Zero;
        private Vector2 previousMousePosition = Vector2.Zero;
        private Vector2 mouseDelta = Vector2.Zero;
        
        public InputManager()
        {
            // Initialize dictionaries with all keys and mouse buttons set to "not pressed"
            foreach (Keys key in System.Enum.GetValues(typeof(Keys)))
            {
                keyStates[key] = false;
                previousKeyStates[key] = false;
            }
            
            foreach (MouseButton button in System.Enum.GetValues(typeof(MouseButton)))
            {
                mouseButtonStates[button] = false;
                previousMouseButtonStates[button] = false;
            }
        }

        public void Update(KeyboardState keyboardState, MouseState mouseState)
        {
            // Update previous states
            previousKeyStates = new Dictionary<Keys, bool>(keyStates);
            previousMouseButtonStates = new Dictionary<MouseButton, bool>(mouseButtonStates);
            previousMousePosition = mousePosition;

            // Update current keyboard state
            foreach (Keys key in System.Enum.GetValues(typeof(Keys)))
            {
                try
                {
                    keyStates[key] = keyboardState.IsKeyDown(key);
                }
                catch
                {
                    // Skip keys that are out of range
                    keyStates[key] = false;
                }
            }

            // Update current mouse state
            foreach (MouseButton button in System.Enum.GetValues(typeof(MouseButton)))
            {
                try
                {
                    mouseButtonStates[button] = mouseState.IsButtonDown(button);
                }
                catch
                {
                    // Skip mouse buttons that are out of range
                    mouseButtonStates[button] = false;
                }
            }

            // Update mouse position and delta
            mousePosition = new Vector2(mouseState.X, mouseState.Y);
            mouseDelta = mousePosition - previousMousePosition;
        }


        public bool IsKeyPressed(Keys key)
        {
            return keyStates[key] && !previousKeyStates[key];
        }
        
        public bool IsKeyDown(Keys key)
        {
            return keyStates[key];
        }
        
        public bool IsKeyReleased(Keys key)
        {
            return !keyStates[key] && previousKeyStates[key];
        }
        
        public bool IsMouseButtonPressed(MouseButton button)
        {
            return mouseButtonStates[button] && !previousMouseButtonStates[button];
        }
        
        public bool IsMouseButtonDown(MouseButton button)
        {
            return mouseButtonStates[button];
        }
        
        public bool IsMouseButtonReleased(MouseButton button)
        {
            return !mouseButtonStates[button] && previousMouseButtonStates[button];
        }
        
        public Vector2 GetMousePosition()
        {
            return mousePosition;
        }
        
        public Vector2 GetMouseDelta()
        {
            return mouseDelta;
        }
    }
}
