#pragma once
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>

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