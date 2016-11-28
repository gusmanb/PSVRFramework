/*
* PSVRFramework - PlayStation VR PC framework
* Copyright (C) 2016 Agustín Giménez Bernad <geniwab@gmail.com>
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as
* published by the Free Software Foundation, either version 3 of the
* License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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