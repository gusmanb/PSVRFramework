#include "rendertarget.h"
#include "texture.h"

namespace RenderTarget
{
	void createTarget(int width, int height, renderTargetData* target)
	{
		target->height = height;
		target->width = width;

		Texture::createTexture(&target->renderTexture);
		Texture::setData(&target->renderTexture, NULL, width, height);
		glGenFramebuffers(1, &target->fboId);
		glBindFramebuffer(GL_FRAMEBUFFER, target->fboId);
		glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, target->renderTexture.id, 0);
		glBindFramebuffer(GL_FRAMEBUFFER, NULL);
	}

	void enableTarget(renderTargetData* target, bool clear)
	{
		glBindFramebuffer(GL_FRAMEBUFFER, target->fboId);

		if (clear)
			glClear(GL_COLOR_BUFFER_BIT);

		glViewport(0, 0, target->width, target->height);
	}

	void disableTarget()
	{
		glBindFramebuffer(GL_FRAMEBUFFER, NULL);
	}

	void destroyTarget(renderTargetData* target)
	{
		glDeleteFramebuffers(1, &target->fboId);
		Texture::destroyTexture(&target->renderTexture);
	}
}