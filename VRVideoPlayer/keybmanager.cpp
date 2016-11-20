#include "keybmanager.h"
#include <stddef.h>
#include <stdbool.h>
#include <stdlib.h>
#include <GLFW\glfw3.h>

namespace KeybManager
{

	keyNode* first = NULL;
	keyNode* last = NULL;

	void registerKey(int Key, bool Control, bool Alt, bool Shift, keybCallback Callback)
	{
		if (first == NULL)
		{
			first = (keyNode*)malloc(sizeof(keyNode));
			last = first;
			first->key = Key;
			first->ctrl = Control;
			first->alt = Alt;
			first->shift = Shift;
			first->callback = Callback;
			first->status = KEY_RELEASED;
			first->next = NULL;
			first->prev = NULL;

		}
		else
		{
			last->next = (keyNode*)malloc(sizeof(keyNode));
			last->next->prev = last;
			last = last->next;
			last->key = Key;
			last->ctrl = Control;
			last->alt = Alt;
			last->shift = Shift;
			last->callback = Callback;
			last->status = KEY_RELEASED;
			last->next = NULL;
		}
	}

	void deregisterKey(int Key, bool Control, bool Alt, bool Shift)
	{
		if (first == NULL)
			return;

		if (first->key == Key && first->ctrl == Control && first->alt == Alt && first->shift == Shift)
		{
			keyNode* del = first;

			first = first->next;

			if (last == del)
				last = NULL;

			free(del);
		}
		else if (last->key == Key && last->ctrl == Control && last->alt == Alt && last->shift == Shift)
		{
			keyNode* del = last;
			last = last->prev;
			free(del);
		}
		else
		{
			keyNode* item = first->next;

			while (item != NULL)
			{
				if (item->key == Key && item->ctrl == Control && item->alt == Alt && item->shift == Shift)
				{
					item->next->prev = item->prev;
					item->prev->next = item->next;
					free(item);
					return;
				}
				else
					item = item->next;
			}
		}
	}

	void updateKeyboard(GLFWwindow* window)
	{
		keyNode* item = first;

		bool ctrl = glfwGetKey(window, GLFW_KEY_LEFT_CONTROL) == KEY_PRESSED || glfwGetKey(window, GLFW_KEY_RIGHT_CONTROL) == KEY_PRESSED;
		bool alt = glfwGetKey(window, GLFW_KEY_LEFT_ALT) == KEY_PRESSED || glfwGetKey(window, GLFW_KEY_RIGHT_ALT) == KEY_PRESSED;
		bool shift = glfwGetKey(window, GLFW_KEY_LEFT_SHIFT) == KEY_PRESSED || glfwGetKey(window, GLFW_KEY_RIGHT_SHIFT) == KEY_PRESSED;

		while (item != NULL)
		{
			keyState status = (keyState)glfwGetKey(window, item->key);

			if (status != item->status)
			{
				if (status == KEY_RELEASED)
					item->status = status;
				else if (item->ctrl == ctrl && item->alt == alt && item->shift == shift)
				{
					item->status = status;
					item->callback(item->key, ctrl, alt, shift);
				}
			}

			item = item->next;
		}
	}

	void clearKeys()
	{
		keyNode* item = first->next;

		while (item != NULL)
		{
			keyNode* temp = item;
			item = item->next;
			free(temp);

		}

		first = NULL;
		last = NULL;
	}
}