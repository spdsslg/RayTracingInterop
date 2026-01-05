namespace RayTracingInterop;

internal sealed class Dielectric : Material
{
    public Dielectric(double refIdx) : base(Native.MaterialDielectric(refIdx)) { }
}