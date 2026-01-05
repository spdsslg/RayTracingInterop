using System;
using System.IO;
namespace RayTracingInterop;

unsafe class Program
{
    public static void Main(string[] args)
    {
        int width = 400;
        int height = 225;
        int stride = 4*width;
        
        byte[] rgba = new byte[stride * height];
        
        var settings = new RenderSettings {
            width = width,
            height = height,
            samples_per_pixel = 50,
            max_depth = 10,
            tile_size = 32,
            seed = 0
        };
        
        var camera = new Camera {
            lookfrom = new Vec3(13,2,3),
            lookat = new Vec3(0,0,0),
            vup = new Vec3(0,1,0),
            vfov_degrees = 20,
            aspect_ratio = (double)width / height,
            aperture = 0.1,
            focus_dist = 10.0
        };
        
        using var scene = new Scene();
        using var ground = new Lambertian(new Vec3(0.5,0.5,0.5));
        using var glass = new Dielectric(1.5);

        scene.AddSphere(new Vec3(0,-1000,0), 1000, ground);
        scene.AddSphere(new Vec3(0,1,0), 1.0, glass);

        int rc = scene.Render(camera, settings, rgba);
        if (rc != 0) Console.Error.WriteLine($"Render failed: {rc}");
        
        SavePpm("out.ppm",rgba, width, height, stride);
        
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
