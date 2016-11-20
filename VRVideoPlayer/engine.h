#pragma once
#include <glm\glm.hpp>

namespace Engine
{

#define M_E 2.71828182845904523536
#define M_LOG2E 1.44269504088896340736
#define M_LOG10E 0.434294481903251827651
#define M_LN2 0.693147180559945309417
#define M_LN10 2.30258509299404568402
#define M_PI 3.14159265358979323846
#define M_PI_2 1.57079632679489661923
#define M_PI_4 0.785398163397448309616
#define M_1_PI 0.318309886183790671538
#define M_2_PI 0.636619772367581343076
#define M_1_SQRTPI 0.564189583547756286948
#define M_2_SQRTPI 1.12837916709551257390
#define M_SQRT2 1.41421356237309504880
#define M_SQRT_2 0.707106781186547524401


#pragma region Shaders

	static char const * vertexRenderShader = "\n"
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

	static char const * fragmentRenderShader = "\n"
		"#version 330 core\n"
		"uniform sampler2D texture;\n"
		"out vec4 color;\n"
		"in vec2 finalUv;\n"
		"void main() {\n"
		"color = texture2D(texture, finalUv);\n"
		"}";

	static char const * vertexPassThruShader = "\n"
		"#version 330 core\n"
		"layout(location = 0) in vec3 pos;\n"
		"layout(location = 1) in vec2 uv;\n"
		"out vec2 finalUv;\n"
		"void main(){\n"
		"vec4 pos4 = vec4(pos, 1);\n"
		"finalUv = uv; \n"
		"gl_Position = pos4; \n"
		"}";

	static char const * fragmentPassThruShader = "\n"
		"#version 330 core\n"
		"uniform sampler2D texture;\n"
		"out vec4 color;\n"
		"in vec2 finalUv;\n"
		"void main() {\n"
		"color = texture2D(texture, finalUv);\n"
		"}";

	static char const * vertexBarrelShader = "\n"
		"#version 330 core\n"
		"layout(location = 0) in vec3 pos;\n"
		"layout(location = 1) in vec2 uv;\n"
		"out vec2 finalUv;\n"
		"void main(){\n"
		"vec4 pos4 = vec4(pos, 1);\n"
		"finalUv = uv; \n"
		"gl_Position = pos4; \n"
		"}";

	static char const * fragmentBarrelShader = "\n"
		"#version 330 core\n"
		"uniform sampler2D texture;\n"
		"uniform vec2 distortion;\n"
		"uniform vec4 backgroundColor;\n"
		"uniform vec4 projectionLeft;\n"
		"uniform vec4 unprojectionLeft;\n"
		"uniform int showCenter;\n"
		"out vec4 color;\n"
		"in vec2 finalUv;\n"
		"float poly(float val){\n"
		"return (showCenter == 1 && val < 0.00010) ? \n"
		"10000.0 : 1.0 + (distortion.x + distortion.y * val) * val;\n"
		"}\n"
		"vec2 barrel(vec2 v, vec4 projection, vec4 unprojection){\n"
		"vec2 w = (v + unprojection.zw) / unprojection.xy;\n"
		"return projection.xy * (poly(dot(w, w)) * w) - projection.zw;\n"
		"}\n"
		"\n"
		"void main(){\n"

		"vec4 projectionRight = (projectionLeft + vec4(0.0, 0.0, 1.0, 0.0)) * vec4(1.0, 1.0, -1.0, 1.0);\n"
		"vec4 unprojectionRight = (unprojectionLeft + vec4(0.0, 0.0, 1.0, 0.0)) * vec4(1.0, 1.0, -1.0, 1.0);\n"
		"vec2 a = (finalUv.x < 0.5) ? \n"
		"barrel(vec2(finalUv.x / 0.5, finalUv.y), projectionLeft, unprojectionLeft) : \n"
		"barrel(vec2((finalUv.x - 0.5) / 0.5, finalUv.y), projectionRight, unprojectionRight);\n"

		"a.y = a.y / 2.0f;\n"

		"if (a.x < 0.0 || a.x > 1.0 || a.y < 0.0 || a.y > 1.0)\n"
		"color = backgroundColor;\n"
		"else\n"
		"color = texture2D(texture, vec2(a.x * 0.5 + (finalUv.x < 0.5 ? 0.0 : 0.5), a.y));\n"
		"}";

#pragma endregion


	typedef struct _eyeConfiguration
	{
		glm::vec2 scale;
		glm::vec2 offset;
	}eyeConfiguration;

	typedef struct _monitor
	{
		int index;
		char* name;
	}monitor;

	typedef struct _videoSettings
	{
		eyeConfiguration leftEye;
		eyeConfiguration rightEye;
		glm::vec3 initialRotation;
		int monitorIndex;
		float hfov;
		float vfov;
		bool equilateral;

	}videoSettings;

	monitor* getScreens(int* count);
	bool init(videoSettings settings);
	void run(const char* videoFile);
	void stop();

}