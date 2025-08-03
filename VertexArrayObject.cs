using Silk.NET.OpenGL;

/// <summary>
/// Represents a generic OpenGL Vertex Array Object with attached VBO and EBO.
/// </summary>
/// <typeparam name="TVertexType">
/// The unmanaged struct type for vertex data.
/// </typeparam>
/// <typeparam name="TIndextype">
/// The unmanaged struct type for element/index data.
/// </typeparam>
class VertexArrayObject<TVertexType, TIndextype> : IDisposable
    where TVertexType : unmanaged
    where TIndextype : unmanaged
{
    private readonly GL _gl;
    private uint _handle;
    private bool disposedValue;

    /// <summary>
    /// Creates a new vertex array object and binds the provided VBO and EBO.
    /// </summary>
    /// <param name="gl">The GL instance.</param>
    /// <param name="vbo">The vertex buffer (VBO).</param>
    /// <param name="ebo">The element buffer (EBO).</param>
    public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndextype> ebo)
    {
        _gl = gl ?? throw new ArgumentNullException(nameof(gl));
        _handle = gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo.Bind();
    }

    /// <summary>
    /// Binds this vertex array object.
    /// </summary>
    public void Bind()
    {
        _gl.BindVertexArray(_handle);
    }

    /// <summary>
    /// Specifies the format of a vertex attribute in this VAO.
    /// </summary>
    /// <param name="index">Attribute index.</param>
    /// <param name="count">Component count (e.g. 3 for vec3).</param>
    /// <param name="type">The attribute data type (e.g. Float).</param>
    /// <param name="vertexSize">Struct size in units of TVertexType.</param>
    /// <param name="offset">Offset in units of TVertexType.</param>
    public unsafe void VertexAttribPointer(
        uint index,
        int count,
        VertexAttribPointerType type,
        uint vertexSize,
        uint offset
    )
    {
        _gl.EnableVertexArrayAttrib(_handle, index);
        _gl.VertexAttribPointer(
            index,
            count,
            type,
            false,
            vertexSize * (uint)sizeof(TVertexType),
            (void*)(offset * sizeof(TVertexType))
        );
    }

    /// <summary>
    /// Disposes the VAO resource.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) { }
            if (_handle != 0)
            {
                _gl.DeleteVertexArray(_handle);
                _handle = 0;
            }
            disposedValue = true;
        }
    }

    /// <summary>
    /// Finalizer. Disposes if not already done.
    /// </summary>
    ~VertexArrayObject()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    /// Disposes and deletes the VAO.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
