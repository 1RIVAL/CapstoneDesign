using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseTracking : MonoBehaviour
{
    public UDPTCPReceive udptcpReceive;
    public GameObject[] posePoints;
  
    void Start()
    {

    }

    void Update()
    {
        // UDP 프로토콜로 전송된 데이터를 받아온다.
        string data = udptcpReceive.data;

        // 가지고 온 데이터에서 대괄호([ ])를 뺀다.
        data = data.Remove(0, 1);
        data = data.Remove(data.Length - 1, 1);

        // 쉼표를 기준으로 데이터를 분활
        string[] points = data.Split(',');

        for (int i = 0; i < 21; i++)
        {
            float x = 5 - float.Parse(points[i * 3]) / 100;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = float.Parse(points[i * 3 + 2]) / 100;

            posePoints[i].transform.localPosition = new Vector3(x, y, z);

        }
    }

}
