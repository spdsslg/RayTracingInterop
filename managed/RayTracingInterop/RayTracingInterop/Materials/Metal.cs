namespace RayTracingInterop;

internal class Metal : Material
{
    public Metal(Vec3 a, double b) : base(Native.MaterialMetal(a, b)) { }
}