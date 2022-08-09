namespace Piot.Surge.DatagramType
{
    public enum DatagramType
    {
        Reserved,
        DeltaSnapshots, // Sent from simulating, arbitrating host to client
        PredictedInputs, // Sent from client to host
    }
}