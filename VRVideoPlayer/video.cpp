#include "video.h"
#include <string.h>
#include <vlc\libvlc.h>
#include <vlc\libvlc_media.h>
#include <vlc\libvlc_media_player.h>
#include <vlc\libvlc_events.h>
#include "texture.h"
#include <windows.h>

namespace Video
{

	static void *lock(void *data, void **p_pixels)
	{
		videoContext* context = (videoContext*)data;

		bool expected = false;

		if (!context->lock.compare_exchange_strong(expected, true))
			p_pixels[0] = NULL;
		else
			p_pixels[0] = context->pixeldata;

		return NULL;
	}

	static void unlock(void *data, void *id, void *const *p_pixels)
	{

		if (p_pixels == NULL)
			return;

		videoContext* context = (videoContext*)data;
		context->time = libvlc_media_player_get_time(context->player);
		context->lock.store(false);
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

	void videoFinished(const struct libvlc_event_t *event, void *data)
	{
		videoContext* context = (videoContext*)data;
		context->finished = true;
	}

	videoContext* initVideo(const char* fileName)
	{
		const char * const vlc_args[] = {
			"--avcodec-hw=any" };

		videoContext* mainContext = (videoContext*)malloc(sizeof(videoContext));
		memset(mainContext, 0, sizeof(videoContext));
		mainContext->instance = libvlc_new(1, vlc_args);
		mainContext->media = libvlc_media_new_location(mainContext->instance, fileName);
		mainContext->player = libvlc_media_player_new_from_media(mainContext->media);
		libvlc_video_set_callbacks(mainContext->player, lock, unlock, display, mainContext);
		libvlc_video_set_format_callbacks(mainContext->player, formatSetup, NULL);
		libvlc_media_player_play(mainContext->player);

		auto manager = libvlc_media_player_event_manager(mainContext->player);
		libvlc_event_attach(manager, libvlc_MediaPlayerStopped, &videoFinished, mainContext);

		return mainContext;
	}

	void destroyVideo(videoContext* context)
	{
		auto manager = libvlc_media_player_event_manager(context->player);
		libvlc_event_detach(manager, libvlc_MediaPlayerStopped, &videoFinished, context);
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

}