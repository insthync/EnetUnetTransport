using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public class EnetUnetTransport : INetworkTransport
{
    public const string TAG = "EnetUnetTransport";

    private bool isStarted;

    public bool IsStarted
    {
        // True if the object has been initialized and is ready to be used.
        get { return isStarted; }
    }

    public int AddHost(HostTopology topology, int port, string ip)
    {
        throw new System.NotImplementedException();
    }

    public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port)
    {
        throw new System.NotImplementedException();
    }

    public int AddWebsocketHost(HostTopology topology, int port, string ip)
    {
        throw new System.NotImplementedException();
    }

    public int Connect(int hostId, string address, int port, int specialConnectionId, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void ConnectAsNetworkHost(int hostId, string address, int port, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public int ConnectEndPoint(int hostId, EndPoint endPoint, int specialConnectionId, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public int ConnectToNetworkPeer(int hostId, string address, int port, int specialConnectionId, int relaySlotId, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public int ConnectWithSimulator(int hostId, string address, int port, int specialConnectionId, out byte error, ConnectionSimulatorConfig conf)
    {
        throw new System.NotImplementedException();
    }

    public bool Disconnect(int hostId, int connectionId, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public bool DoesEndPointUsePlatformProtocols(EndPoint endPoint)
    {
        throw new System.NotImplementedException();
    }

    public void GetBroadcastConnectionInfo(int hostId, out string address, out int port, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void GetBroadcastConnectionMessage(int hostId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void GetConnectionInfo(int hostId, int connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public int GetCurrentRTT(int hostId, int connectionId, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void Init()
    {
        throw new System.NotImplementedException();
    }

    public void Init(GlobalConfig config)
    {
        throw new System.NotImplementedException();
    }

    public NetworkEventType Receive(out int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public NetworkEventType ReceiveFromHost(int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public NetworkEventType ReceiveRelayEventFromHost(int hostId, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public bool RemoveHost(int hostId)
    {
        throw new System.NotImplementedException();
    }

    public bool Send(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void SetBroadcastCredentials(int hostId, int key, int version, int subversion, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void SetPacketStat(int direction, int packetStatId, int numMsgs, int numBytes)
    {
        throw new System.NotImplementedException();
    }

    public void Shutdown()
    {
        throw new System.NotImplementedException();
    }

    public bool StartBroadcastDiscovery(int hostId, int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout, out byte error)
    {
        throw new System.NotImplementedException();
    }

    public void StopBroadcastDiscovery()
    {
        throw new System.NotImplementedException();
    }
}
