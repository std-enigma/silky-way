using System.Drawing;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

class Program
{
    static float blendFactor = 0.5f;
    static Texture? _texture1;
    static Texture? _texture2;
    static VertexArrayObject<float, uint>? _vao; // The vertex array object
    static BufferObject<float>? _vbo; // The vertex buffer object
    static BufferObject<uint>? _ebo; // The element array buffer object
    static ShaderProgram? _program; // The shader program
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

        // Create the texture
        _texture1 = new Texture(_gl, "container.jpg");
        _texture2 = new Texture(_gl, "awesomeface.png");
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Create the vertex buffer object
        var vertices = new float[]
        {
            -0.5f,
            0.5f,
            0.0f,
            0.0f,
            1.0f,
            0.5f,
            0.5f,
            0.0f,
            1.0f,
            1.0f,
            0.5f,
            -0.5f,
            0.0f,
            1.0f,
            0.0f,
            -0.5f,
            -0.5f,
            0.0f,
            0.0f,
            0.0f,
        };
        _vbo = new BufferObject<float>(_gl, BufferTargetARB.ArrayBuffer, vertices);

        // Create the element array buffer object
        var indicies = new uint[] { 0u, 1u, 2u, 2u, 3u, 0u };
        _ebo = new BufferObject<uint>(_gl, BufferTargetARB.ElementArrayBuffer, indicies);

        // Create the vertex array object
        _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

        // Create the shader program
        _program = new ShaderProgram(_gl, "vert.glsl", "frag.glsl");
        _program.Use();
        _program.SetUniform("uTexture1", 0);
        _program.SetUniform("uTexture2", 1);

        // Define shader data mapping
        _vao.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        _vao.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        // Unbind resources
        _gl?.UseProgram(0);
        _gl?.BindVertexArray(0);
        _gl?.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl?.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    // Called every frame (for application logic)
    static void OnUpdate(double deltaTime) { }

    // Called every frame (for drawing logic)
    static unsafe void OnRender(double deltaTime)
    {
        // Clear the color channel
        _gl?.Clear(ClearBufferMask.ColorBufferBit);

        // Draw our beautiful triangle
        _vao?.Bind();
        _program?.Use();
        _program?.SetUniform("uBlendFactor", blendFactor);
        _texture1?.Bind(TextureUnit.Texture0);
        _texture2?.Bind(TextureUnit.Texture1);
        _gl?.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);
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
        _ebo?.Dispose();
        _vbo?.Dispose();
        _vao?.Dispose();
        _program?.Dispose();
        _texture1?.Dispose();
        _texture2?.Dispose();
    }

    // Any key pressed
    static void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key is Key.Escape)
            _window?.Close(); // Quit on ESC
        if (key is Key.Up)
            blendFactor = Math.Clamp(blendFactor + 0.1f, 0f, 1f); // Increase the blending factor of two textures on the up arrow key
        if (key is Key.Down)
            blendFactor = Math.Clamp(blendFactor - 0.1f, 0f, 1f); // Decrease the blending factor of two textures on the up arrow key
    }

    // Any gamepad button pressed
    static void OnButtonDown(IGamepad gamePad, Button button)
    {
        if (button.Name is ButtonName.Back)
            _window?.Close(); // Quit on BACK button
        if (button.Name is ButtonName.DPadUp)
            blendFactor = Math.Clamp(blendFactor + 0.1f, 0f, 1f); // Increase the blending factor of two textures on the DPad-Up button
        if (button.Name is ButtonName.DPadDown)
            blendFactor = Math.Clamp(blendFactor - 0.1f, 0f, 1f); // Decrease the blending factor of two textures on the DPad-Down button
    }
}
