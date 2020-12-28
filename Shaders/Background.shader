shader_type canvas_item;

void fragment() {
	
	COLOR = texture(TEXTURE, UV);
	
	vec2 aUV = UV - 0.5f;
	
	float linex = abs (aUV.x) * abs(aUV.x);
	float liney = abs(aUV.y) * abs(aUV.y);

	float across = clamp( 1f - (linex * liney * 40f) - .5f, 0f, 1f);
	float cir = 1f - clamp(length(aUV) * 2f, 0f, 1f);
	
	//COLOR.r = UV.x;
	COLOR.a = clamp(across * cir * 2f, 0f, 1f);

}

