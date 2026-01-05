namespace RayTracingInterop;
using System;

public class Scene : IDisposable
{
    internal WorldHandle Handle { get; }

    public Scene()
    {
        Handle = Native.WorldCreate();
        if (Handle.IsInvalid) throw new InvalidOperationException("Failed to create native world.");
    }

    public void Clear() => Native.WorldClear(Handle);

    internal void AddSphere(Vec3 center, double radius, Material material)
    {
        if (material == null) throw new ArgumentNullException(nameof(material));
        if (material.Handle.IsInvalid) throw new ArgumentException("Invalid material handle.", nameof(material));

        int rc = Native.WorldAddSphere(Handle, center, radius, material.Handle);
        if (rc != 0) throw new InvalidOperationException($"WorldAddSphere failed: {rc}");
    }

    public unsafe int Render(
        Camera camera,
        RenderSettings settings,
        byte[] outRgba,
        delegate* unmanaged[Cdecl]<int, int, int, int, byte*, int, void> callback = null)
    {
        int stride = settings.width * 4;
        if (outRgba.Length < settings.height * stride)
            throw new ArgumentException("Output buffer too small.", nameof(outRgba));
        
        delegate* unmanaged[Cdecl]<int,int,int,int,byte*,int,void> cb = callback;
        if (cb == null)
            cb = &Native.OnTile;

        fixed (byte* pOut = outRgba)
        {
            RenderSettings* ps = &settings;
            Camera* pc = &camera;

            return Native.RenderScene(ps, pc, Handle, pOut, stride, cb);
        }
    }

    public void Dispose()
    {
        Handle.Dispose();
        GC.SuppressFinalize(this);
    }
}