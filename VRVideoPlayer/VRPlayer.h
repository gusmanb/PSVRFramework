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
