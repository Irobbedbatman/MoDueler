shader_type canvas_item;

void fragment() {
	
	vec2 uvOffset = UV;
	uvOffset.x += TIME / 10f;
	
	vec4 tex = texture(TEXTURE, uvOffset);
	
	COLOR = tex;
	
	COLOR.a *= 1f-abs(UV.x - .5f) * 2f;

}