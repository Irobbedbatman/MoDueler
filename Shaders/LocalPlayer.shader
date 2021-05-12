shader_type canvas_item;

uniform sampler2D mask_tex;
uniform sampler2D char_art;

void fragment() {
	vec4 mask = texture(mask_tex, UV);
	vec4 art = texture(char_art, UV) * ceil(mask.a);
	vec4 frame = texture(TEXTURE, UV);
	COLOR = mix(art, frame, frame.a);
}






