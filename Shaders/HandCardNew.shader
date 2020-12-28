shader_type canvas_item;

const vec4 CREATURE_ART_UV = vec4(0f, 0f, 1f, 0.85f);
const vec4 SPELL_ART_UV = vec4(-0.05f, -0.835f, 1.11f, 0.91f);

uniform sampler2D card_art;
uniform sampler2D card_mask;
uniform bool card_is_creature = true;

void fragment() {
	
	vec2 artUV;
	if (card_is_creature) {
		artUV = UV * CREATURE_ART_UV.za + CREATURE_ART_UV.xy;
	} else {
		artUV = UV * SPELL_ART_UV.za + SPELL_ART_UV.xy;
	}
	
	vec4 art = texture(card_art, artUV) * texture(card_mask, UV).a;
	vec4 tex = texture(TEXTURE, UV);
	COLOR = mix(art, tex, tex.a);
	COLOR.a = clamp(art.a + tex.a, 0, 1);
	float average = (COLOR.r + COLOR.g + COLOR.b)/3f;
	COLOR.rgb = vec3(average, average, average);
	
}