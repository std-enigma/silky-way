#version 330 core

uniform float uOffset;

layout (location = 0) in vec3 aPos;

void main(){
  gl_Position = vec4(aPos.x + uOffset, aPos.yz, 1.0);
}
