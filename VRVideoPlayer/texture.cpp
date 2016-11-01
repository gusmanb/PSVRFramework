#include "texture.h"
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <stdio.h>

void createTexture(texture* texture)
{
	texture->width = 0;
	texture->height = 0;
	glGenTextures(1, &texture->id);
	glBindTexture(GL_TEXTURE_2D, texture->id);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
	glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
}

void destroyTexture(texture* texture)
{
	glDeleteTextures(1, &texture->id);
}

void loadBmpTexture(texture* texture, const char* fileName)
{
	unsigned char header[54];
	unsigned int dataPos;
	unsigned int width, height;
	unsigned int imageSize;
	unsigned char * data;

	FILE * file;
	fopen_s(&file, fileName, "rb");
	
	if (!file)
		return;

	if (fread(header, 1, 54, file) != 54)
	{
		fclose(file);
		return;
	}

	if (header[0] != 'B' || header[1] != 'M')
	{
		fclose(file);
		return;
	}

	dataPos = *(int*)&(header[0x0A]);
	imageSize = *(int*)&(header[0x22]);
	width = *(int*)&(header[0x12]);
	height = *(int*)&(header[0x16]);

	if (imageSize == 0)    
		imageSize = width*height * 3;

	if (dataPos == 0)
		dataPos = 54;

	data = new unsigned char[imageSize];

	fread(data, 1, imageSize, file);
	fclose(file);

	createTexture(texture);
	setData(texture, data, width, height);
}

void setData(texture* texture, void* data, int width, int height)
{
	glBindTexture(GL_TEXTURE_2D, texture->id);

	if (texture->width != width || texture->height != height)
	{
		glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_BGR, GL_UNSIGNED_BYTE, data);
		texture->width = width;
		texture->height = height;
	}
	else
		glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_BGR, GL_UNSIGNED_BYTE, data);

}

void enableTexture(texture* texture, int samplerNumber)
{
	glBindTexture(GL_TEXTURE_2D, texture->id);
	glActiveTexture(GL_TEXTURE0 + samplerNumber);
}