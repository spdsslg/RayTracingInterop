namespace RayTracingInterop;

internal abstract class Material : System.IDisposable
{
    internal MaterialHandle Handle { get; }

    protected Material(MaterialHandle handle)
    {
        Handle = handle;
        if (Handle.IsInvalid) throw new System.InvalidOperationException("Failed to create native material.");
    }

    public void Dispose()
    {
        Handle.Dispose();
        System.GC.SuppressFinalize(this);
    }
}