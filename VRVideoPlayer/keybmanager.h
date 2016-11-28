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
#include <stdbool.h>
#include <GLFW\glfw3.h>

namespace KeybManager
{

	typedef void(*keybCallback)(int Key, bool Control, bool Alt, bool Shift);

	typedef enum _keyState
	{
		KEY_RELEASED,
		KEY_PRESSED

	} keyState;

	typedef struct _keyNode
	{
		int key;
		bool ctrl;
		bool alt;
		bool shift;
		keyState status;
		keybCallback callback;
		struct _keyNode* prev;
		struct _keyNode* next;

	}keyNode;

	void registerKey(int Key, bool Control, bool Alt, bool Shift, keybCallback Callback);
	void deregisterKey(int Key, bool Control, bool Alt, bool Shift);
	void updateKeyboard(GLFWwindow* window);
	void clearKeys();

}