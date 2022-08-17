using Piot.Flood;

namespace Piot.Surge.Pulse.Client
{
    public interface IClientPredictorCorrections
    {
        public void ReadCorrections(IOctetReader snapshotReader);
    }
}