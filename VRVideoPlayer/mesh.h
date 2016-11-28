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
#include <glm\gtc\quaternion.hpp>

namespace Mesh
{

	typedef struct _mesh
	{
		GLuint posVBO;
		GLuint uvVBO;
		int vertexCount;
		//glm::vec3 rotation;
		glm::quat rotation;
		glm::vec3 translation;
		glm::mat4 world;
		bool needsUpdate;

	}meshData;

	void createMesh(const GLfloat* positions, const GLfloat* uvs, int vertexCount, meshData* outMesh);
	void destroyMesh(meshData* meshData);
	void createSphere(meshData* meshData, bool equirectangular);
	void createScreenPlane(meshData* meshData);
	void createPlaneBuffer(meshData* meshData, float width, float height, float widthSegments, float heightSegments);
	void loadMesh(meshData* meshData, const char* meshFile);
	//void rotateMesh(meshData* meshData, glm::vec3 rotation);
	//void orientateMesh(meshData* meshData, glm::vec3 rotation);
	void rotateMesh(meshData* meshData, glm::quat rotation);
	void orientateMesh(meshData* meshData, glm::quat rotation);
	void translateMesh(meshData* meshData, glm::vec3 translation);
	void placeMesh(meshData* meshData, glm::vec3 translation);
	void renderMesh(meshData* meshData, Camera::cameraData* cameraData, GLuint programId, bool leftEye, float ffactorx, float ffactory);
	void renderMesh(meshData* meshData, GLuint programId);
	void setBaseRotation(glm::vec3 rotation);
}