using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace RayTracingInterop;

public static unsafe class Native
{
    private const string Lib = "rtweekend";
    
    [DllImport(Lib, EntryPoint = "rt_world_create")]
    internal static extern WorldHandle WorldCreate();
    
    [DllImport(Lib, EntryPoint = "rt_world_destroy")]
    internal static extern void WorldDestroy(IntPtr world);

    [DllImport(Lib, EntryPoint = "rt_world_clear")]
    internal static extern void WorldClear(WorldHandle world);
    
    [DllImport(Lib, EntryPoint = "rt_world_add_sphere")]
    internal static extern int WorldAddSphere(WorldHandle world, Vec3 center, double radius, MaterialHandle material);
    
    [DllImport(Lib, EntryPoint = "rt_material_lambertian")]
    internal static extern MaterialHandle MaterialLambertian(Vec3 a);
    
    [DllImport(Lib, EntryPoint = "rt_material_metal")]
    internal static extern MaterialHandle MaterialMetal(Vec3 a, double b);
    
    [DllImport(Lib, EntryPoint = "rt_material_dielectric")]
    internal static extern MaterialHandle MaterialDielectric(double refIdx);
    
    [DllImport(Lib, EntryPoint = "rt_material_destroy")]
    public static extern void MaterialDestroy(IntPtr material);
    
    
    [DllImport(Lib, EntryPoint = "rt_render_scene")]
    internal static extern int RenderScene(
        RenderSettings* settings,
        Camera* camera,
        WorldHandle world,
        byte* outRgba,
        int strideBytes,
        delegate* unmanaged[Cdecl]<int, int, int, int, byte*, int, void> callback
    );

    //makes c# method callable from native code
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static void OnTile(int x, int y, int w, int h, byte* rgba, int stride)
    {
        try
        {
            Console.WriteLine($"Tile ({x},{y}) {w}x{h}");
        }
        catch
        {
            //silence
        }
    }
}