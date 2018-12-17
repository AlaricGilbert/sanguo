namespace Sanguo.Core
{
    public interface ISanguoPlugin
    {
        void OnServerLoadedOnly();
        void OnClientLoadedOnly();
        void OnAnyLoadedCommon();
    }
}
