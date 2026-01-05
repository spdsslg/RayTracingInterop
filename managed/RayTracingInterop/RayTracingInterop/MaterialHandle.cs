namespace RayTracingInterop;

internal class MaterialHandle: RtHandle
{
    protected override bool ReleaseHandle()
    {
        Native.MaterialDestroy(handle);
        return true;
    }
}
