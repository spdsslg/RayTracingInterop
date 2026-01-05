namespace RayTracingInterop;

internal class WorldHandle : RtHandle
{
    protected override bool ReleaseHandle()
    {
        Native.WorldDestroy(handle);
        return true;
    }
}
