
#include "camera.h"

#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <glm\gtc\matrix_transform.hpp>

using namespace glm;

void setup(camera* camera)
{
	auto view = glm::lookAt(camera->position, camera->target, glm::vec3(0, 1, 0));
	auto projection = glm::perspective(glm::radians(camera->FOV), (camera->viewWidth / camera->viewHeight), camera->farDistance, camera->nearDistance);
	camera->viewProjection = projection * view;
}

void setupCamera(float FOV, float vpWidth, float vpHeight, float nearDistance, float farDistance, glm::vec3 position, glm::vec3 target, camera* camera)
{
	
	camera->FOV = FOV;
	camera->viewWidth = vpWidth;
	camera->viewHeight = vpHeight;
	camera->farDistance = farDistance;
	camera->nearDistance = nearDistance;
	camera->position = position;
	camera->target = target;
	setup(camera);
}

void changeFOV(camera* camera, float FOV)
{
	camera->FOV = FOV;
	setup(camera);
}