using Silk.NET.Windowing;

var options = WindowOptions.Default with { API = new GraphicsAPI(ContextAPI.OpenGLES, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(3, 0)) };
var window = Window.Create(options);

window.Run();

window.Load += () =>
{

};
window.Render += deltaTime =>
{

};

window.Update += deltaTime =>
{

};

window.Closing += () =>
{

};