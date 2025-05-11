using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace SteveEngine
{
    public class Shader
    {
        public int ProgramId { get; private set; }
        
        public Shader(string vertexCode, string fragmentCode)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexCode);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentCode);
            
            ProgramId = GL.CreateProgram();
            GL.AttachShader(ProgramId, vertexShader);
            GL.AttachShader(ProgramId, fragmentShader);
            GL.LinkProgram(ProgramId);
            
            GL.GetProgram(ProgramId, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(ProgramId);
                Console.WriteLine($"Error linking program: {infoLog}");
            }
            
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }
        
        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"Error compiling {type} shader: {infoLog}");
            }
            
            return shader;
        }
        
        public void Use()
        {
            GL.UseProgram(ProgramId);
        }
        
        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.Uniform1(location, value);
        }
        
        public void SetFloat(string name, float value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.Uniform1(location, value);
        }
        
        public void SetVector3(string name, Vector3 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.Uniform3(location, value);
        }
        
        public void SetMatrix4(string name, Matrix4 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            GL.UniformMatrix4(location, false, ref value);
        }
    }
}