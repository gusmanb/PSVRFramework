/*
* PSVRFramework - PlayStation VR PC framework
* Copyright (C) 2016 Agustín Giménez Bernad <geniwab@gmail.com>
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as
* published by the Free Software Foundation, either version 3 of the
* License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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