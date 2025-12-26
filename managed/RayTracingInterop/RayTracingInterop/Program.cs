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
            samples_per_pixel = 10,
            max_depth = 10,
            seed = 123,
            tile_size = 32,
        };

        fixed (byte* pOut = rgba)
        {
            Native.RtSettings* ps = &s;

            int rc = Native.rt_render(ps, pOut, stride, &Native.OnTile);
            if (rc != 0)
            {
                Console.Error.WriteLine($"rt_render failed: {rc}");
                Environment.Exit(1);
            }
        }
        
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
