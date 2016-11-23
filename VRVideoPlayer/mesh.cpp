#include "mesh.h"
#include "camera.h"
#include "shader.h"
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <glm\gtc\matrix_transform.hpp>
#include <glm/gtx/euler_angles.hpp>
#include <stdio.h>
#include <vector>
#include <math.h>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>

#define PI 3.14159265358979323846
#define _CRT_SECURE_NO_DEPRECATE
#define _CRT_SECURE_NO_WARNINGS
#pragma warning (disable : 4996)

namespace Mesh
{
	glm::vec3 baseRotation;

	void setBaseRotation(glm::vec3 rotation)
	{
		baseRotation = rotation;
	}

	//Creates the mesh buffers
	void createMesh(const GLfloat* positions, const GLfloat* uvs, int vertexCount, meshData* outMesh)
	{

		outMesh->vertexCount = vertexCount;

		glGenBuffers(1, &outMesh->posVBO);
		glBindBuffer(GL_ARRAY_BUFFER, outMesh->posVBO);
		glBufferData(GL_ARRAY_BUFFER, outMesh->vertexCount * 3 * sizeof(GLfloat), positions, GL_STATIC_DRAW);

		glGenBuffers(1, &outMesh->uvVBO);
		glBindBuffer(GL_ARRAY_BUFFER, outMesh->uvVBO);
		glBufferData(GL_ARRAY_BUFFER, outMesh->vertexCount * 2 * sizeof(GLfloat), uvs, GL_STATIC_DRAW);

		outMesh->world = glm::mat4(1.0f);
		outMesh->rotation = glm::vec3(0);
		outMesh->translation = glm::vec3(0);
	}

	void destroyMesh(meshData* meshData)
	{
		glDeleteBuffers(1, &meshData->posVBO);
		glDeleteBuffers(1, &meshData->uvVBO);
	}

	void createScreenPlane(meshData* meshData)
	{
		glm::vec3 planeVertexCoords[4];
		glm::vec2 planeTextureCoords[4];

		glm::vec3 finalPlaneVertexCoords[6];
		glm::vec2 finalPlaneTextureCoords[6];


		planeVertexCoords[0] = glm::vec3(-1, 1, 0);
		planeVertexCoords[1] = glm::vec3(1, 1, 0);
		planeVertexCoords[2] = glm::vec3(-1, -1, 0);
		planeVertexCoords[3] = glm::vec3(1, -1, 0);

		planeTextureCoords[0] = glm::vec2(0, 1);
		planeTextureCoords[1] = glm::vec2(1, 1);
		planeTextureCoords[2] = glm::vec2(0, 0);
		planeTextureCoords[3] = glm::vec2(1, 0);

		finalPlaneVertexCoords[0] = planeVertexCoords[1];
		finalPlaneVertexCoords[1] = planeVertexCoords[2];
		finalPlaneVertexCoords[2] = planeVertexCoords[3];

		finalPlaneVertexCoords[3] = planeVertexCoords[1];
		finalPlaneVertexCoords[4] = planeVertexCoords[0];
		finalPlaneVertexCoords[5] = planeVertexCoords[2];

		finalPlaneTextureCoords[0] = planeTextureCoords[1];
		finalPlaneTextureCoords[1] = planeTextureCoords[2];
		finalPlaneTextureCoords[2] = planeTextureCoords[3];

		finalPlaneTextureCoords[3] = planeTextureCoords[1];
		finalPlaneTextureCoords[4] = planeTextureCoords[0];
		finalPlaneTextureCoords[5] = planeTextureCoords[2];

		Mesh::createMesh(&finalPlaneVertexCoords[0][0], &finalPlaneTextureCoords[0][0], 6, meshData);
	}

	void createPlaneBuffer(meshData* meshData, float width, float height, float widthSegments, float heightSegments)
	{
		glm::vec3 planeVertexCoords[4];
		glm::vec2 planeTextureCoords[4];


		float width_half = width / 2;
		float height_half = height / 2;

		float gridX = widthSegments;
		float gridY = heightSegments;

		float gridX1 = gridX + 1;
		float gridY1 = gridY + 1;

		float segment_width = width / gridX;
		float segment_height = height / gridY;

		int vertexCount = gridX1 * gridY1;

		glm::vec3* vertices = new glm::vec3[vertexCount];
		glm::vec2* uvs = new glm::vec2[vertexCount];

		int offset = 0;

		for (int iy = 0; iy < gridY1; iy++) {

			float y = iy * segment_height - height_half;

			for (int ix = 0; ix < gridX1; ix++) {

				float x = ix * segment_width - width_half;

				vertices[offset] = glm::vec3(x, -y, 0);

				uvs[offset] = glm::vec2(ix / gridX, 1 - (iy / gridY));

				offset++;

			}

		}

		offset = 0;

		int indexCount = gridX * gridY * 6;

		int* indices = new int[indexCount];

		for (int iy = 0; iy < gridY; iy++) {

			for (int ix = 0; ix < gridX; ix++) {

				int a = ix + gridX1 * iy;
				int b = ix + gridX1 * (iy + 1);
				int c = (ix + 1) + gridX1 * (iy + 1);
				int d = (ix + 1) + gridX1 * iy;

				indices[offset] = a;
				indices[offset + 1] = b;
				indices[offset + 2] = d;

				indices[offset + 3] = b;
				indices[offset + 4] = c;
				indices[offset + 5] = d;

				offset += 6;

			}

		}

		std::vector<glm::vec3> final_vertices;
		std::vector<glm::vec2> final_uvs;

		for (unsigned int i = 0; i < indexCount; i++) {

			// Get the indices of its attributes
			unsigned int index = indices[i];

			// Get the attributes thanks to the index
			glm::vec3 vertex = vertices[index];
			glm::vec2 uv = uvs[index];

			// Put the attributes in buffers
			final_vertices.push_back(vertex);
			final_uvs.push_back(uv);

		}

		Mesh::createMesh(&final_vertices[0][0], &final_uvs[0][0], indexCount, meshData);
	}

	void createSphere(meshData* meshData, bool equirectangular)
	{
		const short numSlices = 50;
		const float radius = 10;

		const int numParallels = numSlices / 2;
		const int numVertices = (numParallels + 1) * (numSlices + 1);
		const int numIndices = numParallels * numSlices * 6;
		const float angleStep = (2.0f * (float)PI) / ((float)numSlices);

		short parallel;
		short slice;

		glm::vec3 sphereVetrexCoords[numVertices];
		glm::vec2 sphereTextureCoords[2 * numVertices];

		for (parallel = 0; parallel < numParallels + 1; parallel++)
		{
			for (slice = 0; slice < numSlices + 1; slice++)
			{
				int vertex = (parallel * (numSlices + 1) + slice);

				sphereVetrexCoords[vertex] = glm::vec3(
					-radius * (float)sin(angleStep * (double)parallel) * (float)sin(angleStep * (double)slice),
					-radius * (float)cos(angleStep * (double)parallel),
					radius * (float)sin(angleStep * (double)parallel) * (float)cos(angleStep * (double)slice)
				);

				glm::vec3 n = glm::normalize(sphereVetrexCoords[vertex]);

				if (equirectangular)
					sphereTextureCoords[vertex] = glm::vec2(1.0 - (atan2(n.x, n.z) / (2 * PI) + 0.5), 1.0f - ((float)parallel / (float)numParallels));
				else
					sphereTextureCoords[vertex] = glm::vec2(1.0 - (atan2(n.x, n.z) / (2 * PI) + 0.5), 1.0 - (n.y * 0.5 + 0.5));

			}
		}

		std::vector<glm::vec3> final_vertices;
		std::vector<glm::vec2> final_uvs;

		// Generate the indices
		int thisIndex = 0;

		for (parallel = 0; parallel < numParallels; parallel++)
		{
			for (slice = 0; slice < numSlices; slice++)
			{

				thisIndex = (short)(parallel * (numSlices + 1) + slice);
				final_vertices.push_back(sphereVetrexCoords[thisIndex]);
				final_uvs.push_back(sphereTextureCoords[thisIndex]);

				thisIndex = (short)((parallel + 1) * (numSlices + 1) + slice);
				final_vertices.push_back(sphereVetrexCoords[thisIndex]);
				final_uvs.push_back(sphereTextureCoords[thisIndex]);

				thisIndex = (short)((parallel + 1) * (numSlices + 1) + (slice + 1));
				final_vertices.push_back(sphereVetrexCoords[thisIndex]);
				final_uvs.push_back(sphereTextureCoords[thisIndex]);

				thisIndex = (short)(parallel * (numSlices + 1) + slice);
				final_vertices.push_back(sphereVetrexCoords[thisIndex]);
				final_uvs.push_back(sphereTextureCoords[thisIndex]);

				thisIndex = (short)((parallel + 1) * (numSlices + 1) + (slice + 1));
				final_vertices.push_back(sphereVetrexCoords[thisIndex]);
				final_uvs.push_back(sphereTextureCoords[thisIndex]);

				thisIndex = (short)(parallel * (numSlices + 1) + (slice + 1));
				final_vertices.push_back(sphereVetrexCoords[thisIndex]);
				final_uvs.push_back(sphereTextureCoords[thisIndex]);

			}
		}

		Mesh::createMesh(&final_vertices[0][0], &final_uvs[0][0], final_uvs.size(), meshData);
	}

	void loadMesh(meshData* meshData, const char* meshFile)
	{
		std::vector<unsigned int> vertexIndices, uvIndices, normalIndices;
		std::vector<glm::vec3> temp_vertices;
		std::vector<glm::vec2> temp_uvs;
		std::vector<glm::vec3> temp_normals;

		std::vector<glm::vec3> final_vertices;
		std::vector<glm::vec2> final_uvs;
		std::vector<glm::vec3> final_normals;


		FILE * file;
		fopen_s(&file, meshFile, "r");

		if (file == NULL)
			return;

		while (1) {

			char lineHeader[128];
			// read the first word of the line
			int res = fscanf(file, "%s", lineHeader);
			if (res == EOF)
			{
				fclose(file);
				break; // EOF = End Of File. Quit the loop.
			}
			// else : parse lineHeader

			if (strcmp(lineHeader, "v") == 0) {
				glm::vec3 vertex;
				fscanf(file, "%f %f %f\n", &vertex.x, &vertex.y, &vertex.z);
				temp_vertices.push_back(vertex);
			}
			else if (strcmp(lineHeader, "vt") == 0) {
				glm::vec2 uv;
				fscanf(file, "%f %f\n", &uv.x, &uv.y);
				uv.y = -uv.y; // Invert V coordinate since we will only use DDS texture, which are inverted. Remove if you want to use TGA or BMP loaders.
				temp_uvs.push_back(uv);
			}
			else if (strcmp(lineHeader, "vn") == 0) {
				glm::vec3 normal;
				fscanf(file, "%f %f %f\n", &normal.x, &normal.y, &normal.z);
				temp_normals.push_back(normal);
			}
			else if (strcmp(lineHeader, "f") == 0) {
				std::string vertex1, vertex2, vertex3;
				unsigned int vertexIndex[3], uvIndex[3], normalIndex[3];
				int matches = fscanf_s(file, "%d/%d/%d %d/%d/%d %d/%d/%d\n", &vertexIndex[0], &uvIndex[0], &normalIndex[0], &vertexIndex[1], &uvIndex[1], &normalIndex[1], &vertexIndex[2], &uvIndex[2], &normalIndex[2]);
				if (matches != 9)
					return;

				vertexIndices.push_back(vertexIndex[0]);
				vertexIndices.push_back(vertexIndex[1]);
				vertexIndices.push_back(vertexIndex[2]);
				uvIndices.push_back(uvIndex[0]);
				uvIndices.push_back(uvIndex[1]);
				uvIndices.push_back(uvIndex[2]);
				normalIndices.push_back(normalIndex[0]);
				normalIndices.push_back(normalIndex[1]);
				normalIndices.push_back(normalIndex[2]);
			}
			else {
				// Probably a comment, eat up the rest of the line
				char stupidBuffer[1000];
				fgets(stupidBuffer, 1000, file);
			}

		}

		// For each vertex of each triangle
		for (unsigned int i = 0; i < vertexIndices.size(); i++) {

			// Get the indices of its attributes
			unsigned int vertexIndex = vertexIndices[i];
			unsigned int uvIndex = uvIndices[i];
			unsigned int normalIndex = normalIndices[i];

			// Get the attributes thanks to the index
			glm::vec3 vertex = temp_vertices[vertexIndex - 1];
			glm::vec2 uv = temp_uvs[uvIndex - 1];
			glm::vec3 normal = temp_normals[normalIndex - 1];

			// Put the attributes in buffers
			final_vertices.push_back(vertex);
			final_uvs.push_back(uv);
			final_normals.push_back(normal);

		}

		Mesh::createMesh(&final_vertices[0][0], &final_uvs[0][0], vertexIndices.size(), meshData);
	}

	//void rotateMesh(meshData* meshData, glm::vec3 rotation)
	//{
	//	meshData->rotation += rotation;
	//	meshData->needsUpdate = true;
	//}

	//void orientateMesh(meshData* meshData, glm::vec3 rotation)
	//{
	//	meshData->rotation = rotation;
	//	meshData->needsUpdate = true;
	//}

	void rotateMesh(meshData* meshData, glm::quat rotation)
	{
		meshData->rotation *= rotation;
		meshData->needsUpdate = true;
	}

	void orientateMesh(meshData* meshData, glm::quat rotation)
	{
		meshData->rotation = rotation;
		meshData->needsUpdate = true;
	}

	void translateMesh(meshData* meshData, glm::vec3 translation)
	{
		meshData->translation += translation;
		meshData->needsUpdate = true;
	}

	void placeMesh(meshData* meshData, glm::vec3 translation)
	{
		meshData->translation = translation;
		meshData->needsUpdate = true;
	}

	void renderMesh(meshData* meshData, Camera::cameraData* cameraData, GLuint programId, bool leftEye, float ffactorx, float ffactory)
	{
		glUseProgram(programId);

		auto err = glGetError();

		//Pos buffer
		glEnableVertexAttribArray(0);
		glBindBuffer(GL_ARRAY_BUFFER, meshData->posVBO);
		glVertexAttribPointer(
			0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
			3,                  // size
			GL_FLOAT,           // type
			GL_FALSE,           // normalized?
			0,                  // stride
			(void*)0            // array buffer offset
		);

		////UV buffer
		glEnableVertexAttribArray(1);
		glBindBuffer(GL_ARRAY_BUFFER, meshData->uvVBO);
		glVertexAttribPointer(
			1,                  // attribute 1. No particular reason for 0, but must match the layout in the shader.
			2,                  // size
			GL_FLOAT,           // type
			GL_FALSE,           // normalized?
			0,                  // stride
			(void*)0            // array buffer offset
		);

		if (meshData->needsUpdate)
		{
			meshData->needsUpdate = false;

			auto tmp = glm::eulerAngles(meshData->rotation);
			//auto tmp2 = glm::eulerAngleZ(tmp.z) * glm::eulerAngleX(-tmp.y) * glm::eulerAngleY(tmp.x);// */  glm::orientate4(glm::vec3(, tmp.z, tmp.x));
			// */  glm::orientate4(glm::vec3(, tmp.z, tmp.x));

		
			//float factorx = 0;
			//float factory = 0;
			//if (leftEye)
			//{
			//	factorx = (1 - ((cos(tmp.z / PI) + 1) / 2)) * PI * ffactorx;
			//	factory = (1 - ((cos(tmp.z / PI) + 1) / 2)) * PI * ffactory;
			//}
			//else
			//{
			//	factorx = -(1 - ((cos(tmp.z / PI) + 1) / 2)) * PI * ffactorx;
			//	factory = -(1 - ((cos(tmp.z / PI) + 1) / 2)) * PI * ffactory;
			//}
			//auto tmp2 = /*glm::eulerAngleZ(tmp.z) */ glm::eulerAngleX(-tmp.y) * glm::eulerAngleY(tmp.x);
			//auto tmp2 = glm::toMat4(meshData->rotation);
			//auto tmp3 = glm::quat() * glm::inverse(glm::toQuat(glm::eulerAngleZ(tmp.z)));
			//tmp2 = glm::toMat4(meshData->rotation * tmp3);

			//auto tmp2 = glm::eulerAngleX(tmp.x) * glm::eulerAngleY(tmp.y);
			auto tmp2 = glm::eulerAngleX(-tmp.y  + baseRotation. x) * glm::eulerAngleY(tmp.x + baseRotation.y);

			meshData->world = glm::translate(tmp2, meshData->translation);

			//auto tmp2 = glm::eulerAngleZ(meshData->rotation.z) * glm::eulerAngleX(-meshData->rotation.y) * glm::eulerAngleY(meshData->rotation.x);// */  glm::orientate4(glm::vec3(, tmp.z, tmp.x));

			//meshData->world = glm::translate(tmp2, meshData->translation);


		}

		auto wvp = cameraData->viewProjection * meshData->world;
		Shader::setUniformMatrix(programId, "worldViewProjection", wvp);

		glDrawArrays(GL_TRIANGLES, 0, meshData->vertexCount); // Starting from vertex 0; 3 vertices total -> 1 triangle

		glDisableVertexAttribArray(0);
		glDisableVertexAttribArray(1);

	}

	void renderMesh(meshData* meshData, GLuint programId)
	{
		auto err = glGetError();

		//Pos buffer
		glEnableVertexAttribArray(0);
		glBindBuffer(GL_ARRAY_BUFFER, meshData->posVBO);
		glVertexAttribPointer(
			0,                  // attribute 0. No particular reason for 0, but must match the layout in the shader.
			3,                  // size
			GL_FLOAT,           // type
			GL_FALSE,           // normalized?
			0,                  // stride
			(void*)0            // array buffer offset
		);

		////UV buffer
		glEnableVertexAttribArray(1);
		glBindBuffer(GL_ARRAY_BUFFER, meshData->uvVBO);
		glVertexAttribPointer(
			1,                  // attribute 1. No particular reason for 0, but must match the layout in the shader.
			2,                  // size
			GL_FLOAT,           // type
			GL_FALSE,           // normalized?
			0,                  // stride
			(void*)0            // array buffer offset
		);
		
		glDrawArrays(GL_TRIANGLES, 0, meshData->vertexCount); // Starting from vertex 0; 3 vertices total -> 1 triangle

		glDisableVertexAttribArray(0);
		glDisableVertexAttribArray(1);

	}
}