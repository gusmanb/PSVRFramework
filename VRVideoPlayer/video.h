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

#pragma once
#include "texture.h"
#include <vlc\libvlc.h>
#include <vlc\libvlc_media.h>
#include <vlc\libvlc_media_player.h>
#include <atomic>

namespace Video
{

	typedef struct _videoContext
	{
		libvlc_instance_t* instance;
		libvlc_media_t* media;
		libvlc_media_player_t* player;
		unsigned char *pixeldata;
		unsigned char currentFrame;
		int width;
		int height;
		int duration;
		int time;
		int volume;
		bool finished;
		std::atomic<bool> lock;

	}videoContext;

	void destroyVideo(videoContext* context);
	videoContext* initVideo(const char* fileName);
	void setTime(videoContext* context, long time);
	void setVolume(videoContext* context, int volume);
	void playPause(videoContext* context);

}