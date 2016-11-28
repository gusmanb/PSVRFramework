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
#include <glm\glm.hpp>
namespace VRDevice
{

	#define METERS_PER_INCH 0.0254f
	#define  NEAR_DISTANCE 0.1f
	#define FAR_DISTANCE 100.0f

	typedef struct _vrscreenprops
	{
		float xRes;
		float yRes;
		float xDPI;
		float yDPI;
		

	} vrscreenprops;

	typedef struct _vrphysicalprops
	{
		float widthMeters;
		float heightMeters;
		float bevelMeters;

	} vrphysicalprops;

	typedef struct _vrlensprops
	{
		float fov;
		float interLensDistance;
		float baselineDistance;
		float screenLensDistance;
		float distortionCoeffs[2];

	} vrlensprops;

	typedef struct _vrfov
	{
		float upDegrees;
		float downDegrees;
		float leftDegrees;
		float rightDegrees;
		
	} vrfov;

	typedef struct _vrviewport
	{
		float x;
		float y;
		float width;
		float height;

	} vrviewport;

	typedef struct _vrparams
	{
		float outerDist;
		float innerDist;
		float topDist;
		float bottomDist;
		float eyePosX;
		float eyePosY;

	} vrparams;

	typedef struct _vrdevice
	{
		//Needed data
		vrscreenprops screenProperties;
		vrlensprops lensProperties;
		//Computed data
		vrphysicalprops physicalProperties;
		vrviewport undistortedLeftViewport;
		vrfov undistortedLeftFOV;
		vrfov distortedLeftFOV;
		glm::mat4 distortedProjection;
		glm::mat4 undistortedProjection;
		glm::vec4 projectionLeft;
		glm::vec4 unprojectionLeft;
		
	} vrdevicedata;

	const vrscreenprops PSVRScreenProps = 
	{ 
		//Info taken from https://www.playstation.com/en-ie/explore/playstation-vr/tech-specs/

		1920, //width in pixels
		1080, //height in pixels
		386.47f, //x DPI, computed from res/screen diagonal size, may not be accurated
		386.47f //y DPI, computed from res/screen diagonal size, may not be accurated

	};

	const vrlensprops PSVRLensProps = {

		//Info used for this: https://support.google.com/cardboard/manufacturers/answer/6324808?hl=en&ref_topic=6322188 and https://github.com/borismus/webvr-boilerplate/blob/d91cc2866bd54e65d59022800f62c7e160dc9fee/src/device-info.js
		//ILD, BD and DC can be found experimentally, in the previous link are some tests describing how to check

		68.0f, //FOV
		0.0630999878f, //Distance between the lens centers, the hdm by default is configured for an IPD of 64mm, I assume this is the inter-lens distance
		0.0394899882f, // Distance between viewer baseline and lens center in meters.
		0.0354f, //Distance between the lenses and the screen.
		{ 0.22f, 0.24f } //Distortion coefficients, K1 and K2, must be tweaked


	};

	void initializeDevice(vrdevicedata* deviceData, const vrscreenprops* screenProps, const vrlensprops* lensProps);
}