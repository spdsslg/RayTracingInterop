namespace RayTracingInterop;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Camera
{
    public Vec3 lookfrom;
    public Vec3 lookat;
    public Vec3 vup;
    public double vfov_degrees;
    public double aspect_ratio;
    public double aperture;
    public double focus_dist;
}