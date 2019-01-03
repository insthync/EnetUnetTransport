using ENet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public class EnetUnetTransport : INetworkTransport
{
    public const string TAG = "EnetUnetTransport";

    private int nextConnectionId = 1;
    private int tempConnectionId;
    private int nextHostId = 1;
    private int tempHostId;
    private bool isStarted;
    private GlobalConfig globalConfig;
    private Dictionary<int, HostTopology> topologies = new Dictionary<int, HostTopology>();
    private Dictionary<int, Dictionary<int, Host>> hosts = new Dictionary<int, Dictionary<int, Host>>();
    private Dictionary<int, Dictionary<int, Peer>> connections = new Dictionary<int, Dictionary<int, Peer>>();
    private Dictionary<uint, int> connectionIds = new Dictionary<uint, int>();
    private Host tempHost;
    private Peer tempPeer;
    private ENet.Event tempEvent;

    public bool IsStarted
    {
        // True if the object has been initialized and is ready to be used.
        get { return isStarted; }
    }

    private void AddConnection(int hostId, int connectionId, Peer peer)
    {
        if (!connections.ContainsKey(hostId))
            connections.Add(hostId, new Dictionary<int, Peer>());
        connections[hostId][connectionId] = peer;
        connectionIds[peer.ID] = connectionId;
    }

    private bool AddHostByConfig(int hostId, int port, string ip, int specialConnectionId, HostTopology topology)
    {
        var success = false;
        var maxConnections = topology.MaxDefaultConnections;
        var config = topology.DefaultConfig;
        var address = new Address();
        if (specialConnectionId > 0)
            config = topology.SpecialConnectionConfigs[specialConnectionId - 1];

        // Create new host with its event listener
        tempHost = new Host();

        if (specialConnectionId > 0)
        {
            tempHost.Create();
        }
        else
        {
            if (!string.IsNullOrEmpty(ip))
            {
                address.SetHost(ip);
                tempHost.Create(address, maxConnections);
            }
            else if (port > 0)
            {
                address.Port = (ushort)port;
                tempHost.Create(address, maxConnections);
            }
            else
            {
                tempHost.Create();
            }
        }

        if (!hosts.ContainsKey(hostId))
            hosts.Add(hostId, new Dictionary<int, Host>());
        hosts[hostId][specialConnectionId] = tempHost;
        Debug.Log("[" + TAG + "] added host " + hostId + " port=" + port + " ip=" + ip);

        return success;
    }

    public int AddHost(HostTopology topology, int port, string ip)
    {
        // Creates a host based on HostTopology.
        tempHostId = nextHostId++;
        topologies[tempHostId] = topology;
        for (var i = 0; i < topology.SpecialConnectionConfigsCount + 1; ++i)
        {
            AddHostByConfig(tempHostId, port, ip, i, topology);
        }
        return tempHostId;
    }

    public int AddHostWithSimulator(HostTopology topology, int minTimeout, int maxTimeout, int port)
    {
        // Creates a host and configures it to simulate Internet latency(works on Editor and development
        // builds only).
        Debug.LogWarning("[" + TAG + "] AddHostWithSimulator() not implemented, it will use AddHost()");
        return AddHost(topology, port, null);
    }

    public int AddWebsocketHost(HostTopology topology, int port, string ip)
    {
        // Creates a web socket host.
        Debug.LogWarning("[" + TAG + "] AddWebsocketHost() not implemented, it will use AddHost()");
        return AddHost(topology, port, ip);
    }

    public int Connect(int hostId, string ip, int port, int specialConnectionId, out byte error)
    {
        // Tries to establish a connection to another peer.
        Debug.Log("[" + TAG + "] Connecting to hostId " + hostId + " address " + ip + " port " + port);
        error = (byte)NetworkError.UsageError;
        tempConnectionId = 0;
        if (hosts.ContainsKey(hostId))
        {
            Address address = new Address();
            address.SetHost(ip);
            address.Port = (ushort)port;
            tempPeer = hosts[hostId][specialConnectionId].Connect(address);
            if (tempPeer.IsSet)
            {
                tempConnectionId = nextConnectionId++;
                AddConnection(hostId, tempConnectionId, tempPeer);
                error = (byte)NetworkError.Ok;
            }
            else
            {
                Debug.LogError("[" + TAG + "] Cannot connect to hostId " + hostId + " address " + address + " port " + port);
            }
        }
        else
        {
            Debug.LogError("[" + TAG + "] Cannot connect to hostId " + hostId);
        }
        return tempConnectionId;
    }

    public void ConnectAsNetworkHost(int hostId, string address, int port, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        // Creates a dedicated connection to Relay server
        Debug.LogError("[" + TAG + "] ConnectAsNetworkHost() not implemented");
        throw new System.NotImplementedException();
    }

    public int ConnectEndPoint(int hostId, EndPoint endPoint, int specialConnectionId, out byte error)
    {
        // Tries to establish a connection to the peer specified by the given C# System.EndPoint.
        return Connect(hostId, ((IPEndPoint)endPoint).Address.ToString(), ((IPEndPoint)endPoint).Port, specialConnectionId, out error);
    }

    public int ConnectToNetworkPeer(int hostId, string address, int port, int specialConnectionId, int relaySlotId, NetworkID network, SourceID source, NodeID node, out byte error)
    {
        // Creates a connection to another peer in the Relay group.
        Debug.LogError("[" + TAG + "] ConnectToNetworkPeer() not implemented");
        throw new System.NotImplementedException();
    }

    public int ConnectWithSimulator(int hostId, string address, int port, int specialConnectionId, out byte error, ConnectionSimulatorConfig conf)
    {
        // Tries to establish a connection to another peer with added simulated latency.
        Debug.LogWarning("[" + TAG + "] ConnectWithSimulator() not implemented, it will use Connect()");
        return Connect(hostId, address, port, specialConnectionId, out error);
    }

    public bool Disconnect(int hostId, int connectionId, out byte error)
    {
        // Sends a disconnect signal to the connected peer and closes the connection.
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return false;
        }
        if (!connections[hostId].TryGetValue(connectionId, out tempPeer))
        {
            error = (byte)NetworkError.WrongConnection;
            return false;
        }
        error = (byte)NetworkError.Ok;
        tempPeer.DisconnectNow(0);
        return true;
    }

    public bool DoesEndPointUsePlatformProtocols(EndPoint endPoint)
    {
        // Checks whether the specified end point uses platform-specific protocols.
        return false;
    }

    public void GetBroadcastConnectionInfo(int hostId, out string address, out int port, out byte error)
    {
        // After INetworkTransport.Receive() returns a NetworkEventType.BroadcastEvent, this function 
        // returns the connection information of the broadcast sender. This information can then be used 
        // for connecting to the broadcast sender.
        Debug.LogError("[" + TAG + "] GetBroadcastConnectionInfo() not implemented");
        throw new System.NotImplementedException();
    }

    public void GetBroadcastConnectionMessage(int hostId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // 	After INetworkTransport.Receive() returns NetworkEventType.BroadcastEvent, this function
        // returns a complimentary message from the broadcast sender.
        Debug.LogError("[" + TAG + "] GetBroadcastConnectionMessage() not implemented");
        throw new System.NotImplementedException();
    }

    public void GetConnectionInfo(int hostId, int connectionId, out string address, out int port, out NetworkID network, out NodeID dstNode, out byte error)
    {
        // Returns the connection parameters for the specified connectionId. These parameters can be sent
        // to other users to establish a direct connection to this peer. If this peer is connected to the host
        // via Relay, the Relay-related parameters are set.
        Debug.Log("[" + TAG + "] GetConnectionInfo()");
        address = "";
        port = -1;
        network = NetworkID.Invalid;
        dstNode = NodeID.Invalid;
        error = (byte)NetworkError.UsageError;
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return;
        }
        if (!connections[hostId].ContainsKey(connectionId))
        {
            error = (byte)NetworkError.WrongConnection;
            return;
        }
        tempPeer = connections[hostId][connectionId];
        address = tempPeer.IP;
        port = tempPeer.Port;
        error = (byte)NetworkError.Ok;
    }

    public int GetCurrentRTT(int hostId, int connectionId, out byte error)
    {
        // Return the round trip time for the given connectionId.
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return 0;
        }
        if (!connections[hostId].ContainsKey(connectionId))
        {
            error = (byte)NetworkError.WrongConnection;
            return 0;
        }
        error = (byte)NetworkError.Ok;
        return (int)connections[hostId][connectionId].RoundTripTime;
    }

    public void Init()
    {
        // Initializes the object implementing INetworkTransport. Must be called before doing any other
        // operations on the object.
        Debug.Log("[" + TAG + "] Init() globalConfig=" + (globalConfig != null));
        // Init default transport to make everything works, this is HACK
        // I actually don't know what I have to do with init function
        NetworkManager.defaultTransport.Init();
        isStarted = true;
    }

    public void Init(GlobalConfig config)
    {
        // Initializes the object implementing INetworkTransport. Must be called before doing any other
        // operations on the object.
        globalConfig = config;
        Init();
    }

    public NetworkEventType Receive(out int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // Polls the underlying system for events.
        hostId = 0;
        connectionId = 0;
        channelId = 0;
        receivedSize = 0;
        error = (byte)NetworkError.Ok;
        foreach (var currentHostId in hosts.Keys)
        {
            var result = ReceiveFromHost(currentHostId, out connectionId, out channelId, buffer, bufferSize, out receivedSize, out error);
            if (result != NetworkEventType.Nothing)
            {
                hostId = currentHostId;
                return result;
            }
        }
        return NetworkEventType.Nothing;
    }

    public NetworkEventType ReceiveFromHost(int hostId, out int connectionId, out int channelId, byte[] buffer, int bufferSize, out int receivedSize, out byte error)
    {
        // Similar to INetworkTransport.Receive but will only poll for the provided hostId.
        connectionId = 0;
        channelId = 0;
        receivedSize = 0;
        error = (byte)NetworkError.Ok;

        if (!hosts.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return NetworkEventType.Nothing;
        }

        NetworkEventType eventType;
        var hostsByConfig = hosts[hostId].Values;
        foreach (var hostByConfig in hostsByConfig)
        {
            hostByConfig.Service(0, out tempEvent);
            eventType = NetworkEventType.Nothing;
            channelId = tempEvent.ChannelID;
            
            switch (tempEvent.Type)
            {
                case ENet.EventType.Connect:
                    eventType = NetworkEventType.ConnectEvent;
                    if (!connectionIds.ContainsKey(tempEvent.Peer.ID))
                    {
                        connectionId = nextConnectionId++;
                        AddConnection(hostId, connectionId, tempEvent.Peer);
                    }
                    break;

                case ENet.EventType.Disconnect:
                    eventType = NetworkEventType.DisconnectEvent;
                    break;

                case ENet.EventType.Timeout:
                    eventType = NetworkEventType.DisconnectEvent;
                    error = (byte)NetworkError.Timeout;
                    break;

                case ENet.EventType.Receive:
                    eventType = NetworkEventType.DataEvent;
                    var length = tempEvent.Packet.Length;
                    if (length <= bufferSize)
                    {
                        tempEvent.Packet.CopyTo(buffer);
                        receivedSize = length;
                    }
                    else
                        error = (byte)NetworkError.MessageToLong;
                    tempEvent.Packet.Dispose();
                    break;
            }

            if (eventType != NetworkEventType.Nothing)
            {
                connectionId = connectionIds[tempEvent.Peer.ID];
                return eventType;
            }
        }

        // If event is nothing set default data
        connectionId = 0;
        channelId = 0;
        receivedSize = 0;
        return NetworkEventType.Nothing;
    }

    public NetworkEventType ReceiveRelayEventFromHost(int hostId, out byte error)
    {
        // Polls the host for the following events: NetworkEventType.ConnectEvent and
        // NetworkEventType.DisconnectEvent.
        Debug.LogError("[" + TAG + "] ReceiveRelayEventFromHost() not implemented");
        throw new System.NotImplementedException();
    }

    public bool RemoveHost(int hostId)
    {
        // Closes the opened transport pipe, and closes all connections belonging to that transport pipe.
        // Disconnection connection
        if (connections.ContainsKey(hostId))
        {
            var tempConnections = connections[hostId].ToArray();
            foreach (var entry in tempConnections)
            {
                entry.Value.DisconnectNow(0);
                connections[hostId].Remove(entry.Key);
                connectionIds.Remove(entry.Value.ID);
            }
            connections[hostId].Clear();
            connections.Remove(hostId);
        }
        // Stop host
        if (hosts.ContainsKey(hostId))
        {
            var tempHosts = hosts[hostId].ToArray();
            foreach (var entry in tempHosts)
            {
                entry.Value.Flush();
                entry.Value.Dispose();
                hosts[hostId].Remove(entry.Key);
            }
            hosts.Remove(hostId);
            return true;
        }
        return false;
    }

    public bool Send(int hostId, int connectionId, int channelId, byte[] buffer, int size, out byte error)
    {
        // Sends data to peer with the given connection ID.
        error = (byte)NetworkError.UsageError;
        if (!connections.ContainsKey(hostId))
        {
            error = (byte)NetworkError.WrongHost;
            return false;
        }
        if (!connections[hostId].ContainsKey(connectionId))
        {
            error = (byte)NetworkError.WrongConnection;
            return false;
        }
        var packetFlag = PacketFlags.None;
        switch (topologies[hostId].DefaultConfig.Channels[channelId].QOS)
        {
            case QosType.AllCostDelivery:
                packetFlag = PacketFlags.Reliable | PacketFlags.UnreliableFragment;
                break;
            case QosType.Reliable:
                packetFlag = PacketFlags.Reliable | PacketFlags.Unsequenced;
                break;
            case QosType.ReliableFragmented:
                packetFlag = PacketFlags.Reliable | PacketFlags.UnreliableFragment | PacketFlags.Unsequenced;
                break;
            case QosType.ReliableFragmentedSequenced:
                packetFlag = PacketFlags.Reliable | PacketFlags.UnreliableFragment;
                break;
            case QosType.ReliableSequenced:
                packetFlag = PacketFlags.Reliable;
                break;
            case QosType.ReliableStateUpdate:
                packetFlag = PacketFlags.Reliable;
                break;
            case QosType.StateUpdate:
                packetFlag = PacketFlags.None;
                break;
            case QosType.Unreliable:
                packetFlag = PacketFlags.None | PacketFlags.Unsequenced;
                break;
            case QosType.UnreliableFragmented:
                packetFlag = PacketFlags.None | PacketFlags.UnreliableFragment;
                break;
            case QosType.UnreliableFragmentedSequenced:
                packetFlag = PacketFlags.None | PacketFlags.UnreliableFragment | PacketFlags.Unsequenced;
                break;
            case QosType.UnreliableSequenced:
                packetFlag = PacketFlags.None;
                break;
        }
        Packet packet = new Packet();
        byte[] data = new byte[size];
        System.Buffer.BlockCopy(buffer, 0, data, 0, size);
        packet.Create(data, size, packetFlag);
        error = (byte)NetworkError.Ok;
        return connections[hostId][connectionId].Send(0, ref packet);
    }

    public void SetBroadcastCredentials(int hostId, int key, int version, int subversion, out byte error)
    {
        // Sets the credentials required for receiving broadcast messages. If the credentials of a received
        // broadcast message do not match, that broadcast discovery message is dropped.
        Debug.LogError("[" + TAG + "] SetBroadcastCredentials() not implemented");
        throw new System.NotImplementedException();
    }

    public void SetPacketStat(int direction, int packetStatId, int numMsgs, int numBytes)
    {
        // Keeps track of network packet statistics.
        NetworkManager.defaultTransport.SetPacketStat(direction, packetStatId, numMsgs, numBytes);
    }

    public void Shutdown()
    {
        // Shuts down the transport object.
        var tempHosts = hosts.Keys.ToArray();
        foreach (var hostId in tempHosts)
        {
            RemoveHost(hostId);
        }
        connections.Clear();
        connectionIds.Clear();
        hosts.Clear();
        nextConnectionId = 1;
        nextHostId = 1;
    }

    public bool StartBroadcastDiscovery(int hostId, int broadcastPort, int key, int version, int subversion, byte[] buffer, int size, int timeout, out byte error)
    {
        // Starts sending a broadcasting message across all local subnets.
        Debug.LogError("[" + TAG + "] StartBroadcastDiscovery() not implemented");
        throw new System.NotImplementedException();
    }

    public void StopBroadcastDiscovery()
    {
        // Stops sending the broadcast discovery message across all local subnets.
        Debug.LogError("[" + TAG + "] StopBroadcastDiscovery() not implemented");
        throw new System.NotImplementedException();
    }
}
