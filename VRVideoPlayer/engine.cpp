#include "engine.h"
#include "mesh.h"
#include "shader.h"
#include "camera.h"
#include "texture.h"
#include "video.h"
#include "keybmanager.h"
#include "rendertarget.h"
#include "vrdevice.h"

#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <string.h>
#include <iostream>
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <glm/gtc/quaternion.hpp>
#include <glm/gtx/quaternion.hpp>
#include <glm/gtx/euler_angles.hpp>
#include <stdbool.h>
#include <string>
#include<math.h>
#include <Windows.h>
#define PI 3.14159265358979323846
using namespace std;

namespace Engine
{

	using namespace glm;

	GLFWwindow* window;
	bool endLoop = false;
	double lastTime = 0;
	int nbFrames = 0;
	videoSettings sets;
	Camera::cameraData mainCamera;
	Mesh::meshData eyeMesh;
	Mesh::meshData effectMesh;
	Texture::textureData videoTexture;
	Video::videoContext* vctx;
	//glm::vec3 rot;
	glm::quat rot;
	glm::vec3 ipdLeftControl;
	glm::vec3 ipdRightControl;
	float maxXRot = 0;
	float maxYRot = 0;
	float minXRot = 0;
	float minYRot = 0;
	VRDevice::vrdevicedata psvrDevice;
	VRDevice::vrlensprops lensProps;

	monitor* getScreens(int* count)
	{
		if (!glfwInit())
			return false;

		GLFWmonitor** monitors = glfwGetMonitors(count);

		monitor* monitorData = (monitor*)malloc(sizeof(monitor) * *count);

		for (int buc = 0; buc < *count; buc++)
		{
			monitorData[buc].index = buc;
			const char* name = glfwGetMonitorName(monitors[buc]);
			int len = strlen(name) + 1;
			monitorData[buc].name = (char *)malloc(len);
			memset(monitorData[buc].name, 0, len);
			strcpy_s(monitorData[buc].name, len, name);

		}

		glfwTerminate();

		return monitorData;
	}

	bool init(videoSettings settings)
	{
		if (!glfwInit())
			return false;

		memcpy(&lensProps, &VRDevice::PSVRLensProps, sizeof(VRDevice::vrlensprops));

		VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

		sets = settings;

		glfwWindowHint(GLFW_SAMPLES, 0); // no antialiasing
		glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3); // We want OpenGL 3.3
		glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
		glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE); //We don't want the old OpenGL 

		int cnt;
		auto mons = glfwGetMonitors(&cnt);

		if (settings.monitorIndex >= cnt)
			throw std::invalid_argument("Invalid monitor index");

		auto mon = mons[settings.monitorIndex];

		window = glfwCreateWindow(1920, 1080, "", mon, NULL);

		glfwSwapInterval(0);

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

		float angle = glm::radians((settings.hfov - 100) / 2);
		maxXRot = settings.initialRotation.y + angle;
		minXRot = settings.initialRotation.y - angle;
		angle = glm::radians(settings.vfov - 68) / 2;
		maxYRot = settings.initialRotation.x + angle;
		minYRot = settings.initialRotation.x - angle;
		return true;

	}

	GLuint createVBA()
	{
		GLuint vbaId;
		glGenVertexArrays(1, &vbaId);
		glBindVertexArray(vbaId);
		return vbaId;
	}

	bool dump;
	bool showCenter = false;
	float vRelief = 0.06f;
	float hRelief = 0.07f;
	float ffactorx = 0;
	float ffactory = 0;
	void keybHandler(int Key, bool Control, bool Alt, bool Shift)
	{
		switch (Key)
		{
		case GLFW_KEY_LEFT:

			if (Control)
			{
				ipdLeftControl.z += 0.001f;
				ipdRightControl.z -= 0.001f;
			}
			else if (Shift)
			{
				Video::setTime(vctx, max(vctx->time - 30000, 0));
			}
			else
			{
				Video::setTime(vctx, max(vctx->time - 5000, 0));
			}

			break;
		case GLFW_KEY_RIGHT:

			if (Control)
			{
				ipdLeftControl.z -= 0.001f;
				ipdRightControl.z += 0.001f;
			}
			else if (Shift)
			{
				Video::setTime(vctx, min(vctx->time + 30000, vctx->duration));
			}
			else
			{
				Video::setTime(vctx, min(vctx->time + 5000, vctx->duration));
			}

			break;
		case GLFW_KEY_UP:

			if (Control)
			{
				Camera::changeFOV(&mainCamera, mainCamera.FOV + 0.5f);
			}
			else
			{
				Video::setVolume(vctx, min(vctx->volume + 1, 100));
			}

			break;
		case GLFW_KEY_DOWN:

			if (Control)
			{
				Camera::changeFOV(&mainCamera, mainCamera.FOV - 0.5f);
			}
			else
			{
				Video::setVolume(vctx, max(vctx->volume - 1, 0));
			}

			break;

		case GLFW_KEY_SPACE:

			Video::playPause(vctx);
			dump = true;
			break;

		case GLFW_KEY_Q:

			lensProps.interLensDistance += 0.000005;
			VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

			break;

		case GLFW_KEY_W:

			lensProps.interLensDistance -= 0.000005;
			VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

			break;

		case GLFW_KEY_A:

			lensProps.baselineDistance += 0.000005;
			VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

			break;

		case GLFW_KEY_S:

			lensProps.baselineDistance -= 0.000005;
			VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

			break;

		case GLFW_KEY_Z:

			//lensProps.fov += 1;
			//VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);
			ffactorx -= 0.05f;
			break;

		case GLFW_KEY_X:

			//lensProps.fov -= 1;
			//VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);
			ffactorx += 0.05f;
			break;

		case GLFW_KEY_E:

			lensProps.screenLensDistance += 0.0001;
			VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

			break;

		case GLFW_KEY_R:

			lensProps.screenLensDistance -= 0.0001;
			VRDevice::initializeDevice(&psvrDevice, &VRDevice::PSVRScreenProps, &lensProps);

			break;

		case GLFW_KEY_D:

			hRelief -= 0.01;
			break;

		case GLFW_KEY_F:

			hRelief += 0.01;
			break;

		case GLFW_KEY_C:

			//vRelief -= 0.01;
			ffactory -= 0.05;
			break;

		case GLFW_KEY_V:

			//vRelief += 0.01;
			ffactory += 0.05;
			break;

		case GLFW_KEY_1:

			showCenter = !showCenter;
			break;

		}
	}

	glm::vec3 baseRot;
	glm::quat deviceRot = toQuat(orientate3(glm::vec3(0, PI / 2.0f, 0)));
	void updateRotation(float x, float y, float z, float w)
	{
		glm::quat q = glm::quat();
		q.x = x;
		q.y = y;
		q.z = z;
		q.w = w;
		rot = q;// *deviceRot;
	}

	
	/*void updateRotation(float x, float y, float z)
	{
		rot = glm::vec3(x, y, z);
	}*/

	void run(const char* videoFile)
	{
		endLoop = false;
		glfwSetInputMode(window, GLFW_STICKY_KEYS, GL_TRUE);
		glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
		glfwMakeContextCurrent(window);
		glDisable(GL_CULL_FACE);

		auto vbId = createVBA();

		setupCamera(90, 960, 1080, 0.0001f, 100, vec3(0, 0, 0), vec3(0, 0, 10), &mainCamera);

		Mesh::setBaseRotation(baseRot);

		Mesh::createSphere(&eyeMesh, sets.equilateral);
		Mesh::createPlaneBuffer(&effectMesh, 2, 2, 1, 1);

		auto shaderId = Shader::createShader(vertexRenderShader, fragmentRenderShader);
		auto effectId = Shader::createShader(vertexBarrelShader, fragmentBarrelChromaticShader);

		RenderTarget::renderTargetData renderTarget;
		RenderTarget::createTarget(1920, 1080, &renderTarget);

		Texture::createTexture(&videoTexture);

		Texture::textureData grid;

		Texture::loadBmpTexture(&grid, "grid.bmp");

		vctx = Video::initVideo(videoFile);
		baseRot = sets.initialRotation;
		ipdLeftControl = glm::vec3(0, 0, 0);
		ipdRightControl = glm::vec3(0, 0, 0);

		setVolume(vctx, 0);

		KeybManager::registerKey(GLFW_KEY_LEFT, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_LEFT, true, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_LEFT, false, false, true, &keybHandler);

		KeybManager::registerKey(GLFW_KEY_RIGHT, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_RIGHT, true, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_RIGHT, false, false, true, &keybHandler);

		KeybManager::registerKey(GLFW_KEY_UP, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_UP, true, false, false, &keybHandler);

		KeybManager::registerKey(GLFW_KEY_DOWN, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_DOWN, true, false, false, &keybHandler);

		KeybManager::registerKey(GLFW_KEY_SPACE, false, false, false, &keybHandler);


		KeybManager::registerKey(GLFW_KEY_Q, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_W, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_A, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_S, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_Z, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_X, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_E, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_R, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_D, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_F, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_C, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_V, false, false, false, &keybHandler);
		KeybManager::registerKey(GLFW_KEY_1, false, false, false, &keybHandler);

		unsigned char currentFrame = 0;

		double xpos, ypos;
		double nxpos, nypos;
		double delta;

		glfwGetCursorPos(window, &xpos, &ypos);

		bool exchangeExpected = false;

		do {

			KeybManager::updateKeyboard(window);

			if (vctx->currentFrame != currentFrame)
			{
				//while (!vctx->lock.compare_exchange_strong(exchangeExpected, true))
				//	Sleep(0);

				currentFrame = vctx->currentFrame;
				Texture::setData(&videoTexture, vctx->pixeldata, vctx->width, vctx->height);
				//vctx->lock.store(false);
			}

			glClearColor(0, 0, 0, 1);
			glClear(GL_COLOR_BUFFER_BIT);

			glUseProgram(shaderId);

			Texture::enableTexture(&videoTexture, 0);
			//Texture::enableTexture(&grid, 0);


			RenderTarget::enableTarget(&renderTarget, true);

			glViewport(0, 0, 960, 1080);
			Mesh::orientateMesh(&eyeMesh, rot);
			Mesh::placeMesh(&eyeMesh, ipdLeftControl);
			Shader::setUniformVec2(shaderId, "uvScale", sets.leftEye.scale);
			Shader::setUniformVec2(shaderId, "uvOffset", sets.leftEye.offset);
			Mesh::renderMesh(&eyeMesh, &mainCamera, shaderId, true, ffactorx, ffactory);

			glViewport(960, 0, 960, 1080);
			Mesh::orientateMesh(&eyeMesh, rot);
			Mesh::placeMesh(&eyeMesh, ipdRightControl);
			Shader::setUniformVec2(shaderId, "uvScale", sets.rightEye.scale);
			Shader::setUniformVec2(shaderId, "uvOffset", sets.rightEye.offset);
			Mesh::renderMesh(&eyeMesh, &mainCamera, shaderId, false, ffactorx, ffactory);
			
			RenderTarget::disableTarget();

			if (dump)
			{
				dump = false;
				Texture::saveBmpTexture(&renderTarget.renderTexture, "dump.bmp");
			}
	
			glViewport(0, 0, 1920, 1080);
			
			glUseProgram(effectId);

			if(showCenter)
				Texture::enableTexture(&grid, 0);
			else
				Texture::enableTexture(&renderTarget.renderTexture, 0);

			Shader::setUniformVec2(effectId, "distortion", psvrDevice.lensProperties.distortionCoeffs);
			Shader::setUniformVec4(effectId, "backgroundColor", glm::vec4(0,0,0,1));
			Shader::setUniformVec4(effectId, "projectionLeft", psvrDevice.projectionLeft);
			Shader::setUniformVec4(effectId, "unprojectionLeft", psvrDevice.unprojectionLeft);
			Shader::setUniformInt(effectId, "showCenter", showCenter);
			Shader::setUniformFloat(effectId, "vEyeRelief", vRelief);
			Shader::setUniformFloat(effectId, "hEyeRelief", hRelief);

			Mesh::renderMesh(&effectMesh, effectId);

			glfwSwapBuffers(window);
			glfwPollEvents();

		} while (!endLoop && !vctx->finished && glfwGetKey(window, GLFW_KEY_ESCAPE) != GLFW_PRESS &&
			glfwWindowShouldClose(window) == 0 && (vctx->duration == 0 || vctx->time < vctx->duration));

		Video::destroyVideo(vctx);
		Mesh::destroyMesh(&eyeMesh);
		Texture::destroyTexture(&videoTexture);
		Shader::destroyShader(shaderId);
		glfwDestroyWindow(window);
		window = NULL;
		KeybManager::clearKeys();
		glfwTerminate();
	}

	void stop()
	{
		endLoop = true;
	}
}