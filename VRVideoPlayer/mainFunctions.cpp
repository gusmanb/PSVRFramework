#include "mainFunctions.h"
#include "mesh.h"
#include "shader.h"
#include "camera.h"
#include "texture.h"
#include "video.h"

#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <string.h>
#include <iostream>
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>

using namespace std;

#pragma region Test data

static const GLfloat triangle_pos_data[] = {
	-1.0f, -1.0f, -2.0f,
	1.0f, -1.0f, -2.0f,
	0.0f,  1.0f, -2.0f,
};

static const GLfloat triangle_uv_data[] = {
	0, 0,
	1, 0,
	0.5f, 1
};


static char const * vertexShader = "\n"
"#version 330 core\n"
"layout(location = 0) in vec3 pos;\n"
"layout(location = 1) in vec2 uv;\n"
"uniform mat4 worldViewProjection;\n"
"uniform vec2 uvOffset;\n"
"uniform vec2 uvScale;\n"
"out vec2 finalUv;\n"
"void main(){\n"
"vec4 pos4 = vec4(pos, 1);\n"
"finalUv = (uv * uvScale) + uvOffset; \n"
"gl_Position = worldViewProjection * pos4; \n"
"}";

static char const * fragmentShader = "\n"
"#version 330 core\n"
"uniform sampler2D texture;\n"
"out vec4 color;\n"
"in vec2 finalUv;\n"
"void main() {\n"
"color = texture2D(texture, finalUv);\n"
"}";

#pragma endregion

using namespace glm;

GLFWwindow* window;
bool endLoop = false;

GLuint createVBA()
{
	GLuint vbaId;
	glGenVertexArrays(1, &vbaId);
	glBindVertexArray(vbaId);
	return vbaId;
}

double lastTime = 0;
int nbFrames = 0;

bool initOpenGL()
{
	if (!glfwInit())
		return false;
	

	glfwWindowHint(GLFW_SAMPLES, 0); // 4x antialiasing
	glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3); // We want OpenGL 3.3
	glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
	glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE); //We don't want the old OpenGL 

																   // Open a window and create its OpenGL context
	 // (In the accompanying source code, this variable is global)
	window = glfwCreateWindow(1920, 1080, "",NULL, NULL);//glfwGetPrimaryMonitor()

	glfwSwapInterval(1);

	if (window == NULL) 
	{
		glfwTerminate();
		return false;
	}
	glfwMakeContextCurrent(window); // Initialize GLEW
	glewExperimental = true; // Needed in core profile
	if (glewInit() != GLEW_OK) 
	{
		glfwTerminate();
		return false;
	}

	glViewport(0, 0, 1920, 1080);

	return true;

}

struct mesh eyeMesh;
glm::quat extRot;

void extRotate(float quat[])
{
	extRot = glm::quat(quat[1], quat[2], quat[3], quat[0]);
}

void runLoop(const char* videoFile)
{
	endLoop = false;
	glfwSetInputMode(window, GLFW_STICKY_KEYS, GL_TRUE);
	glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
	glfwMakeContextCurrent(window);
	glDisable(GL_CULL_FACE);

	auto vbId = createVBA();

	struct camera camera;

	setupCamera(90, 960, 1080, 0.0001f, 100, vec3(0, 0, 0), vec3(0, 0, -10), &camera);

	

	loadMesh(&eyeMesh, "eyeBall.obj");
	auto shaderId = createShader(vertexShader, fragmentShader);
	glUseProgram(shaderId);
	
	vec2 uvScale = vec2(-1, 1);
	vec2 uvOffset = vec2(0);

	setUniformVec2(shaderId, "uvScale", uvScale);
	setUniformVec2(shaderId, "uvOffset", uvOffset);
	
	struct texture videoTexture;
	createTexture(&videoTexture);

	auto vctx = initVideo(videoFile);//"file:///c:/temp/video.mp4");

	auto rotRight = glm::quat(glm::vec3(0, M_PI_2 + M_PI, 0));
	auto rotLeft = glm::quat(glm::vec3(0, M_PI_2, 0));
	auto ipdControl = glm::vec3(0, 0, 0);

	unsigned char currentFrame = 0;

	double xpos, ypos;
	double nxpos, nypos;
	double delta;
	glfwGetCursorPos(window, &xpos, &ypos);

	do {

		glfwGetCursorPos(window, &nxpos, &nypos);

		/*if (nxpos != xpos)
		{
			delta = (nxpos - xpos) / 1000;
			xpos = nxpos;
			rotRight.z += delta;
			rotLeft.z += delta;
		}

		if (nypos != ypos)
		{
			delta = (nypos - ypos) / 1000;
			ypos = nypos;
			rotRight.y += delta;
			rotLeft.y -= delta;
		}*/

		if (glfwGetKey(window, GLFW_KEY_LEFT) == GLFW_PRESS && 
			((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			ipdControl.z += 0.005f;
		
		if (glfwGetKey(window, GLFW_KEY_RIGHT) == GLFW_PRESS &&
			((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			ipdControl.z-= 0.005f;

		if (glfwGetKey(window, GLFW_KEY_UP) == GLFW_PRESS &&
			((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			changeFOV(&camera, camera.FOV + 0.1f);

		if (glfwGetKey(window, GLFW_KEY_DOWN) == GLFW_PRESS &&
			((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			changeFOV(&camera, camera.FOV - 0.1f);

		if (glfwGetKey(window, GLFW_KEY_LEFT) == GLFW_PRESS &&
			!((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			setTime(vctx, max(vctx->time - 5000, 0));

		if (glfwGetKey(window, GLFW_KEY_RIGHT) == GLFW_PRESS &&
			!((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			setTime(vctx, min(vctx->time + 5000, vctx->duration));

		if (glfwGetKey(window, GLFW_KEY_UP) == GLFW_PRESS &&
			!((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			setVolume(vctx, min(vctx->volume + 1, 100));

		if (glfwGetKey(window, GLFW_KEY_DOWN) == GLFW_PRESS &&
			!((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			setVolume(vctx, max(vctx->volume - 1, 0));

		if (glfwGetKey(window, GLFW_KEY_SPACE) == GLFW_PRESS &&
			!((glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) | glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL)) == GLFW_PRESS))
			playPause(vctx);

		if (vctx->currentFrame != currentFrame)
		{
			currentFrame = vctx->currentFrame;
			setData(&videoTexture, vctx->pixeldata, vctx->width, vctx->height);
		}

		glClearColor(0, 0, 0, 1);
		glClear(GL_COLOR_BUFFER_BIT);

		glViewport(0, 0, 960, 1080);
		orientateMesh(&eyeMesh, extRot * rotLeft);
		placeMesh(&eyeMesh, ipdControl);
		renderMesh(&eyeMesh, &camera, shaderId);

		glViewport(960, 0, 960, 1080);
		orientateMesh(&eyeMesh, extRot * rotRight);
		placeMesh(&eyeMesh, ipdControl);
		renderMesh(&eyeMesh, &camera, shaderId);
		
		auto va = libvlc_media_player_event_manager(vctx->player);
		
		glfwSwapBuffers(window);
		glfwPollEvents();

	} // Check if the ESC key was pressed or the window was closed
	while (!endLoop && glfwGetKey(window, GLFW_KEY_ESCAPE) != GLFW_PRESS &&
		glfwWindowShouldClose(window) == 0 && (vctx->duration == 0 || vctx->time < vctx->duration));

	destroyVideo(vctx);
	destroyMesh(&eyeMesh);
	destroyTexture(&videoTexture);
	destroyShader(shaderId);
	glfwDestroyWindow(window);
	window = NULL;

}

void endOpenGL()
{
	endLoop = true;
}