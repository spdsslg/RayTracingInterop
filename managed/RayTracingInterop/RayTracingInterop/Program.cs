using System;
using System.IO;

unsafe class Program
{
    public static void Main(string[] args)
    {
        int width = 400;
        int height = 225;
        int stride = 4*width;
        
        byte[] rgba = new byte[stride * height];

        var s = new Native.RtSettings
        {
            width = width,
            height = height,
            samples_per_pixel = 100,
            max_depth = 50,
            seed = 123,
            tile_size = 32,
        };
        
        nint world = Native.rt_world_create(); 
        nint ground = Native.rt_material_lambertian(new Native.RtVec3 { x=0.5, y=0.5, z=0.5 });
        Native.rt_world_add_sphere(world, new Native.RtVec3 { x=0, y=-1000, z=0 }, 1000, ground);
        nint glass = Native.rt_material_dielectric(1.5);
        Native.rt_world_add_sphere(world, new Native.RtVec3 { x=0, y=1, z=0 }, 1.0, glass);
        
        double aspect = (double)width / height;
        nint cam = Native.rt_camera_create(new Native.RtVec3{ x=13, y=2, z=3 }, new Native.RtVec3{ x=0, y=0, z=0 },
            new Native.RtVec3{ x=0, y=1, z=0 },
            20, aspect, 0.1, 10.0
        );

        fixed (byte* pOut = rgba)
        {
            Native.RtSettings* ps = &s;
            
            int rc = Native.rt_render_scene(ps, world, cam, pOut, stride, &Native.OnTile);
            if (rc != 0)
            {
                Console.Error.WriteLine($"rt_render failed: {rc}");
                Environment.Exit(1);
            }
        }
        
        SavePpm("out.ppm",rgba, width, height, stride);
        
        Native.rt_camera_destroy(cam);
        Native.rt_material_destroy(glass);
        Native.rt_material_destroy(ground);
        Native.rt_world_destroy(world);
    }

    static void SavePpm(string path, byte[] rgba, int width, int height, int stride)
    {
        using var fs = File.Create(path);
        using var bw = new StreamWriter(fs);
        
        bw.WriteLine("P3");
        bw.WriteLine($"{width} {height}");
        bw.WriteLine("255");

        for (int y = 0; y < height; y++)
        {
            int row =  y * stride;
            for (int x = 0; x < width; x++)
            {
                int i = row + x*4;
                int r = rgba[i];
                int g = rgba[i+1];
                int b = rgba[i+2];
                bw.WriteLine($"{r} {g} {b}");
            }
        }
    }
}
