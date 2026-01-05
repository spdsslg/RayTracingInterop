namespace RayTracingInterop;

internal class Lambertian : Material
{
    public Lambertian(Vec3 a) : base(Native.MaterialLambertian(a)) {}
}