#pragma once

#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>

namespace Camera
{

	typedef struct _camera
	{
		glm::mat4 view;
		glm::mat4 projection;
		glm::mat4 viewProjection;
		glm::vec3 position;
		glm::vec3 target;
		float FOV;
		float nearDistance;
		float farDistance;
		float viewWidth;
		float viewHeight;

	}cameraData;

	void setupCamera(float FOV, float vpWidth, float vpHeight, float nearDistance, float farDistance, glm::vec3 position, glm::vec3 target, cameraData* cameraData);
	void changeFOV(cameraData* cameraData, float FOV);

}