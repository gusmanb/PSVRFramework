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

#include "shader.h"

#include <string>
#include <vector>
#include <iostream>
#include <fstream>
#include <stdio.h>
#include <stdlib.h>
#include <GL\glew.h>
#include <GLFW\glfw3.h>
#include <glm\glm.hpp>
#include <GL\GL.h>
#include <glm/gtc/type_ptr.hpp>
namespace Shader
{

	GLuint createShader(char const * vertex_shader_content, char const * fragment_shader_content)
	{

		// Create the shaders
		GLuint VertexShaderID = glCreateShader(GL_VERTEX_SHADER);
		GLuint FragmentShaderID = glCreateShader(GL_FRAGMENT_SHADER);

		GLint Result = GL_FALSE;
		int InfoLogLength;


		// Compile Vertex Shader
		glShaderSource(VertexShaderID, 1, &vertex_shader_content, NULL);
		glCompileShader(VertexShaderID);

		// Check Vertex Shader
		glGetShaderiv(VertexShaderID, GL_COMPILE_STATUS, &Result);
		glGetShaderiv(VertexShaderID, GL_INFO_LOG_LENGTH, &InfoLogLength);
		if (InfoLogLength > 0) {
			std::vector<char> VertexShaderErrorMessage(InfoLogLength + 1);
			glGetShaderInfoLog(VertexShaderID, InfoLogLength, NULL, &VertexShaderErrorMessage[0]);
			printf("%s\n", &VertexShaderErrorMessage[0]);
		}



		// Compile Fragment Shader
		glShaderSource(FragmentShaderID, 1, &fragment_shader_content, NULL);
		glCompileShader(FragmentShaderID);

		// Check Fragment Shader
		glGetShaderiv(FragmentShaderID, GL_COMPILE_STATUS, &Result);
		glGetShaderiv(FragmentShaderID, GL_INFO_LOG_LENGTH, &InfoLogLength);
		if (InfoLogLength > 0) {
			std::vector<char> FragmentShaderErrorMessage(InfoLogLength + 1);
			glGetShaderInfoLog(FragmentShaderID, InfoLogLength, NULL, &FragmentShaderErrorMessage[0]);
			printf("%s\n", &FragmentShaderErrorMessage[0]);
		}



		// Link the program
		printf("Linking program\n");
		GLuint ProgramID = glCreateProgram();
		glAttachShader(ProgramID, VertexShaderID);
		glAttachShader(ProgramID, FragmentShaderID);
		glLinkProgram(ProgramID);

		// Check the program
		glGetProgramiv(ProgramID, GL_LINK_STATUS, &Result);
		glGetProgramiv(ProgramID, GL_INFO_LOG_LENGTH, &InfoLogLength);
		if (InfoLogLength > 0) {
			std::vector<char> ProgramErrorMessage(InfoLogLength + 1);
			glGetProgramInfoLog(ProgramID, InfoLogLength, NULL, &ProgramErrorMessage[0]);
			printf("%s\n", &ProgramErrorMessage[0]);
		}


		glDetachShader(ProgramID, VertexShaderID);
		glDetachShader(ProgramID, FragmentShaderID);

		glDeleteShader(VertexShaderID);
		glDeleteShader(FragmentShaderID);

		return ProgramID;
	}

	void destroyShader(GLuint programId)
	{
		glDeleteProgram(programId);
	}

	void setUniformMatrix(GLuint programId, const char* uniformName, glm::mat4 matrix)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniformMatrix4fv(uniformId, 1, GL_FALSE, glm::value_ptr(matrix));
	}

	void setUniformVec2(GLuint programId, const char* uniformName, glm::vec2 vector)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniform2fv(uniformId, 1, glm::value_ptr(vector));
	}

	void setUniformVec2(GLuint programId, const char* uniformName, float* vector)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniform2fv(uniformId, 1, vector);
	}

	void setUniformVec3(GLuint programId, const char* uniformName, glm::vec3 vector)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniform3fv(uniformId, 1, glm::value_ptr(vector));
	}

	void setUniformVec4(GLuint programId, const char* uniformName, glm::vec4 vector)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniform4fv(uniformId, 1, glm::value_ptr(vector));
	}

	void setUniformInt(GLuint programId, const char* uniformName, int value)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniform1i(uniformId, value);
	}

	void setUniformFloat(GLuint programId, const char* uniformName, float value)
	{
		GLint uniformId = glGetUniformLocation(programId, uniformName);
		glUniform1f(uniformId, value);
	}
}