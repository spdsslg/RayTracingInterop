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

RT_API int rt_render(const rt_settings* s, uint8_t* out_rgba, int stride_bytes, rt_tile_callback cb);

#ifdef __cplusplus
}
#endif
