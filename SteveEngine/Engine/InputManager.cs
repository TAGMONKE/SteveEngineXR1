using System;
using System.Collections.Generic;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace SteveEngine
{
    public class InputManager : IDisposable
    {
        private readonly GameWindow _window;

        // Key and mouse states
        private readonly HashSet<Keys> _keysDown = new HashSet<Keys>();
        private readonly HashSet<Keys> _keysPressedThisFrame = new HashSet<Keys>();
        private readonly HashSet<Keys> _keysReleasedThisFrame = new HashSet<Keys>();

        private readonly HashSet<MouseButton> _mouseDown = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> _mousePressedThisFrame = new HashSet<MouseButton>();
        private readonly HashSet<MouseButton> _mouseReleasedThisFrame = new HashSet<MouseButton>();

        // Mouse movement and scroll
        public Vector2 MousePosition { get; private set; }
        public Vector2 MouseDelta { get; private set; }
        public float ScrollDelta { get; private set; }

        public InputManager(GameWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            HookEvents();
        }

        private void HookEvents()
        {
            _window.KeyDown += OnKeyDown;
            _window.KeyUp += OnKeyUp;
            _window.MouseDown += OnMouseDown;
            _window.MouseUp += OnMouseUp;
            _window.MouseMove += OnMouseMove;
            _window.MouseWheel += OnMouseWheel;
            _window.UpdateFrame += OnUpdateFrame;
        }

        private void OnKeyDown(KeyboardKeyEventArgs e)
        {
            // If first time down this frame
            if (_keysDown.Add(e.Key))
                _keysPressedThisFrame.Add(e.Key);
        }

        private void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (_keysDown.Remove(e.Key))
                _keysReleasedThisFrame.Add(e.Key);
        }

        private void OnMouseDown(MouseButtonEventArgs e)
        {
            if (_mouseDown.Add(e.Button))
                _mousePressedThisFrame.Add(e.Button);
        }

        private void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_mouseDown.Remove(e.Button))
                _mouseReleasedThisFrame.Add(e.Button);
        }

        private Vector2 _lastMousePosition;
        private void OnMouseMove(MouseMoveEventArgs e)
        {
            var newPos = new Vector2(e.X, e.Y);
            MouseDelta = newPos - _lastMousePosition;
            MousePosition = newPos;
            _lastMousePosition = newPos;
        }

        private void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScrollDelta += e.OffsetY;
        }

        private void OnUpdateFrame(FrameEventArgs e)
        {
            // Clear per-frame state at the end of update
            _keysPressedThisFrame.Clear();
            _keysReleasedThisFrame.Clear();

            _mousePressedThisFrame.Clear();
            _mouseReleasedThisFrame.Clear();

            ScrollDelta = 0;
            MouseDelta = Vector2.Zero;
        }

        // Query methods
        public bool IsKeyDown(Keys key) => _keysDown.Contains(key);
        public bool IsKeyPressed(Keys key) => _keysPressedThisFrame.Contains(key);
        public bool IsKeyReleased(Keys key) => _keysReleasedThisFrame.Contains(key);

        public bool IsAnyKeyDown(params Keys[] keys)
        {
            foreach (var k in keys)
                if (IsKeyDown(k)) return true;
            return false;
        }

        public bool AreAllKeysDown(params Keys[] keys)
        {
            foreach (var k in keys)
                if (!IsKeyDown(k)) return false;
            return keys.Length > 0;
        }

        public bool IsMouseButtonDown(MouseButton button) => _mouseDown.Contains(button);
        public bool IsMouseButtonPressed(MouseButton button) => _mousePressedThisFrame.Contains(button);
        public bool IsMouseButtonReleased(MouseButton button) => _mouseReleasedThisFrame.Contains(button);

        public void Dispose()
        {
            _window.KeyDown -= OnKeyDown;
            _window.KeyUp -= OnKeyUp;
            _window.MouseDown -= OnMouseDown;
            _window.MouseUp -= OnMouseUp;
            _window.MouseMove -= OnMouseMove;
            _window.MouseWheel -= OnMouseWheel;
            _window.UpdateFrame -= OnUpdateFrame;
        }
    }
}
