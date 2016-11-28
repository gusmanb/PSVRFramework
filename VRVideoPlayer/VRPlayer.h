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
#include "mainFunctions.h"
#pragma managed(pop)

namespace VRVideoPlayer {


	public ref class VRPlayer
	{
	public:
		VRPlayer()
		{

		}

		~VRPlayer()
		{

		}

		bool LaunchPlayer()
		{
			if (fileName == nullptr || glThread != nullptr)
				return false;

			glThread = gcnew Thread(gcnew ThreadStart(this, &VRPlayer::glThreadProc));
			glThread->Start();
			return true;
		}
		 
		bool StopPlayer()
		{
			if (glThread == nullptr)
				return false;

			endOpenGL();
			glThread->Join();
			glThread = nullptr;

			return true;
		}

		void RotateMesh(float quat[])
		{
			extRotate(quat);
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
		void glThreadProc()
		{
			if (!initOpenGL())
				throw gcnew InvalidOperationException("Cannot init OpenGL"); 

			runLoop((const char*)Marshal::StringToHGlobalAnsi(fileName).ToPointer());
		}

	};
}
