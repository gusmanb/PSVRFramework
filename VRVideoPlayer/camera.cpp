
#include "camera.h"

#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <glm\gtc\matrix_transform.hpp>

using namespace glm;

namespace Camera
{

	void setup(cameraData* cameraData)
	{
		auto view = glm::lookAt(cameraData->position, cameraData->target, glm::vec3(0, 1, 0));
		auto projection = glm::perspective(glm::radians(cameraData->FOV), (cameraData->viewWidth / cameraData->viewHeight), cameraData->farDistance, cameraData->nearDistance);
		cameraData->viewProjection = projection * view;
	}

	void setupCamera(float FOV, float vpWidth, float vpHeight, float nearDistance, float farDistance, glm::vec3 position, glm::vec3 target, cameraData* cameraData)
	{

		cameraData->FOV = FOV;
		cameraData->viewWidth = vpWidth;
		cameraData->viewHeight = vpHeight;
		cameraData->farDistance = farDistance;
		cameraData->nearDistance = nearDistance;
		cameraData->position = position;
		cameraData->target = target;
		setup(cameraData);
	}

	void changeFOV(cameraData* cameraData, float FOV)
	{
		cameraData->FOV = FOV;
		setup(cameraData);
	}
}