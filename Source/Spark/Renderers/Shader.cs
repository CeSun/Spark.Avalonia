using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Renderers;

public class Shader : IDisposable
{
    public uint ProgramId { get; internal set; }

    private GL? gl;
    public void Dispose()
    {
        if (gl != null)
        {
            gl.UseProgram(0);
            gl = null;
        }
    }


    public void SetInt(string name, int value)
    {
        if (gl == null)
            return;
        var location = gl.GetUniformLocation(ProgramId, name);
#if DEBUG && TraceShaderUniformError
        if (location < 0)
        {
            string stackInfo = new StackTrace().ToString();
            Console.WriteLine("Not Found Location: " + name);
            Console.WriteLine(stackInfo);
        }

#else
        if (location < 0)
        {
            return;
        }
#endif
        gl.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        if (gl == null)
            return;
        var location = gl.GetUniformLocation(ProgramId, name);
#if DEBUG && TraceShaderUniformError
        if (location < 0)
        {
            string stackInfo = new StackTrace().ToString();
            Console.WriteLine("Not Found Location: " + name);
            Console.WriteLine(stackInfo);
        }

#else
        if (location < 0)
        {
            return;
        }
#endif
        gl.Uniform1(location, value);
    }

    public void SetVector2(string name, Vector2 value)
    {
        if (gl == null)
            return;
        var location = gl.GetUniformLocation(ProgramId, name);
#if DEBUG && TraceShaderUniformError
        if (location < 0)
        {
            string stackInfo = new StackTrace().ToString();
            Console.WriteLine("Not Found Location: " + name);
            Console.WriteLine(stackInfo);
        }

#else
        if (location < 0)
        {
            return;
        }
#endif
        gl.Uniform2(location, value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        if (gl == null)
            return;
        var location = gl.GetUniformLocation(ProgramId, name);
#if DEBUG && TraceShaderUniformError
        if (location < 0)
        {
            string stackInfo = new StackTrace().ToString();
            Console.WriteLine("Not Found Location: " + name);
            Console.WriteLine(stackInfo);
        }

#else
        if (location < 0)
        {
            return;
        }
#endif
        gl.Uniform3(location, value);
    }

    public unsafe void SetMatrix(string name, Matrix4x4 value)
    {
        if (gl == null)
            return;
        var location = gl.GetUniformLocation(ProgramId, name);
#if DEBUG && TraceShaderUniformError
        if (location < 0)
        {
            string stackInfo = new StackTrace().ToString();
            Console.WriteLine("Not Found Location: " + name);
            Console.WriteLine(stackInfo);
        }
#else
        if (location < 0)
        {
            return;
        }
#endif
        gl.UniformMatrix4(location, 1, false, (float*)&value);
    }
    public Shader Use(GL gl)
    {
        this.gl = gl;
        gl.UseProgram(ProgramId);
        return this;
    }
   
}

public static class ShaderHelper
{
    public static string PreProcessShaderSource(string File, List<string>? Micros = default)
    {
        var source = SparkResource.ResourceManager.GetString(File)!;
        var lines = source.Split("\n", StringSplitOptions.TrimEntries).ToList();
        for (int i = 0; i < lines.Count; i ++)
        {
            var line = lines[i];
            line = line.Trim().Replace(" ", "");
            bool CrosslineComments = false;

            if (line.IndexOf("//") >= 0)
            {
                lines[i] = line.Substring(0, line.IndexOf("//"));
            }
            else if (CrosslineComments == false && line.IndexOf("\\*") >= 0 && line.IndexOf("*\\") > 0)
            {
                lines[i] = line.Substring(0, line.IndexOf("\\*"));
                lines[i] += line.Substring(line.IndexOf("*\\") + 2, line.Length - line.IndexOf("*\\") + 2);
            }
            else if (CrosslineComments == false && line.IndexOf("\\*") >= 0)
            {
                lines[i] = line.Substring(0, line.IndexOf("\\*"));
                CrosslineComments = true;
            }
            else if (CrosslineComments == true && line.IndexOf("*\\") > 0)
            {
                lines[i] = line.Substring(line.IndexOf("*\\") + 2, line.Length - line.IndexOf("*\\") + 2);
            }
            else if (CrosslineComments == true)
            {
                lines[i] = "";
            }
            else if (line.StartsWith("#include"))
            {
                var start = line.IndexOf("<");
                var end = line.IndexOf(">");
                if (start == -1)
                {
                    start = line.IndexOf("\"");
                    end = line.IndexOf("\"");
                }
                if (start == -1 || end == -1 || end <= start)
                    throw new Exception("shader error");
                var path = line.Substring(start + 1, end - start - 1);
                lines[i] = PreProcessShaderSource(path, null);
            }
        }
        if (Micros != null && Micros.Count > 0) 
        {
            foreach(var micro in Micros)
            {
                var line = $"#define {micro}";
                lines.Insert(1, line);
            }
        }
        return string.Join("\n", lines);
    }
    public static Shader CreateShader(this GL gl, string vs, string fs, List<string>? Micros = default)
    {
        var VertShaderSource = PreProcessShaderSource(vs, Micros);
        var FragShaderSource = PreProcessShaderSource(fs, Micros);
        var vert = gl.CreateShader(GLEnum.VertexShader);

        gl.ShaderSource(vert, VertShaderSource);
        gl.CompileShader(vert);
        gl.GetShader(vert, GLEnum.CompileStatus, out int code);
        if (code == 0)
        {
            var info = gl.GetShaderInfoLog(vert);
            Console.WriteLine(VertShaderSource);
            throw new Exception(info);
        }
        var frag = gl.CreateShader(GLEnum.FragmentShader);
        gl.ShaderSource(frag, FragShaderSource);
        gl.CompileShader(frag);
        gl.GetShader(frag, GLEnum.CompileStatus, out code);
        if (code == 0)
        {
            gl.DeleteShader(vert);
            var info = gl.GetShaderInfoLog(frag);
            Console.WriteLine(FragShaderSource);
            throw new Exception(info);
        }

        var ProgramId = gl.CreateProgram();
        gl.AttachShader(ProgramId, vert);
        gl.AttachShader(ProgramId, frag);
        gl.LinkProgram(ProgramId);
        gl.GetProgram(ProgramId, GLEnum.LinkStatus, out code);
        if (code == 0)
        {
            gl.DeleteShader(vert);
            gl.DeleteShader(frag);

            var info = gl.GetProgramInfoLog(ProgramId);
            throw new Exception(info);
        }
        gl.DeleteShader(vert);
        gl.DeleteShader(frag);
        return new Shader() 
        {
            ProgramId = ProgramId,
        };
    }
}
