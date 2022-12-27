namespace Piot.Surge.Ecs2
{
    public interface IEcsWorldSetter
    {
        public void Set<T>(uint entityId, T data) where T : struct;
    }
}