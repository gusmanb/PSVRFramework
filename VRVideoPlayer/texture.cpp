#include "texture.h"
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <stdio.h>
#define WIN32_LEAN_AND_MEAN             // Excluir material rara vez utilizado de encabezados de Windows
// Archivos de encabezado de Windows:
#include <windows.h>

namespace Texture
{

	void createTexture(textureData* textureData)
	{
		textureData->width = 0;
		textureData->height = 0;
		glGenTextures(1, &textureData->id);
		glBindTexture(GL_TEXTURE_2D, textureData->id);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
		glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
	}

	void destroyTexture(textureData* textureData)
	{
		glDeleteTextures(1, &textureData->id);
	}

	void loadBmpTexture(textureData* textureData, const char* fileName)
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

		createTexture(textureData);
		setData(textureData, data, width, height);
	}

	void setData(textureData* textureData, void* data, int width, int height)
	{
		glBindTexture(GL_TEXTURE_2D, textureData->id);

		if (textureData->width != width || textureData->height != height)
		{
			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_BGR, GL_UNSIGNED_BYTE, data);
			textureData->width = width;
			textureData->height = height;
		}
		else
			glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, width, height, GL_BGR, GL_UNSIGNED_BYTE, data);

	}

	void enableTexture(textureData* textureData, int samplerNumber)
	{
		glBindTexture(GL_TEXTURE_2D, textureData->id);
		glActiveTexture(GL_TEXTURE0 + samplerNumber);
	}

	void saveBmpTexture(textureData* textureData, const char* fileName)
	{
		FILE *file;
		unsigned long imageSize;
		GLbyte *data = NULL;
		GLenum lastBuffer;
		BITMAPFILEHEADER bmfh;
		BITMAPINFOHEADER bmih;
		bmfh.bfType = 'MB';
		bmfh.bfReserved1 = 0;
		bmfh.bfReserved2 = 0;
		bmfh.bfOffBits = 54;

		imageSize = ((textureData->width + ((4 - (textureData->width % 4)) % 4))* textureData->height * 3) + 2;
		bmfh.bfSize = imageSize + sizeof(bmfh) + sizeof(bmih);
		data = (GLbyte*)malloc(imageSize);

		glPixelStorei(GL_PACK_ALIGNMENT, 4);
		glPixelStorei(GL_PACK_ROW_LENGTH, 0);
		glPixelStorei(GL_PACK_SKIP_ROWS, 0);
		glPixelStorei(GL_PACK_SKIP_PIXELS, 0);
		glPixelStorei(GL_PACK_SWAP_BYTES, 1);

		glBindTexture(GL_TEXTURE_2D, textureData->id);

		glGetTexImage(GL_TEXTURE_2D, 0, GL_BGR, GL_UNSIGNED_BYTE, data);

		data[imageSize - 1] = 0;
		data[imageSize - 2] = 0;


		fopen_s(&file, fileName, "wb");
		bmih.biSize = 40;
		bmih.biWidth = textureData->width;
		bmih.biHeight = textureData->height;
		bmih.biPlanes = 1;
		bmih.biBitCount = 24;
		bmih.biCompression = 0;
		bmih.biSizeImage = imageSize;
		bmih.biXPelsPerMeter = 45089;
		bmih.biYPelsPerMeter = 45089;
		bmih.biClrUsed = 0;
		bmih.biClrImportant = 0;
		fwrite(&bmfh, sizeof(bmfh), 1, file);
		fwrite(&bmih, sizeof(bmih), 1, file);
		fwrite(data, imageSize, 1, file);
		free(data);
		fclose(file);
	}

}