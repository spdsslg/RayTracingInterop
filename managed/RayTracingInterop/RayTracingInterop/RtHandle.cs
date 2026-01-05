namespace RayTracingInterop;
using Microsoft.Win32.SafeHandles;

internal abstract class RtHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    protected RtHandle() : base(true) {}
}
