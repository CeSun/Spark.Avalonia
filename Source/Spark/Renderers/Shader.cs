using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public Shader Using(GL gl)
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
    public static Shader CreateShader(this GL gl, string vs, string fs)
    {

        return new Shader() 
        {
        };
    }
}
