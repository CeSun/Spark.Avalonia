using Spark.Assets;
using Spark.Avalonia;
using Spark.Avalonia.Assets;

namespace Spark.Importer;

public static class Importer
{
    public static Texture ImportTexture(this Engine engine, Stream stream)
    {
        var texture = new Texture();
        // todo Import
        engine.RenderMethods.Add(texture.SetupRender);
        return texture;
    }

    public static StaticMesh ImportStaticMeshFromGLB(this Engine engine, Stream stream)
    {
        var staticMesh = new StaticMesh();
        // todo Import
        return staticMesh;
    }
}
