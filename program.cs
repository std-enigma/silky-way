using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

class Program
{
    static uint _vao1; // The first vertex array object
    static uint _vbo1; // The first vertex buffer object
    static uint _vao2; // The second vertex array object
    static uint _vbo2; // The second vertex buffer object
    static uint _program1; // The shader program
    static uint _program2; // The shader program
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
        _vao1 = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao1);

        // Create the vertex buffer object
        _vbo1 = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo1);

        const uint posLoc = 0;

        // Add the vertices to the vertex buffer object
        var vertices1 = new float[] { -0.9f, -0.5f, 0.0f, -0.0f, -0.5f, 0.0f, -0.45f, 0.5f, 0.0f };
        fixed (float* bufData = vertices1)
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(vertices1.Length * sizeof(float)),
                bufData,
                BufferUsageARB.StaticDraw
            );

        // Define shader data mapping
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(
            posLoc,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            null
        );

        // Create the vertex array object
        _vao2 = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao2);

        // Create the vertex buffer object
        _vbo2 = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo2);

        // Add the vertices to the vertex buffer object
        var vertices2 = new float[] { 0.0f, -0.5f, 0.0f, 0.9f, -0.5f, 0.0f, 0.45f, 0.5f, 0.0f };
        fixed (float* bufData = vertices2)
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(vertices2.Length * sizeof(float)),
                bufData,
                BufferUsageARB.StaticDraw
            );

        // Define shader data mapping
        _gl.EnableVertexAttribArray(posLoc);
        _gl.VertexAttribPointer(
            posLoc,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            null
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
        var fragShader1Src = File.ReadAllText("frag1.glsl");
        var fragShader1 = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragShader1, fragShader1Src);
        _gl.CompileShader(fragShader1);
        _gl.GetShader(fragShader1, ShaderParameterName.CompileStatus, out int f1Status);
        if (f1Status != (int)GLEnum.True)
            throw new Exception(
                "Fragment shader failed to compile: " + _gl?.GetShaderInfoLog(fragShader1)
            );

        var fragShader2Src = File.ReadAllText("frag2.glsl");
        var fragShader2 = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragShader2, fragShader2Src);
        _gl.CompileShader(fragShader2);
        _gl.GetShader(fragShader2, ShaderParameterName.CompileStatus, out int f2Status);
        if (f1Status != (int)GLEnum.True)
            throw new Exception(
                "Fragment shader failed to compile: " + _gl?.GetShaderInfoLog(fragShader2)
            );

        // Create the shader program
        _program1 = _gl.CreateProgram();
        _gl.AttachShader(_program1, vertShader);
        _gl.AttachShader(_program1, fragShader1);
        _gl.LinkProgram(_program1);
        _gl.GetProgram(_program1, ProgramPropertyARB.LinkStatus, out int l1Status);
        if (l1Status != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program1));

        _program2 = _gl.CreateProgram();
        _gl.AttachShader(_program2, vertShader);
        _gl.AttachShader(_program2, fragShader2);
        _gl.LinkProgram(_program2);
        _gl.GetProgram(_program2, ProgramPropertyARB.LinkStatus, out int l2Status);
        if (l2Status != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program2));

        // Delete the shaders
        _gl.DetachShader(_program1, vertShader);
        _gl.DetachShader(_program1, fragShader1);
        _gl.DetachShader(_program2, fragShader2);
        _gl.DeleteShader(vertShader);
        _gl.DeleteShader(fragShader1);
        _gl.DeleteShader(fragShader2);

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
        _gl?.UseProgram(_program1);
        _gl?.BindVertexArray(_vao1);
        _gl?.DrawArrays(PrimitiveType.Triangles, 0, 3);
        _gl?.UseProgram(_program2);
        _gl?.BindVertexArray(_vao2);
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
        _gl?.DeleteBuffer(_vbo1);
        _gl?.DeleteBuffer(_vbo2);
        _gl?.DeleteVertexArray(_vao1);
        _gl?.DeleteVertexArray(_vao2);
        _gl?.DeleteProgram(_program1);
        _gl?.DeleteProgram(_program2);
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
