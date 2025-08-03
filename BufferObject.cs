using Silk.NET.OpenGL;

/// <summary>
/// Represents a strongly-typed OpenGL buffer object (e.g., VBO or EBO) in Silk.NET.
/// Handles creation, upload, binding, and disposal of GPU buffer resources.
/// </summary>
/// <typeparam name="TDataType">
/// The unmanaged value type stored in this buffer (e.g., float, uint, struct).
/// </typeparam>
class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private readonly GL _gl;
    private readonly BufferTargetARB _target;
    private uint _handle;
    private bool disposedValue;

    /// <summary>
    /// Creates and fills an OpenGL buffer with the provided data.
    /// </summary>
    /// <param name="gl">The GL context.</param>
    /// <param name="target">Buffer target (ArrayBuffer, ElementArrayBuffer, etc).</param>
    /// <param name="data">The data to upload to the GPU.</param>
    public unsafe BufferObject(GL gl, BufferTargetARB target, Span<TDataType> data)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _target = target;
        _handle = gl.GenBuffer();
        Bind();
        fixed (TDataType* bufData = data)
            gl.BufferData(
                target,
                (nuint)(data.Length * sizeof(TDataType)),
                bufData,
                BufferUsageARB.StaticDraw
            );
    }

    /// <summary>
    /// Binds this buffer to its target (binds VBO, EBO, etc).
    /// </summary>
    public void Bind()
    {
        _gl.BindBuffer(_target, _handle);
    }

    /// <summary>
    /// Disposes the GPU buffer resource.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            if (_handle != 0)
            {
                _gl.DeleteBuffer(_handle);
                _handle = 0;
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer for releasing unmanaged resources if Dispose was missed.
    /// </summary>
    ~BufferObject()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes and deletes the buffer.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
