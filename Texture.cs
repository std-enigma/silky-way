using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

/// <summary>
/// Represents a 2D OpenGL texture loaded from an image file.
/// </summary>
class Texture : IDisposable
{
    private readonly GL _gl;
    private uint _handle;
    private bool disposedValue;

    /// <summary>
    /// Loads an image from file and creates an OpenGL 2D texture.
    /// </summary>
    /// <param name="gl">The OpenGL context.</param>
    /// <param name="path">The image file path.</param>
    public unsafe Texture(GL gl, string path)
    {
        _gl = gl;
        _handle = gl.GenTexture();
        Bind();

        // Load the image data
        using var img = Image.Load<Rgba32>(path);
        var imgData = new Rgba32[img.Width * img.Height];
        img.Mutate(image => image.Flip(FlipMode.Vertical));
        img.CopyPixelDataTo(imgData);

        // Send the image data to the GPU
        fixed (Rgba32* texData = imgData)
            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                (uint)img.Width,
                (uint)img.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                texData
            );

        // set the texture parameters
        SetParameters();

        // Unbind the texture
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>
    /// Loads an image from file and creates an OpenGL 2D texture.
    /// </summary>
    /// <param name="gl">The OpenGL context.</param>
    /// <param name="path">The image file path.</param>
    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        _gl = gl;
        _handle = gl.GenTexture();
        Bind();

        // Send the image data to the GPU
        fixed (byte* texData = data)
            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                texData
            );

        // set the texture parameters
        SetParameters();

        // Unbind the texture
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>
    /// Binds this texture to the specified texture unit.
    /// </summary>
    /// <param name="unit">The texture unit to bind (default is Texture0).</param>
    public void Bind(TextureUnit unit = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(unit);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    /// <summary>
    /// Sets the default texture parameters and generates mipmaps.
    /// </summary>
    private void SetParameters()
    {
        var textureWrapMode = (int)TextureWrapMode.Repeat;
        var textureMinFilter = (int)TextureMinFilter.Nearest;
        var textureMagFilter = (int)TextureMinFilter.Nearest;
        _gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            ref textureWrapMode
        );
        _gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            ref textureWrapMode
        );
        _gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            ref textureMinFilter
        );
        _gl.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            ref textureMagFilter
        );
        _gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    /// <summary>
    /// Disposes the OpenGL texture resource.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            if (_handle != 0)
            {
                _gl.DeleteTexture(_handle);
                _handle = 0;
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer to clean up the OpenGL texture if not already disposed.
    /// </summary>
    ~Texture()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes and deletes the texture.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
