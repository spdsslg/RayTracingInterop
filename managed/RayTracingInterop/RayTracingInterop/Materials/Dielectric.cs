namespace RayTracingInterop;

internal class Dielectric : Material
{
    public Dielectric(double refIdx) : base(Native.MaterialDielectric(refIdx)) { }
}