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