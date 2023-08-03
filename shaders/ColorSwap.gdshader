shader_type spatial;
render_mode unshaded;
uniform sampler2D texture_albedo : source_color;
void fragment()
{
	vec4 albedo_tex = texture(texture_albedo, UV);
	ALBEDO = albedo_tex.bgr;
	ALPHA = albedo_tex.a;
}