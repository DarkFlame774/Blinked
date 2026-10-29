using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Blink : MonoBehaviour
{
    [SerializeField] GameObject altWorld;
    [SerializeField] GameObject trans;
    [SerializeField] GameObject altlights;
    [SerializeField] GameObject lights;
    UdpClient udpClient;
    Thread receiveThread;
    public int port = 5052;
    public bool blinkDetected = false;

    void Start()
    {
        udpClient = new UdpClient(port);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);
                if (message == "blink")
                {
                    blinkDetected = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Error: " + e);
            }
        }
    }

    void Update()
    {
        if (blinkDetected)
        {
            if (trans.activeSelf)
            {
                altWorld.SetActive(true);
                altlights.SetActive(true);
                trans.SetActive(false);
                lights.SetActive(false);

            }
            else
            {
                altWorld.SetActive(false);
                altlights.SetActive(false);
                trans.SetActive(true);
                lights.SetActive(true);
            }



                blinkDetected = false;

            // Example action:
            Camera.main.backgroundColor = new Color(Random.value, Random.value, Random.value);
        }
    }

    void OnApplicationQuit()
    {
        receiveThread.Abort();
        udpClient.Close();
    }
}
