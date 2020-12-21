using ParkourFPS_Server;
using UnityEngine;

public static class ClientSend
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.udpTestReceive))
        {
            _packet.Write("Received a UDP packet.");

            SendUDPData(_packet);
        }
    }

    public static void PlayerInput(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerInput))
        {
            foreach(bool _input in _inputs)
            {
                _packet.Write(_input);
            }

            SendUDPData(_packet);
        }
    }
    #endregion
}
