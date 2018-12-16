namespace Sanguo.Core
{
    public interface ISanguoPlugin
    {
        void OnServerLoaded();
        void OnClientLoaded();
    }
}
