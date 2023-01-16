using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;


public class NetServer : MonoBehaviour
{
    class NetworkPlayer
    {
        GameObject gameObject;
        string clientKey;
        float lastReceivedPacket;
        public NetworkPlayer(GameObject playerPrefab, string clientKey)
        {
            gameObject = GameObject.Instantiate(playerPrefab);
            this.clientKey = clientKey;
        }

        public string GetClientKey()
        {
            return clientKey;
        }

        public void ManagePacket(byte[] packet, int len)
        {
            if (len < 12 + 4 + 12 + 8)
            {
                return;
            }

            lastReceivedPacket = Time.time;

            float x = BitConverter.ToSingle(packet, 0);
            float y = BitConverter.ToSingle(packet, 4);
            float z = BitConverter.ToSingle(packet, 8);

            float ry = BitConverter.ToSingle(packet, 12);

            float red = BitConverter.ToSingle(packet, 16);
            float green = BitConverter.ToSingle(packet, 20);
            float blue = BitConverter.ToSingle(packet, 24);

            string playerName = Encoding.ASCII.GetString(packet, 28, 8);

            gameObject.GetComponentInChildren<TextMesh>().text = playerName + "\n" + clientKey;
            gameObject.GetComponent<Renderer>().material.color = new Color(red, green, blue, 1);
            gameObject.name = "Player: " + playerName;
            gameObject.transform.position = new Vector3(x, y, z);
            gameObject.transform.eulerAngles = new Vector3(0, ry, 0);
        }

        public float GetLastReceivedPacketTimestamp()
        {
            return lastReceivedPacket;
        }

        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    string serverAddress = "0.0.0.0";

    [SerializeField]
    int port = 9999;

    [SerializeField]
    int maxPacketSize = 512;

    [SerializeField]
    int maxPacketsPerFrame = 500;

    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    float deadClientTolerance = 10;

    Socket socket;

    byte[] packet;
    IPEndPoint clientIPEndPoint;
    EndPoint clientEndPoint;

    [SerializeField]
    Dictionary<string, NetworkPlayer> knownClients;

    // Start is called before the first frame update
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Blocking = false;

        IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), port);
        socket.Bind(serverEndPoint);

        packet = new byte[maxPacketSize];

        clientIPEndPoint = new IPEndPoint(0, 0);
        clientEndPoint = clientIPEndPoint as EndPoint;

        knownClients = new Dictionary<string, NetworkPlayer>();


    }

    // Update is called once per frame
    void Update()
    {
        float now = Time.time;
        List<NetworkPlayer> deadPlayers = new List<NetworkPlayer>();
        foreach (var client in knownClients.Values)
        {
            if (now - client.GetLastReceivedPacketTimestamp() > deadClientTolerance)
            {
                deadPlayers.Add(client);
            }
        }

        foreach (var client in deadPlayers)
        {
            knownClients[client.GetClientKey()].DestroyGameObject();
            knownClients.Remove(client.GetClientKey());
        }

        for (int i = 0; i < maxPacketsPerFrame; i++)
        {
            int rlen = -1;
            try
            {
                rlen = socket.ReceiveFrom(packet, ref clientEndPoint);
                Debug.Log(rlen.ToString() + " " + clientEndPoint.ToString());
            }
            catch (SocketException)
            {
                break;
            }


            string clientKey = clientEndPoint.ToString();

            if (!knownClients.ContainsKey(clientKey))
            {
                Debug.Log("new client");
                knownClients.Add(clientKey, new NetworkPlayer(playerPrefab, clientKey));
            }

            knownClients[clientKey].ManagePacket(packet, rlen);
        }
    }
}