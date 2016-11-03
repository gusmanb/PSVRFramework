#include "mesh.h"
#include "camera.h"
#include "shader.h"
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <glm\gtc\matrix_transform.hpp>
#include <glm/gtx/euler_angles.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>

#include <stdio.h>
#include <vector>

#define _CRT_SECURE_NO_DEPRECATE
#define _CRT_SECURE_NO_WARNINGS
#pragma warning (disable : 4996)

//Creates the mesh buffers
void createMesh(const GLfloat* positions, const GLfloat* uvs, int vertexCount, mesh* outMesh)
{

	outMesh->vertexCount = vertexCount;

	glGenBuffers(1, &outMesh->posVBO);
	glBindBuffer(GL_ARRAY_BUFFER, outMesh->posVBO);
	glBufferData(GL_ARRAY_BUFFER, outMesh->vertexCount * 3 * sizeof(GLfloat), positions, GL_STATIC_DRAW);

	glGenBuffers(1, &outMesh->uvVBO);
	glBindBuffer(GL_ARRAY_BUFFER, outMesh->uvVBO);
	glBufferData(GL_ARRAY_BUFFER, outMesh->vertexCount * 2 * sizeof(GLfloat), uvs, GL_STATIC_DRAW);
	
	outMesh->world = glm::mat4(1.0f);
	outMesh->rotation = glm::quat(1.0, 0, 0, 0);
	outMesh->translation = glm::vec3(0);
}

void destroyMesh(mesh* mesh)
{
	glDeleteBuffers(1, &mesh->posVBO);
	glDeleteBuffers(1, &mesh->uvVBO);
}

void loadMesh(mesh* mesh, const char* meshFile)
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
	for (unsigned int i = 0; i<vertexIndices.size(); i++) {

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

	createMesh(&final_vertices[0][0], &final_uvs[0][0], vertexIndices.size(), mesh);
}

void rotateMesh(mesh* mesh, glm::quat rotation)
{
	mesh->rotation *= rotation;
	mesh->needsUpdate = true;
}

void orientateMesh(mesh* mesh, glm::quat rotation)
{
	mesh->rotation = rotation;
	mesh->needsUpdate = true;
}

void translateMesh(mesh* mesh, glm::vec3 translation)
{
	mesh->translation += translation;
	mesh->needsUpdate = true;
}

void placeMesh(mesh* mesh, glm::vec3 translation)
{
	mesh->translation = translation;
	mesh->needsUpdate = true;
}

void renderMesh(mesh* mesh, camera* camera,  GLuint programId)
{
	glUseProgram(programId);

	auto err = glGetError();

	//Pos buffer
	glEnableVertexAttribArray(0);
	glBindBuffer(GL_ARRAY_BUFFER, mesh->posVBO);
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
	glBindBuffer(GL_ARRAY_BUFFER, mesh->uvVBO);
	glVertexAttribPointer(
		1,                  // attribute 1. No particular reason for 0, but must match the layout in the shader.
		2,                  // size
		GL_FLOAT,           // type
		GL_FALSE,           // normalized?
		0,                  // stride
		(void*)0            // array buffer offset
	);

	if (mesh->needsUpdate)
	{
		mesh->needsUpdate = false;
		mesh->world = glm::translate(glm::toMat4(mesh->rotation), mesh->translation);
	}

	auto wvp = camera->viewProjection * mesh->world;
	setUniformMatrix(programId, "worldViewProjection", wvp);

	glDrawArrays(GL_TRIANGLES, 0, mesh->vertexCount); // Starting from vertex 0; 3 vertices total -> 1 triangle

	glDisableVertexAttribArray(0);
	glDisableVertexAttribArray(1);

}