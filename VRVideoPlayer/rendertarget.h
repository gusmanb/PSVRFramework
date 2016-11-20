#pragma once
#include <GL/glew.h>
#include "texture.h"

namespace RenderTarget
{
	typedef struct _rendertarget
	{
		GLuint fboId;
		Texture::textureData renderTexture;
		int width;
		int height;

	}renderTargetData;

	void createTarget(int width, int height, renderTargetData* target);
	void enableTarget(renderTargetData* target, bool clear);
	void disableTarget();
	void destroyTarget(renderTargetData* target);
}