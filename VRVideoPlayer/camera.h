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