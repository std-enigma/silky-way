#version 330 core

uniform sampler2D uTexture1;
uniform sampler2D uTexture2;

in vec2 TexCoord;

out vec4 FragColor;

void main(){
  FragColor = mix(texture(uTexture1, TexCoord), texture(uTexture2, vec2(1.0 - TexCoord.x, TexCoord.y)) * vec4(0.4, 0.8, 0.9, 1.0), 0.4);
}
