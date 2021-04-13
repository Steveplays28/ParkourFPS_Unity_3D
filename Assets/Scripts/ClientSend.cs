using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    /// <summary>Sends a packet to the server via TCP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to the server via UDP.</summary>
    /// <param name="_packet">The packet to send to the sever.</param>
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    /// <summary>Lets the server know that the welcome message was received.</summary>
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void Ping()
    {
        using (Packet _packet = new Packet((int)ClientPackets.ping))
        {
            SendUDPData(_packet);
        }
    }

    /// <summary>Sends player input to the server.</summary>
    /// <param name="_inputs">The currently pressed inputs.</param>
    /// <param name="_playerRotation">The current player rotation.</param>
    /// <param name="_cameraRotation">The current camera rotation.</param>
    public static void PlayerMovement(bool[] _inputs, Quaternion _playerRotation, Quaternion _cameraRotation)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(_playerRotation);
            _packet.Write(_cameraRotation);

            SendUDPData(_packet);
        }
    }

    public static void PlayerShoot()
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerStopShooting()
    {
        using (Packet packet = new Packet((int)ClientPackets.playerStopShooting))
        {
            SendTCPData(packet);
        }
    }

    public static void PlayerThrowItem(Vector3 _facing)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerThrowItem))
        {
            _packet.Write(_facing);

            SendTCPData(_packet);
        }
    }

    public static void PlayerJump()
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerJump))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerRun()
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerRun))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerCrouch()
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerCrouch))
        {
            SendTCPData(_packet);
        }
    }

    public static void PlayerEquipWeapon(int weaponId)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerEquipWeapon))
        {
            packet.Write(weaponId);

            SendTCPData(packet);
        }
    }

    public static void PlayerReloadWeapon()
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerReloadWeapon))
        {
            SendTCPData(_packet);
        }
    }
    #endregion
}
