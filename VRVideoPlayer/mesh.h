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