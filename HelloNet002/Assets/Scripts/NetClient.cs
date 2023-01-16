using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text;

public struct PlayerBuffer
{
    public Vector3 position;
    public float yRotation;
    public Color color;
    public string name;
}

public class NetClient : MonoBehaviour
{
    [SerializeField]
    string serverAddress;

    [SerializeField]
    int serverPort;

    [SerializeField]
    ClientPuppetcontroller controller;

    Socket socket;
    IPEndPoint serverEndPoint;

    byte[] packet;

    int state = 0;

    int randomValue = 0;

    PlayerBuffer playerBuffer;
    Material playerMat;


    private void Awake()
    {
        if (controller == null)
        {
            Debug.LogError("NetClient must have an associated ClientPuppetController instance");
            Application.Quit();
        }
        playerBuffer = new PlayerBuffer();
        playerMat = controller.gameObject.GetComponent<MeshRenderer>().material;
    }

    // Start is called before the first frame update
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;

        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);

        packet = new byte[512];
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerBuffer();
        PlayerBufferConversion(playerBuffer);


        if (state == 0) // send the first "handshake"
        {
            randomValue = UnityEngine.Random.Range(0, 100000);
            BitConverter.GetBytes(randomValue).CopyTo(packet, 0);

            socket.SendTo(packet, 4, SocketFlags.None, serverEndPoint);

            state = 1;
        }
        else if (state == 1)
        {
            EndPoint endPoint = serverEndPoint as EndPoint;
            try
            {
                int rlen = socket.ReceiveFrom(packet, ref endPoint);
                if (rlen == 4)
                {
                    int serverValue = BitConverter.ToInt32(packet, 0);
                    int totalChallenge = randomValue + serverValue;
                    byte[] challengeBytes = BitConverter.GetBytes(totalChallenge);
                    SHA256 hash = SHA256.Create();
                    byte[] hashedChallenge = hash.ComputeHash(challengeBytes);
                    socket.SendTo(hashedChallenge, serverEndPoint);
                    state = 2;
                }
            }
            catch (SocketException)
            {
                return;
            }
        } else if (state == 2)
        {
            UpdatePlayerBuffer();
            byte[] buffer = PlayerBufferConversion(playerBuffer);
            socket.SendTo(buffer, serverEndPoint);
        }
    }

    private void UpdatePlayerBuffer()
    {
        playerBuffer.position = controller.transform.position;
        playerBuffer.yRotation = controller.transform.rotation.y;
        playerBuffer.color = playerMat.color;
        playerBuffer.name = controller.PlayerName;
    }

    private byte[] PlayerBufferConversion(PlayerBuffer buffer)
    {
        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms, Encoding.ASCII);
        bw.Write(buffer.position.x);
        bw.Write(buffer.position.y);
        bw.Write(buffer.position.z);
        bw.Write(buffer.yRotation);
        bw.Write(buffer.color.r);
        bw.Write(buffer.color.g);
        bw.Write(buffer.color.b);
        bw.Write(buffer.name);
        byte[] result = ms.ToArray();

        return result;
    }

        
}