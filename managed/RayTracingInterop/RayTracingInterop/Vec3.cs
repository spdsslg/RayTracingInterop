namespace RayTracingInterop;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vec3
{
    public readonly double x, y, z;
    public Vec3(double x, double y, double z) => (this.x, this.y, this.z) = (x, y, z);
}
