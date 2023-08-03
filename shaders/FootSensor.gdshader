shader_type spatial;
uniform vec3 led_data[1862];
const int SIZE_Y = 49;
const int SIZE_X = 38;
void fragment() {
    int coord_y = int(floor(float(SIZE_Y) * UV.y));
    int coord_x = int(floor(float(SIZE_X) * UV.x));

    vec3 coord_color = led_data[(coord_y * SIZE_X + coord_x)];
    
    vec2 local_UV = vec2((UV.x - float(coord_x) * 1.0/float(SIZE_X))*float(SIZE_X),
        (UV.y - float(coord_y) * 1.0/float(SIZE_Y))*float(SIZE_Y));
    
    float dist = ((local_UV.x - 0.5)*(local_UV.x - 0.5) + (local_UV.y - 0.5)*(local_UV.y - 0.5));
    float factor = exp(-25.0*dist);
    
    ALBEDO = factor*coord_color;
	EMISSION = ALBEDO;
}