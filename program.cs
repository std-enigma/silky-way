using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

class Program
{
    static IWindow? _window; // Window instance
    static IInputContext? _input; // Input manager

    static void Main(string[] args)
    {
        // Create the window
        var options = WindowOptions.Default with
        {
            Title = "Silky-Way",
            Size = new Vector2D<int>(1280, 720),
        };
        _window = Window.Create(options);

        // Event hooks
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += OnFrameBufferResize;

        // Run the window
        _window.Run();
        _window.Dispose();
    }

    // Called once when window loads
    static unsafe void OnLoad()
    {
        // Fallback if the window initialization has failed
        if (_window is null)
            return;

        // Create the input context
        _input = _window.CreateInput();
        // Handle keyboard input
        for (int i = 0; i < _input?.Keyboards.Count; i++)
            _input.Keyboards[i].KeyDown += OnKeyDown;
        // Handle gamepad input
        for (int i = 0; i < _input?.Gamepads.Count; i++)
            _input.Gamepads[i].ButtonDown += OnButtonDown;
    }

    // Called every frame (for application logic)
    static void OnUpdate(double deltaTime) { }

    // Called every frame (for drawing logic)
    static unsafe void OnRender(double deltaTime) { }

    // Window was resized
    static void OnFrameBufferResize(Vector2D<int> newSize) { }

    // Any key pressed
    static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key is Key.Escape)
            _window?.Close(); // Quit on ESC
    }

    // Any gamepad button pressed
    static void OnButtonDown(IGamepad gamePad, Button button)
    {
        if (button.Name is ButtonName.Back)
            _window?.Close(); // Quit on BACK button
    }
}
