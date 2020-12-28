shader_type canvas_item;

uniform vec4 border_color = vec4(1,1,1,1);
uniform sampler2D creature_art;
uniform sampler2D creature_mask;

void fragment() {
	COLOR = texture(creature_art, UV) * texture(creature_mask, UV);
	vec4 border = border_color * texture(TEXTURE, UV);	
	COLOR = mix(COLOR, border, border.a);	
}