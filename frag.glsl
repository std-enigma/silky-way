#version 330 core

uniform sampler2D uTexture;

in vec2 TexCoord;
in vec3 VertColor;

out vec4 FragColor;

void main(){
  FragColor = texture(uTexture, TexCoord) * vec4(VertColor, 1.0);
}
