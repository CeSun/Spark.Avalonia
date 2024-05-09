using Silk.NET.OpenGLES;
using System.Numerics;

namespace Spark.Renderers;


public class Shader : IDisposable
{
    public uint ProgramId { get; internal set; }

    private GL? _gl;
    public void Dispose()
    {
        if (_gl == null) 
            return;
        _gl.UseProgram(0);
        _gl = null;
    }


    public void SetInt(string name, int value)
    {
        if (_gl == null)
            return;
        var location = _gl.GetUniformLocation(ProgramId, name);
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
        _gl.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        if (_gl == null)
            return;
        var location = _gl.GetUniformLocation(ProgramId, name);
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
        _gl.Uniform1(location, value);
    }

    public void SetVector2(string name, Vector2 value)
    {
        if (_gl == null)
            return;
        var location = _gl.GetUniformLocation(ProgramId, name);
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
        _gl.Uniform2(location, value);
    }

    public void SetVector3(string name, Vector3 value)
    {
        if (_gl == null)
            return;
        var location = _gl.GetUniformLocation(ProgramId, name);
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
        _gl.Uniform3(location, value);
    }

    public unsafe void SetMatrix(string name, Matrix4x4 value)
    {
        if (_gl == null)
            return;
        var location = _gl.GetUniformLocation(ProgramId, name);
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
        _gl.UniformMatrix4(location, 1, false, (float*)&value);
    }
    public Shader Use(GL gl)
    {
        this._gl = gl;
        gl.UseProgram(ProgramId);
        return this;
    }
   
}

public static class ShaderHelper
{
    public static string PreProcessShaderSource(string file, List<string>? micros = default)
    {
        var source = SparkResource.ResourceManager.GetString(file)!;
        var lines = source.Split("\n", StringSplitOptions.TrimEntries).ToList();
        for (int i = 0; i < lines.Count; i ++)
        {
            var line = lines[i];
            line = line.Trim().Replace(" ", "");
            var crossLineComments = false;

            if (line.IndexOf("//", StringComparison.Ordinal) >= 0)
            {
                lines[i] = line[..line.IndexOf("//", StringComparison.Ordinal)];
            }
            else if (crossLineComments == false && line.Contains("\\*") && line.IndexOf("*\\", StringComparison.Ordinal) > 0)
            {
                lines[i] = line[..line.IndexOf("\\*", StringComparison.Ordinal)];
                lines[i] += line.Substring(line.IndexOf("*\\", StringComparison.Ordinal) + 2, line.Length - line.IndexOf("*\\", StringComparison.Ordinal) + 2);
            }
            else if (crossLineComments == false && line.Contains("\\*", StringComparison.CurrentCulture))
            {
                lines[i] = line[..line.IndexOf("\\*", StringComparison.Ordinal)];
                crossLineComments = true;
            }
            else if (crossLineComments && line.IndexOf("*\\", StringComparison.Ordinal) > 0)
            {
                lines[i] = line.Substring(line.IndexOf("*\\", StringComparison.Ordinal) + 2, line.Length - line.IndexOf("*\\", StringComparison.Ordinal) + 2);
            }
            else if (crossLineComments)
            {
                lines[i] = "";
            }
            else if (line.StartsWith("#include"))
            {
                var start = line.IndexOf('<');
                var end = line.IndexOf('>');
                if (start == -1)
                {
                    start = line.IndexOf('"');
                    end = line.IndexOf('"');
                }
                if (start == -1 || end == -1 || end <= start)
                    throw new Exception("shader error");
                var path = line.Substring(start + 1, end - start - 1);
                lines[i] = PreProcessShaderSource(path, null);
            }
        }

        if (micros is not { Count: > 0 }) 
            return string.Join("\n", lines);
        
        foreach (var line in micros.Select(micro => $"#define {micro}"))
        {
            lines.Insert(0, line);
        }
       
        return string.Join("\n", lines);
    }

    private static bool? _isOpenGlES;
    private const string GlESHeader = "#version 300 es\r\nprecision mediump float;\r\n";
    private const string GlHeader = "#version 330 core\r\nprecision mediump float;\r\n";

    public static Shader CreateShader(this GL gl, string vs, string fs, List<string>? micros = default)
    {
        if (_isOpenGlES == null)
        {
            var version = gl.GetStringS(GLEnum.Version);
            if (version.Replace(" ", "").ToLower().IndexOf("opengles", StringComparison.Ordinal) >= 0 )
            {
                _isOpenGlES = true;
            }
            else
            {
                _isOpenGlES = false;
            }
        }
        var vertShaderSource = (_isOpenGlES == true? GlESHeader: GlHeader) + PreProcessShaderSource(vs, micros);
        var fragShaderSource = (_isOpenGlES == true ? GlESHeader : GlHeader) + PreProcessShaderSource(fs, micros);
        var vert = gl.CreateShader(GLEnum.VertexShader);

        gl.ShaderSource(vert, vertShaderSource);
        gl.CompileShader(vert);
        gl.GetShader(vert, GLEnum.CompileStatus, out int code);
        if (code == 0)
        {
            var info = gl.GetShaderInfoLog(vert);
            Console.WriteLine(vertShaderSource);
            throw new Exception(info);
        }
        var frag = gl.CreateShader(GLEnum.FragmentShader);
        gl.ShaderSource(frag, fragShaderSource);
        gl.CompileShader(frag);
        gl.GetShader(frag, GLEnum.CompileStatus, out code);
        if (code == 0)
        {
            gl.DeleteShader(vert);
            var info = gl.GetShaderInfoLog(frag);
            Console.WriteLine(fragShaderSource);
            throw new Exception(info);
        }

        var programId = gl.CreateProgram();
        gl.AttachShader(programId, vert);
        gl.AttachShader(programId, frag);
        gl.LinkProgram(programId);
        gl.GetProgram(programId, GLEnum.LinkStatus, out code);
        if (code == 0)
        {
            gl.DeleteShader(vert);
            gl.DeleteShader(frag);

            var info = gl.GetProgramInfoLog(programId);
            throw new Exception(info);
        }
        gl.DeleteShader(vert);
        gl.DeleteShader(frag);
        return new Shader() 
        {
            ProgramId = programId,
        };
    }
}
