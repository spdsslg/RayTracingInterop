using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class Native
{
    private const string Lib = "rtweekend";
    
    [StructLayout(LayoutKind.Sequential)]
    public struct RtVec3 { public double x, y, z; }

    [StructLayout(LayoutKind.Sequential)]
    public struct RtSettings
    {
        public int width;
        public int height;
        public int samples_per_pixel;
        public int max_depth;
        public int tile_size;
        public int seed;
    }
    
    public struct RtWorld { 
        public nint Handle; 
        public RtWorld(nint h) => Handle = h; 
    }
    
    public struct RtCamera { 
        public nint Handle; 
        public RtCamera(nint h) => Handle = h; 
    }
    
    public struct RtMaterial { 
        public nint Handle; 
        public RtMaterial(nint h) => Handle = h; 
    }
    
    [DllImport(Lib)]
    public static extern nint rt_world_create();
    [DllImport(Lib)]
    public static extern void rt_world_destroy(nint world);
    [DllImport(Lib)]
    public static extern void rt_world_clear(nint world);
    [DllImport(Lib)]
    public static extern nint rt_material_lambertian(RtVec3 a);
    [DllImport(Lib)]
    public static extern nint rt_material_metal(RtVec3 a, double b);
    [DllImport(Lib)]
    public static extern nint rt_material_dielectric(double c);
    [DllImport(Lib)]
    public static extern void rt_material_destroy(nint mat);
    [DllImport(Lib)]
    public static extern nint rt_camera_create(RtVec3 lookfrom, RtVec3 lookat, RtVec3 vup, double degrees, double aspect_ratio, double aperture, double focus_dist);
    [DllImport(Lib)]
    public static extern void rt_camera_destroy(nint cam);
    [DllImport(Lib)]
    public static extern int rt_world_add_sphere(nint world, RtVec3 center, double radius, nint mat);
    
    [DllImport(Lib)]
    public static extern int rt_render_scene(
        RtSettings* s,
        nint world,
        nint camera,
        byte* out_rgba,
        int stride_bytes,
        delegate* unmanaged[Cdecl]<int,int,int,int,byte*,int,void> cb
    );
    
    [DllImport(Lib)]
    public static extern int rt_render(
        RtSettings* s,
        byte* out_rgba,
        int stride_bytes,
        delegate* unmanaged[Cdecl]<int,int,int,int,byte*,int,void> cb
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