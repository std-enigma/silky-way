#version 330 core

out vec4 FragColor;
uniform float uTime;

// HSV to RGB conversion
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main(){
    // Color changes over time: hue from 0..1, saturation & value at 1
    float s = 0.6; // color-changing speed
    float hue = mod(uTime * s, 1.0);
    vec3 color = hsv2rgb(vec3(hue, 1.0, 1.0));
    FragColor = vec4(color, 1.0);
}
