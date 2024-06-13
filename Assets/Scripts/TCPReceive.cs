using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

public class TCPReceive : MonoBehaviour
{
    private Thread receiveThread; // ������ ���� ������
    private TcpClient tcpClient; // TCP Ŭ���̾�Ʈ
    private NetworkStream stream; // �����κ��� ������ �б� ���� ��Ʈ��ũ ��Ʈ��

    private Texture2D texture; // ���� �̹��� ǥ�� �ؽ���
    private byte[] receivedImageBytes; // ���� �̹��� ���� ����
    private bool newDataReceived = false; // ���ο� ������ ���� ���� �÷���

    public string serverIP = "127.0.0.1"; // ���� IP �ּ�
    public int tcpServerPort = 8080; // TCP ��Ʈ ��ȣ

    private bool startReceiving = true; // ������ ���� ���� ���� �÷���

    private object lockObject = new object(); // ������ ����ȭ ���� ��� ��ü

    public GameObject playWorkout; // UI ������Ʈ
    public int counter; // Ƚ�� ���� ����
    public bool printToConsole = false; // ������ �ܼ� ��� ����
    public string data;

    void Start()
    {
        // �ؽ�ó �ʱ�ȭ
        texture = new Texture2D(2, 2);

        // ���� ������ ����
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            // ���� ����
            tcpClient = new TcpClient(serverIP, tcpServerPort);
            stream = tcpClient.GetStream();
            byte[] lengthBuffer = new byte[8]; // ������ ������ ���� ���� ����

            while (startReceiving)
            {
                try
                {
                    // �̹��� ������ ���� ����
                    int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break; // ������ ������ ������ ���� ����

                    long imageLength = BitConverter.ToInt64(lengthBuffer, 0);  // ���� ���۸� long���� ��ȯ
                    byte[] imageDataBuffer = new byte[imageLength]; // ������ �̹��� ������ ���� ���� ����
                    int totalRead = 0;

                    // ���ۿ� �̹��� ������ �о����
                    while (totalRead < imageLength)
                    {
                        bytesRead = stream.Read(imageDataBuffer, totalRead, imageDataBuffer.Length - totalRead);
                        if (bytesRead == 0) break; // ������ ������ ������ ���� ����
                        totalRead += bytesRead; // �� ���� ����Ʈ �� ������Ʈ
                    }

                    // counter �� ����
                    bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break; // ������ ������ ������ ���� ����

                    long counterValue = BitConverter.ToInt64(lengthBuffer, 0);

                    // ��ü ��װ� ���ŵ� �̹��� ����Ʈ �� counter �� ������Ʈ
                    lock (lockObject)
                    {
                        receivedImageBytes = imageDataBuffer;
                        counter = (int)counterValue;
                        newDataReceived = true; // ���ο� ������ ���ŵǾ����� ��Ÿ���� �÷��� ����
                    }

                    if (printToConsole) { print(counter); }
                }
                catch (Exception e)
                {
                    Debug.LogError("ReceiveData���� ���� �߻�: " + e.ToString());
                    startReceiving = false; // ���� �߻� �� ������ ���� ����
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ReceiveData �ʱ�ȭ���� ���� �߻�: " + e.ToString());
            startReceiving = false; // ���� �߻� �� ������ ���� ����
        }
        finally
        {
            // ���ҽ� ����
            if (stream != null) stream.Close();
            if (tcpClient != null) tcpClient.Close();
        }
    }

    void Update()
    {
        // ���ο� ������ ���ŵǸ� �ؽ�ó�� ���ο� �̹����� ������Ʈ
        if (newDataReceived)
        {
            lock (lockObject)
            {
                texture.LoadImage(receivedImageBytes); // ���ŵ� �̹��� ����Ʈ�� �ؽ�ó�� �ε�
                newDataReceived = false; // �÷��� �ʱ�ȭ
            }
            UpdateUIImage();
        }
    }

    private void UpdateUIImage()
    {
        // playWorkout ������Ʈ�� ��ȿ���� Ȯ��
        if (playWorkout != null)
        {
            // UI ������Ʈ���� Image ������Ʈ ��������
            Image imageComponent = playWorkout.GetComponent<Image>();
            if (imageComponent != null)
            {
                // �ؽ�ó�� ��������Ʈ�� ��ȯ�Ͽ� UI �̹����� ����
                Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imageComponent.sprite = newSprite;
            }
            else
            {
                Debug.LogError("playWorkout ������Ʈ�� Image ������Ʈ�� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("playWorkout ������Ʈ�� �������� �ʾҽ��ϴ�.");
        }
    }

    void OnApplicationQuit()
    {
        // ���ø����̼� ���� �� ������ ���� �����ϰ� ���ҽ� ����
        startReceiving = false;
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Join(); // ���� ������ ����� ������ ���
        if (stream != null) stream.Close(); // ��Ʈ��ũ ��Ʈ�� �ݱ�
        if (tcpClient != null) tcpClient.Close(); // TCP Ŭ���̾�Ʈ �ݱ�
    }
}
