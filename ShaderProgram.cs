using System;
using System.IO;
using Silk.NET.OpenGL;

/// <summary>
/// Represents a compiled and linked OpenGL shader program in Silk.NET.
/// Handles loading, compiling, linking, usage, and disposal.
/// </summary>
class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private uint _handle;
    private bool disposedValue;

    /// <summary>
    /// Compiles and links vertex+fragment shaders from file paths.
    /// </summary>
    /// <param name="gl">The Silk.NET GL instance.</param>
    /// <param name="vertPath">Vertex shader file path.</param>
    /// <param name="fragPath">Fragment shader file path.</param>
    public ShaderProgram(GL gl, string vertSrcPath, string fragSrcPath)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _handle = gl.CreateProgram();

        // Compile shaders
        var vertShader = LoadShader(ShaderType.VertexShader, vertSrcPath);
        var fragShader = LoadShader(ShaderType.FragmentShader, fragSrcPath);

        // Attach and link program
        gl.AttachShader(_handle, vertShader);
        gl.AttachShader(_handle, fragShader);
        gl.LinkProgram(_handle);

        // Check link status
        gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status != (int)GLEnum.True)
        {
            var log = gl.GetProgramInfoLog(_handle);
            throw new Exception($"Program failed to link, with error: {log}");
        }

        // Cleanup: detach and delete individual shaders
        gl.DetachShader(_handle, vertShader);
        gl.DetachShader(_handle, fragShader);
        gl.DeleteShader(vertShader);
        gl.DeleteShader(fragShader);
    }

    /// <summary>
    /// Activates this shader program.
    /// </summary>
    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    /// <summary>
    /// Sets an integer uniform by name.
    /// </summary>
    public void SetUniform(string name, int value)
    {
        var loc = _gl.GetUniformLocation(_handle, name);
        if (loc == -1)
            throw new Exception($"{name} uniform not found on shader.");
        _gl.Uniform1(loc, value);
    }

    /// <summary>
    /// Sets a float uniform by name.
    /// </summary>
    public void SetUniform(string name, float value)
    {
        var loc = _gl.GetUniformLocation(_handle, name);
        if (loc == -1)
            throw new Exception($"{name} uniform not found on shader.");
        _gl.Uniform1(loc, value);
    }

    /// <summary>
    /// Loads, compiles, and returns a shader handle from a source file.
    /// Throws if shader compilation fails.
    /// </summary>
    private uint LoadShader(ShaderType type, string path)
    {
        var handle = _gl.CreateShader(type);
        var src = File.ReadAllText(path);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);

        // Check for compilation errors
        _gl.GetShader(handle, ShaderParameterName.CompileStatus, out int status);
        if (status != (int)GLEnum.True)
        {
            var log = _gl.GetShaderInfoLog(handle);
            throw new Exception($"Shader of type {type} failed to compile, with error: {log}");
        }
        return handle;
    }

    /// <summary>
    /// Disposes and deletes the shader program. Call when finished with this shader.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            if (_handle != 0)
            {
                _gl.DeleteProgram(_handle);
                _handle = 0;
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer to catch missed disposal.
    /// </summary>
    ~ShaderProgram()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes and deletes all OpenGL resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
