#pragma once
#include <glm\glm.hpp>
namespace VRDevice
{

	#define METERS_PER_INCH 0.0254f
	//Check if these are physical properties or must be the same as the camera projection
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

		54, //computed, hdm has 100º of horizontal FOV, the aspect ratio is 16:9, vertical FOV = 68
		0.0635f, //Distance between the lens centers, the hdm by default is configured for an IPD of 64mm, I assume this is the inter-lens distance
		0.0565f, // Distance between viewer baseline and lens center in meters. No real data for this, cardboard has 0.035 by default, but reading info this can be 0 on the PSVR (eye centered on the lens)
		0.037f, //Distance between the lenses and the screen. No data for this, just cardboard values
		{ 0.22f, 0.12f } //Distortion coefficients, K1 and K2, taken fron the oculus rift dk1 (had similar properties to the PS VR), must be tweaked

		//50, //computed, hdm has 100º of horizontal FOV, the aspect ratio is 16:9, vertical FOV = 68
		//0.056f, //Distance between the lens centers, the hdm by default is configured for an IPD of 64mm, I assume this is the inter-lens distance
		//0.033f, // Distance between viewer baseline and lens center in meters. No real data for this, cardboard has 0.035 by default, but reading info this can be 0 on the PSVR (eye centered on the lens)
		//0.040f, //Distance between the lenses and the screen. No data for this, just cardboard values
		//{ 0.36f, 1.1f } //Distortion coefficients, K1 and K2, taken fron the oculus rift dk1 (had similar properties to the PS VR), must be tweaked
	};

	void initializeDevice(vrdevicedata* deviceData, const vrscreenprops* screenProps, const vrlensprops* lensProps);
}