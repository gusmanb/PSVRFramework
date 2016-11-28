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

#include "video.h"
#include <string.h>
#include <vlc\libvlc.h>
#include <vlc\libvlc_media.h>
#include <vlc\libvlc_media_player.h>
#include "texture.h"

static void *lock(void *data, void **p_pixels)
{

	videoContext* context = (videoContext*)data;
	p_pixels[0] = context->pixeldata;

	return NULL;
}

static void unlock(void *data, void *id, void *const *p_pixels)
{
	videoContext* context = (videoContext*)data;
	context->time = libvlc_media_player_get_time(context->player);
	context->currentFrame++;
}

static void display(void *data, void *id)
{

}

static unsigned int formatSetup(void **opaque, char *chroma, unsigned *w, unsigned *h, unsigned *pitches, unsigned *lines)
{
	videoContext* context = (videoContext*)*opaque;

	memcpy(chroma, "RV24", sizeof("RV24") - 1);
	
	context->width = *w;
	context->height = *h;
	
	*pitches = context->width * 3;
	*lines = context->height;
	
	context->pixeldata = (unsigned char*)malloc(context->width * context->height * 4);
	context->duration = libvlc_media_player_get_length(context->player);
	context->time = 0;
	context->volume = libvlc_audio_get_volume(context->player);

	return 1;
}

videoContext* initVideo(const char* fileName)
{
	videoContext* mainContext = (videoContext*)malloc(sizeof(videoContext));
	memset(mainContext, 0, sizeof(videoContext));
	mainContext->instance = libvlc_new(0, NULL);
	mainContext->media = libvlc_media_new_location(mainContext->instance, fileName);
	mainContext->player = libvlc_media_player_new_from_media(mainContext->media);
	libvlc_video_set_callbacks(mainContext->player, lock, unlock, display, mainContext);
	libvlc_video_set_format_callbacks(mainContext->player, formatSetup, NULL);
	libvlc_media_player_play(mainContext->player);

	return mainContext;
}

void destroyVideo(videoContext* context)
{
	libvlc_media_player_release(context->player);
	libvlc_media_release(context->media);
	free(context->pixeldata);
	free(context);

}

void setTime(videoContext* context, long time)
{
	libvlc_media_player_set_time(context->player, time);
}

void setVolume(videoContext* context, int volume)
{
	libvlc_audio_set_volume(context->player, volume);
	context->volume = volume;
}

void playPause(videoContext* context)
{
	if (libvlc_media_player_is_playing(context->player))
		libvlc_media_player_pause(context->player);
	else
		libvlc_media_player_play(context->player);
}