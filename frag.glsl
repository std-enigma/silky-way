#version 330 core

in vec3 oPos;
out vec4 FragColor;

void main(){
  FragColor = vec4(oPos, 1.0);
}
