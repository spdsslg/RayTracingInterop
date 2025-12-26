using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static unsafe class Native
{
    private const string Lib = "rtweekend";

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

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)]
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