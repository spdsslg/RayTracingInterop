#pragma once 
#include<stdint.h>

#ifdef _WIN32
#define RT_API __declspec(dllexport)
#else
#define RT_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C"{
#endif

typedef struct rt_vec3{
    double x,y,z;
}rt_vec3;

typedef struct rt_settings{
    int width;
    int height;
    int samples_per_pixel;
    int max_depth;
    int tile_size;
    int seed;
}rt_settings;

//for partial image after each finished tile
typedef void (*rt_tile_callback)(
    int x,
    int y,
    int w,
    int h,
    const uint8_t* rgba,
    int stride_bytes
);

//void* so that c# can communicate with c++ without knowing its memory layout
typedef void* rt_world_t;
typedef void* rt_camera_t;
typedef void* rt_material_t;

//for world
RT_API rt_world_t rt_world_create(void);
RT_API void rt_world_destroy(rt_world_t world);
RT_API void rt_world_clear(rt_world_t world);

//materials
RT_API rt_material_t rt_material_lambertian(rt_vec3 a);
RT_API rt_material_t rt_material_metal(rt_vec3 a, double b);
RT_API rt_material_t rt_material_dielectric(double c);
RT_API void rt_material_destroy(rt_material_t material);

RT_API rt_camera_t rt_camera_create(rt_vec3 lookfrom, rt_vec3 lookat, rt_vec3 vup, double degrees, double aspect_ratio, double aperture, double focus_dist);
RT_API void rt_camera_destroy(rt_camera_t cam);

RT_API int rt_world_add_sphere(rt_world_t world, rt_vec3 center, double radius, rt_material_t material);

RT_API int rt_render_scene(const rt_settings* s, rt_world_t world, rt_camera_t camera, uint8_t* out_rgba, int stride_bytes, rt_tile_callback cb);
RT_API int rt_render(const rt_settings* s, uint8_t* out_rgba, int stride_bytes, rt_tile_callback cb);

#ifdef __cplusplus
}
#endif
