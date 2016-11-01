#pragma once
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>

#define FOURCC_DXT1 0x31545844 // Equivalent to "DXT1" in ASCII
#define FOURCC_DXT3 0x33545844 // Equivalent to "DXT3" in ASCII
#define FOURCC_DXT5 0x35545844 // Equivalent to "DXT5" in ASCII

struct texture
{
	GLuint id;
	int width;
	int height;
};

void createTexture(texture* texture);
void destroyTexture(texture* texture);
void loadBmpTexture(texture* texture, const char* fileName);
void setData(texture* texture, void* data, int width, int height);
void enableTexture(texture* texture, int samplerNumber);