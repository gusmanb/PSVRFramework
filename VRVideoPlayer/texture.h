#pragma once
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>

#define FOURCC_DXT1 0x31545844 // Equivalent to "DXT1" in ASCII
#define FOURCC_DXT3 0x33545844 // Equivalent to "DXT3" in ASCII
#define FOURCC_DXT5 0x35545844 // Equivalent to "DXT5" in ASCII

namespace Texture
{

	typedef struct _texture
	{
		GLuint id;
		int width;
		int height;

	} textureData;

	void createTexture(textureData* textureData);
	void destroyTexture(textureData* textureData);
	void loadBmpTexture(textureData* textureData, const char* fileName);
	void setData(textureData* textureData, void* data, int width, int height);
	void enableTexture(textureData* textureData, int samplerNumber);
	void saveBmpTexture(textureData* textureData, const char* fileName);
}