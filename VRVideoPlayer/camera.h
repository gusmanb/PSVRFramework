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

struct camera
{
	glm::mat4 viewProjection;
	glm::vec3 position;
	glm::vec3 target;
	float FOV;
	float nearDistance;
	float farDistance;
	float viewWidth;
	float viewHeight;
};

void setupCamera(float FOV, float vpWidth, float vpHeight, float nearDistance, float farDistance, glm::vec3 position, glm::vec3 target, camera* camera);
void changeFOV(camera* camera, float FOV);