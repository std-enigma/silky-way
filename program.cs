using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

class Program
{
    static uint _vao; // The vertex array object
    static uint _vbo; // The vertex buffer object
    static uint _program; // The shader program
    static GL? _gl; // OpenGL context
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
        _window.Closing += OnClose;

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

        // Create the opengl context
        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.Black);

        // Create the vertex array object
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        // Create the vertex buffer object
        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        // Add the vertices to the vertex buffer object
        var vertices = new float[] { -0.5f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.5f, 0.0f, 0.0f };
        fixed (float* bufData = vertices)
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(vertices.Length * sizeof(float)),
                bufData,
                BufferUsageARB.StaticDraw
            );

        // Create and compile the vertex shader
        var vertShaderSrc = File.ReadAllText("vert.glsl");
        var vertShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertShader, vertShaderSrc);
        _gl.CompileShader(vertShader);
        _gl.GetShader(vertShader, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int)GLEnum.True)
            throw new Exception(
                "Vertex shader failed to compile: " + _gl?.GetShaderInfoLog(vertShader)
            );

        // Create and compile the fragment shader
        var fragShaderSrc = File.ReadAllText("frag.glsl");
        var fragShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragShader, fragShaderSrc);
        _gl.CompileShader(fragShader);
        _gl.GetShader(fragShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int)GLEnum.True)
            throw new Exception(
                "Fragment shader failed to compile: " + _gl?.GetShaderInfoLog(fragShader)
            );

        // Create the shader program
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vertShader);
        _gl.AttachShader(_program, fragShader);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));

        // Delete the shaders
        _gl.DetachShader(_program, vertShader);
        _gl.DetachShader(_program, fragShader);
        _gl.DeleteShader(vertShader);
        _gl.DeleteShader(fragShader);

        // Define shader data mapping
        const uint posLoc = 0;
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(
            posLoc,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            null
        );

        // Unbind resources
        _gl?.BindVertexArray(0);
        _gl?.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }

    // Called every frame (for application logic)
    static void OnUpdate(double deltaTime) { }

    // Called every frame (for drawing logic)
    static unsafe void OnRender(double deltaTime)
    {
        // Clear the color channel
        _gl?.Clear(ClearBufferMask.ColorBufferBit);

        // Draw our beautiful triangle
        _gl?.BindVertexArray(_vao);
        _gl?.UseProgram(_program);
        _gl?.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    // Window was resized
    static void OnFrameBufferResize(Vector2D<int> newSize)
    {
        // Resize the viewport to the new size
        _gl?.Viewport(newSize);
    }

    // Program closing
    static void OnClose()
    {
        _gl?.DeleteBuffer(_vbo);
        _gl?.DeleteVertexArray(_vao);
        _gl?.DeleteProgram(_program);
    }

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
