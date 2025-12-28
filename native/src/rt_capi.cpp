//code for this file was taken and slightly modified from https://github.com/utilForever/ray-tracing-in-one-weekend-cpp/blob/master/src/main.cpp


#include "rt_capi.h"

#include "common.h"
#include "book/hittable_list.h"
#include "camera.h"
#include "dielectric.h"
#include "material.h"
#include "metal.h"
#include "ray.h"
#include "sphere.h"
#include "lambertian.h"
#include "vec3.h"

#include <algorithm>
#include <cstddef>
#include <memory>
#include <stdint.h>
#include<cmath>
using namespace std;

struct WorldWrap {
    hittable_list world;
};

struct MaterialWrap {
    shared_ptr<material> mat;
};

struct CameraWrap {
    camera cam;
    CameraWrap(const camera& c) : cam(c) {}
};

vec3 to_vec3(rt_vec3 v) { 
    return vec3{v.x, v.y, v.z}; 
}

//float [0;1] to byte [0;255]
uint8_t to_u8(double x){
    x = max(0.0, min(1.0,x));
    return (uint8_t)(x*255.999);
}

vec3 ray_color(const ray& r, const hittable& world, int depth)
{
    hit_record rec;

    if (depth <= 0) {
        return vec3{0, 0, 0};
    }

    if (world.hit(r, 0.001, infinity, rec)) {
        ray scattered;
        vec3 attenuation;

        if (rec.mat_ptr->scatter(r, rec, attenuation, scattered)) {
            return attenuation * ray_color(scattered, world, depth - 1);
        }
        return vec3{0, 0, 0};
    }

    const vec3 unit_direction = unit_vector(r.direction());
    const auto t = 0.5 * (unit_direction.y() + 1.0);
    return (1.0 - t) * vec3{1.0, 1.0, 1.0} + t * vec3{0.5, 0.7, 1.0};
}


//this function is for creating the image from the cover
hittable_list random_scene()
{
    hittable_list world;

    world.add(std::make_shared<sphere>(
        vec3{0, -1000, 0}, 1000,
        make_shared<lambertian>(vec3{0.5, 0.5, 0.5})));

    for (int a = -11; a < 11; ++a) {
        for (int b = -11; b < 11; ++b) {
            auto choose_mat = random_double();
            vec3 center{a + 0.9 * random_double(), 0.2, b + 0.9 * random_double()};

            if ((center - vec3{4, 0.2, 0}).length() > 0.9) {
                if (choose_mat < 0.8) {
                    auto a = vec3::random() * vec3::random();
                    world.add(std::make_shared<sphere>(
                        center, 0.2, std::make_shared<lambertian>(a)));
                } else if (choose_mat < 0.95) {
                    auto a = vec3::random(0.5, 1);
                    auto b = random_double(0, 0.5);
                    world.add(make_shared<sphere>(
                        center, 0.2, make_shared<metal>(a, b)));
                } else {
                    world.add(make_shared<sphere>(
                        center, 0.2, make_shared<dielectric>(1.5)));
                }
            }
        }
    }

    world.add(make_shared<sphere>(vec3{0, 1, 0}, 1.0,
                                       make_shared<dielectric>(1.5)));

    world.add(make_shared<sphere>(
        vec3{-4, 1, 0}, 1.0,
        make_shared<lambertian>(vec3(0.4, 0.2, 0.1))));

    world.add(make_shared<sphere>(
        vec3{4, 1, 0}, 1.0, make_shared<metal>(vec3(0.7, 0.6, 0.5), 0.0)));

    return world;
}


extern "C"{
rt_world_t rt_world_create() {
    return new WorldWrap{};
}

void rt_world_destroy(rt_world_t world){
    delete static_cast<WorldWrap*>(world);
}

void rt_world_clear(rt_world_t world){
    if(world == NULL){
        return;
    }
    static_cast<WorldWrap*>(world)->world=hittable_list{};
}

rt_material_t rt_material_lambertian(rt_vec3 a){
    auto* w = new MaterialWrap{};
    w->mat = make_shared<lambertian>(to_vec3(a));
    return w;
}

rt_material_t rt_material_metal(rt_vec3 a, double b) {
    auto* w = new MaterialWrap{};
    w->mat = make_shared<metal>(to_vec3(a), b);
    return w;
}

rt_material_t rt_material_dielectric(double c) {
    auto* w = new MaterialWrap{};
    w->mat = make_shared<dielectric>(c);
    return w;
}

void rt_material_destroy(rt_material_t material) {
    delete static_cast<MaterialWrap*>(material);
}

rt_camera_t rt_camera_create(rt_vec3 lookfrom, rt_vec3 lookat, rt_vec3 vup, double degrees, double aspect_ratio, double aperture, double focus_dist) {
    camera cam(to_vec3(lookfrom), to_vec3(lookat), to_vec3(vup), degrees, aspect_ratio, aperture, focus_dist);
    return new CameraWrap{cam};
}

void rt_camera_destroy(rt_camera_t cam) {
    delete static_cast<CameraWrap*>(cam);
}

int rt_world_add_sphere(rt_world_t world, rt_vec3 center, double radius, rt_material_t material) {
    if (world==NULL || material==NULL) return 1;
    auto* w = static_cast<WorldWrap*>(world);
    auto* m = static_cast<MaterialWrap*>(material);

    w->world.add(make_shared<sphere>(to_vec3(center), radius, m->mat));
    return 0;
}

int rt_render_scene(const rt_settings* s, rt_world_t world, rt_camera_t camera, uint8_t* out_rgba, int stride_bytes, rt_tile_callback cb){
    if(!s || !out_rgba){
        return 1;
    } 
    if(s->width<=0 || s->height<=0){
        return 2;
    }
    if(stride_bytes<s->width*4){ //to avoid overlap
        return 3;
    }

    int image_width  = s->width;
    int image_height = s->height;
    int samples_per_pixel = (s->samples_per_pixel > 0) ? s->samples_per_pixel : 10;
    int max_depth = (s->max_depth > 0) ? s->max_depth : 10;
    int tile = (s->tile_size > 0) ? s->tile_size : 32;

    auto* wwrap = static_cast<WorldWrap*>(world);
    auto* cwrap = static_cast<CameraWrap*>(camera);

    //render tiles
    for(int i_y=0;i_y<s->height;i_y+=tile){
        for(int j_x=0;j_x<s->width;j_x+=tile){
            int w = min(tile, s->width-j_x);
            int h = min(tile, s->height-i_y);
            
            //render one tile
            for(int y=i_y;y<i_y+h;y++){
                uint8_t* row = out_rgba + y*stride_bytes;
                int j = (image_height - 1) - y; //map y to j. main code from book repo renders j from height-1 down to 0
                for(int x=j_x;x<j_x+w;x++){
                    vec3 pixel_color{0, 0, 0};
                    for (int sp = 0; sp < samples_per_pixel; ++sp) {
                        const auto u = (x + random_double()) / image_width;
                        const auto v = (j + random_double()) / image_height;

                        ray r = cwrap->cam.get_ray(u, v);
                        pixel_color += ray_color(r, wwrap->world, max_depth);
                    }

                    double scale = 1.0 / samples_per_pixel;
                    double r = sqrt(scale * pixel_color.x());
                    double g = sqrt(scale * pixel_color.y());
                    double b = sqrt(scale * pixel_color.z());

                    uint8_t* px = row + x * 4;
                    px[0] = to_u8(r);
                    px[1] = to_u8(g);
                    px[2] = to_u8(b);
                    px[3] = 255;
                }
            }
            if(cb!=NULL){
                uint8_t* tile_ptr = out_rgba + i_y*stride_bytes + 4*j_x;
                cb(j_x, i_y, w,h,tile_ptr, stride_bytes);
            }
        }
    }
    return 0;
}

//function below is for creating the image from the cover

int rt_render(const rt_settings* s, uint8_t* out_rgba, int stride_bytes, rt_tile_callback cb){
    if(!s || !out_rgba){
        return 1;
    } 
    if(s->width<=0 || s->height<=0){
        return 2;
    }
    if(stride_bytes<s->width*4){ //to avoid overlap
        return 3;
    }

    int image_width  = s->width;
    int image_height = s->height;
    int samples_per_pixel = (s->samples_per_pixel > 0) ? s->samples_per_pixel : 10;
    int max_depth = (s->max_depth > 0) ? s->max_depth : 10;
    int tile = (s->tile_size > 0) ? s->tile_size : 32;

    auto aspect_ratio = static_cast<double>(image_width) / image_height;

    //scene+camera
    auto world = random_scene();

    vec3 lookfrom{13, 2, 3};
    vec3 lookat{0, 0, 0};
    vec3 vup{0, 1, 0};
    auto dist_to_focus = 10.0;
    auto aperture = 0.1;

    camera cam(lookfrom, lookat, vup, 20, aspect_ratio, aperture, dist_to_focus);

    //render tiles
    for(int i_y=0;i_y<s->height;i_y+=tile){
        for(int j_x=0;j_x<s->width;j_x+=tile){
            int w = min(tile, s->width-j_x);
            int h = min(tile, s->height-i_y);
            
            //render one tile
            for(int y=i_y;y<i_y+h;y++){
                uint8_t* row = out_rgba + y*stride_bytes;
                int j = (image_height - 1) - y; //map y to j. main code from book repo renders j from height-1 down to 0
                for(int x=j_x;x<j_x+w;x++){
                    vec3 pixel_color{0, 0, 0};
                    for (int sp = 0; sp < samples_per_pixel; ++sp) {
                        const auto u = (x + random_double()) / image_width;
                        const auto v = (j + random_double()) / image_height;

                        ray r = cam.get_ray(u, v);
                        pixel_color += ray_color(r, world, max_depth);
                    }

                    double scale = 1.0 / samples_per_pixel;
                    double r = sqrt(scale * pixel_color.x());
                    double g = sqrt(scale * pixel_color.y());
                    double b = sqrt(scale * pixel_color.z());

                    uint8_t* px = row + x * 4;
                    px[0] = to_u8(r);
                    px[1] = to_u8(g);
                    px[2] = to_u8(b);
                    px[3] = 255;
                }
            }
            if(cb!=NULL){
                uint8_t* tile_ptr = out_rgba + i_y*stride_bytes + 4*j_x;
                cb(j_x, i_y, w,h,tile_ptr, stride_bytes);
            }
        }
    }
    return 0;
}

}