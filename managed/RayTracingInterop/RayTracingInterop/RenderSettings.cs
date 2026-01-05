namespace RayTracingInterop;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct RenderSettings
{
    public int width;
    public int height;
    public int samples_per_pixel;
    public int max_depth;
    public int tile_size;
    public int seed;
}