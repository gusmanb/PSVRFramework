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
#include "camera.h"
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>

struct mesh
{
	GLuint posVBO;
	GLuint uvVBO;
	int vertexCount;
	glm::quat rotation;
	glm::vec3 translation;
	glm::mat4 world;
	bool needsUpdate;
};

void createMesh(const GLfloat* positions, const GLfloat* uvs, int vertexCount, mesh* outMesh);
void destroyMesh(mesh* mesh);
void loadMesh(mesh* mesh, const char* meshFile);
void rotateMesh(mesh* mesh, glm::quat rotation);
void orientateMesh(mesh* mesh, glm::quat rotation);
void translateMesh(mesh* mesh, glm::vec3 translation);
void placeMesh(mesh* mesh, glm::vec3 translation);
void renderMesh(mesh* mesh, camera* camera, GLuint programId);