shader_type canvas_item;

uniform vec4 color_main = vec4(1,1,1,1); 

void fragment() {
	COLOR = color_main * texture(TEXTURE, UV);
}

