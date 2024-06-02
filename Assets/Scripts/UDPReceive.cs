using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private Texture2D texture;
    private byte[] receivedImageBytes;
    private bool newDataReceived = false;
    public string data;

    public string serverIP = "127.0.0.1";
    public int serverPort = 8080;
    private bool startReceiving = true;
    private object lockObject = new object();

    void Start()
    {
        texture = new Texture2D(2, 2);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();
            byte[] lengthBuffer = new byte[8];

            while (startReceiving)
            {
                try
                {
                    int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break;

                    long dataLength = BitConverter.ToInt64(lengthBuffer, 0);
                    byte[] dataBuffer = new byte[dataLength];
                    int totalRead = 0;
                    while (totalRead < dataLength)
                    {
                        bytesRead = stream.Read(dataBuffer, totalRead, dataBuffer.Length - totalRead);
                        if (bytesRead == 0) break;
                        totalRead += bytesRead;
                    }

                    lock (lockObject)
                    {
                        receivedImageBytes = dataBuffer;
                        newDataReceived = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error in ReceiveData: " + e.ToString());
                    startReceiving = false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in ReceiveData initialization: " + e.ToString());
        }
    }

    void Update()
    {
        if (newDataReceived)
        {
            lock (lockObject)
            {
                texture.LoadImage(receivedImageBytes);
                newDataReceived = false;
            }
        }
    }

    void OnGUI()
    {
        if (texture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, ScaleMode.ScaleToFit);
        }
    }

    void OnApplicationQuit()
    {
        startReceiving = false;
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Join();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
