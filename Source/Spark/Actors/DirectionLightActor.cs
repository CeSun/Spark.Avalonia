namespace Spark.Actors;

public class DirectionLightActor(Engine engine) : BaseLightActor(engine), IActorCreator<DirectionLightActor>
{
    public new static DirectionLightActor Create(Engine engine) => new(engine);
}
