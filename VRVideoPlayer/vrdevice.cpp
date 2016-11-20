#include "vrdevice.h"
#include <math.h>
#include <glm\glm.hpp>
#include <glm/gtc/matrix_transform.hpp>

namespace VRDevice 
{
	float distort(float coefficients[], float radius)
	{
		float result = 1;
		float rFactor = 1;
		float rSquared = radius * radius;
		
		rFactor *= rSquared; //unnecessary, can be shorted, left to increase readability
		result += coefficients[0] * rFactor;
		rFactor *= rSquared;
		result += coefficients[1] * rFactor;

		return result;
	}

	float distortInverse(float coefficients[], float radius)
	{
		float r0 = radius / 0.9f;
		float r1 = radius * 0.9f;
		float dr0 = radius - distort(coefficients, r0);
		
		while (abs(r1 - r0) > 0.0001) //0.1mm
		{
			float dr1 = radius - distort(coefficients, r1);
			float r2 = r1 - dr1 * ((r1 - r0) / (dr1 - dr0));
			r0 = r1;
			r1 = r2;
			dr0 = dr1;
		}

		return r1;
	}

	glm::vec4 projectionMatrixToVector(glm::mat4 matrix)
	{

		glm::vec4 vec = glm::vec4(
			matrix[0][0],
			matrix[1][1],
			matrix[2][0] - 1,
			matrix[2][1] - 1 );

		vec /= 2.0f;

		return vec;
	}

	void initializeDevice(vrdevicedata* deviceData, const vrscreenprops* screenProps, const vrlensprops* lensProps)
	{
		memcpy(&deviceData->screenProperties, screenProps, sizeof(vrscreenprops));
		memcpy(&deviceData->lensProperties, lensProps, sizeof(vrlensprops));

		vrphysicalprops* pprops = &deviceData->physicalProperties;
		pprops->widthMeters = (METERS_PER_INCH / screenProps->xDPI) * screenProps->xRes;
		pprops->heightMeters = (METERS_PER_INCH / screenProps->yDPI) * screenProps->yRes;
		pprops->bevelMeters = 0; //Cardboard residual data, left to test it

		//Compute undirstorted FOV
		float eyeToScreenDistance = deviceData->lensProperties.screenLensDistance;
		float halfLensDistance = deviceData->lensProperties.interLensDistance / 2 / eyeToScreenDistance;
		float screenWidth = pprops->widthMeters / eyeToScreenDistance;
		float screenHeight = pprops->heightMeters / eyeToScreenDistance;

		float eyePosX = screenWidth / 2 - halfLensDistance;
		float eyePosY = (deviceData->lensProperties.baselineDistance - pprops->bevelMeters) / eyeToScreenDistance;

 		float maxFov = deviceData->lensProperties.fov;
		float viewerMax = distortInverse(deviceData->lensProperties.distortionCoeffs, tan(glm::radians(maxFov)));
		float outerDist = glm::min(eyePosX, viewerMax);
		float innerDist = glm::min(halfLensDistance, viewerMax);
		float bottomDist = glm::min(eyePosY, viewerMax);
		float topDist = glm::min(screenHeight - eyePosY, viewerMax);

		vrfov* ulf = &deviceData->undistortedLeftFOV;

		ulf->upDegrees = glm::degrees(atan(topDist));
		ulf->downDegrees = glm::degrees(atan(bottomDist));
		ulf->leftDegrees = glm::degrees(atan(outerDist));
		ulf->rightDegrees = glm::degrees(atan(innerDist));
		
		//Compute distorted FOV
		outerDist = (pprops->widthMeters - deviceData->lensProperties.interLensDistance) / 2;
		innerDist = deviceData->lensProperties.interLensDistance / 2;
		bottomDist = deviceData->lensProperties.baselineDistance - pprops->bevelMeters;
		topDist = pprops->heightMeters - bottomDist;

		vrfov* dlf = &deviceData->distortedLeftFOV;

		ulf->upDegrees = glm::min(glm::degrees(atan(distort(deviceData->lensProperties.distortionCoeffs, topDist / eyeToScreenDistance))), maxFov);
		dlf->downDegrees = glm::min(glm::degrees(atan(distort(deviceData->lensProperties.distortionCoeffs, bottomDist / eyeToScreenDistance))), maxFov);
		dlf->leftDegrees = glm::min(glm::degrees(atan(distort(deviceData->lensProperties.distortionCoeffs, outerDist / eyeToScreenDistance))), maxFov);
		dlf->rightDegrees = glm::min(glm::degrees(atan(distort(deviceData->lensProperties.distortionCoeffs, innerDist / eyeToScreenDistance))), maxFov);
		
		//Compute undistorted projection
		float top = tan(glm::radians(ulf->upDegrees)) * NEAR_DISTANCE;
		float bottom = tan(glm::radians(ulf->downDegrees)) * NEAR_DISTANCE;
		float left = tan(glm::radians(ulf->leftDegrees)) * NEAR_DISTANCE;
		float right = tan(glm::radians(ulf->rightDegrees)) * NEAR_DISTANCE;
		
		deviceData->undistortedProjection = glm::frustumRH(-left, right, -bottom, top, NEAR_DISTANCE, FAR_DISTANCE);

		//Compute distorted projection
		top = tan(glm::radians(dlf->upDegrees)) * NEAR_DISTANCE;
		bottom = tan(glm::radians(dlf->downDegrees)) * NEAR_DISTANCE;
		left = tan(glm::radians(dlf->leftDegrees)) * NEAR_DISTANCE;
		right = tan(glm::radians(dlf->rightDegrees)) * NEAR_DISTANCE;

		deviceData->distortedProjection = glm::frustumRH(-left, right, -bottom, top, NEAR_DISTANCE, FAR_DISTANCE);
		
		deviceData->projectionLeft = projectionMatrixToVector(deviceData->distortedProjection);
		deviceData->unprojectionLeft = projectionMatrixToVector(deviceData->undistortedProjection);
	}
}