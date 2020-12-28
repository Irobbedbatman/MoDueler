shader_type canvas_item;

void fragment() {
	
	vec4 col = texture(TEXTURE, UV);
	
	COLOR = col;
	
	if (col.r + col.g + col.b < 0.15f)
		COLOR.a = 0f;
	
	
}