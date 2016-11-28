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

using namespace System;
using namespace System::Threading;
using namespace System::Runtime::InteropServices;

#pragma managed(push, off)
#include "engine.h"
#pragma managed(pop)

namespace VRVideoPlayer {

	public ref class Monitor
	{
	public:
		property int Index;
		property String^ Name;
	};

	public ref class Vector2
	{
	public:
		float X;
		float Y;
	};
	public ref class Vector3
	{
	public:
		float X;
		float Y;
		float Z;
	};
	public ref class EyeConfiguration
	{
	public:
		Vector2^ Offset;
		Vector2^ Scale;
		EyeConfiguration()
		{
			Offset = gcnew Vector2();
			Scale = gcnew Vector2();
		}
	};

	public ref class VideoSettings
	{
	public:
		EyeConfiguration^ LeftEye;
		EyeConfiguration^ RightEye;
		Vector3^ InitialRotation;
		int MonitorIndex;
		float VFOV;
		float HFOV;
		bool Equilateral;

		VideoSettings()
		{
			LeftEye = gcnew EyeConfiguration();
			RightEye = gcnew EyeConfiguration();
			InitialRotation = gcnew Vector3();
		}

	};

	public ref class VRPlayer
	{
	public:
		event EventHandler^ PlayFinished;
		VRPlayer()
		{

		}

		~VRPlayer()
		{
			StopPlayer();
		}

		void UpdateRotation(float x, float y, float z, float w)
		{
			Engine::updateRotation(x, y, z, w);
		}

		/*void UpdateRotation(float x, float y, float z)
		{
			Engine::updateRotation(x, y, z);
		}*/

		static array<Monitor^>^ GetMonitors()
		{
			int count;
			auto nativeMonitors = Engine::getScreens(&count);

			array<Monitor^>^ monitors = gcnew array<Monitor^>(count);

			for (int buc = 0; buc < count; buc++)
			{
				Engine::monitor nmon = nativeMonitors[buc];
				Monitor^ mon = gcnew Monitor();
				mon->Index = nmon.index;
				IntPtr ptr = System::IntPtr((void*)nmon.name);
				mon->Name = Marshal::PtrToStringAnsi(ptr);
				monitors[buc] = mon;
				free(nmon.name);
			}

			free(nativeMonitors);

			return monitors;
		}

		bool LaunchPlayer(VideoSettings ^Settings)
		{
			if (fileName == nullptr || glThread != nullptr)
				return false;

			glThread = gcnew Thread(gcnew ParameterizedThreadStart(this, &VRPlayer::glThreadProc));
			glThread->Start(Settings);
			return true;
		}
		 
		bool StopPlayer()
		{
			if (glThread == nullptr)
				return false;

			Engine::stop();
			glThread->Join();
			glThread = nullptr;

			return true;
		}

		property String^ FileName
		{
			String^ get()
			{
				return fileName;
			}
			void set(String^ value)
			{
				fileName = value;
			}
		}

	private:
		Thread^ glThread;
		String^ fileName;
		
		void glThreadProc(System::Object^ Param)
		{
			Engine::videoSettings settings;
			VideoSettings^ set = (VideoSettings ^)Param;
			
			settings.leftEye.offset.x = set->LeftEye->Offset->X;
			settings.rightEye.offset.x = set->RightEye->Offset->X;
			settings.leftEye.offset.y = set->LeftEye->Offset->Y;
			settings.rightEye.offset.y = set->RightEye->Offset->Y;
			
			settings.leftEye.scale.x = set->LeftEye->Scale->X;
			settings.rightEye.scale.x = set->RightEye->Scale->X;
			settings.leftEye.scale.y = set->LeftEye->Scale->Y;
			settings.rightEye.scale.y = set->RightEye->Scale->Y;

			settings.initialRotation.x = set->InitialRotation->X;
			settings.initialRotation.y = set->InitialRotation->Y;
			settings.initialRotation.z = set->InitialRotation->Z;
			
			settings.hfov = set->HFOV;
			settings.vfov = set->VFOV;

			settings.equilateral = set->Equilateral;

			settings.monitorIndex = set->MonitorIndex;

			if (!init(settings))
				throw gcnew InvalidOperationException("Cannot init OpenGL"); 

			Engine::run((const char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer());

			PlayFinished(this, EventArgs::Empty);
		}

	};
}
