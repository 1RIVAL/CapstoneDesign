using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PoseTracking : MonoBehaviour
{
    public UDPReceive udpReceive;
    public GameObject[] handPoints;
  

    public Button startButton;
    public Text startButtonText;

    // 추가
    public float t = 0.1f;
    public float waitStartTime = 2.75f;

    private bool isHandDetectorTransmitting = false;
    // 추가

    void Start()
    {
        //StartCoroutine(WaitForPythonLoading());
    }

    void Update()
    {
        if (!isHandDetectorTransmitting) return; // 추가

        // UDP 프로토콜로 전송된 데이터를 받아온다.
        string data = udpReceive.data;

        // 가지고 온 데이터에서 대괄호([ ])를 뺀다.
        if (data != null)
        {
            data = data.Remove(0, 1);
            data = data.Remove(data.Length - 1, 1);
        }

        //print(data);
        // 쉼표를 기준으로 데이터를 분활
        string[] points = data.Split(',');
        //print(points[0]);

        for (int i = 0; i < 21; i++)
        {
            float x = 6.5f - float.Parse(points[i * 3]) / 90;
            float y = float.Parse(points[i * 3 + 1]) / 100;
            float z = float.Parse(points[i * 3 + 2]) / 100;



            // instant movement
            handPoints[i].transform.localPosition = new Vector3(-x, -z, y);

        }
    }

    //private IEnumerator WaitForPythonLoading()
    //{
    //    string data;

    //    do
    //    {
    //        data = udpReceive.data;
    //        yield return null;
    //    }
    //    while (!data.Equals("True"));

    //    yield return new WaitForSecondsRealtime(waitStartTime);

    //    startButton.interactable = true;
    //    startButtonText.text = "Play!";

    //    isHandDetectorTransmitting = true;
    //    //StartCoroutine(GetHandData());
    //}

    //private ienumerator gethanddata()
    //{
    //    while (true)
    //    {
    //        string data = udpreceive.data;
    //        if (data != null)
    //        {
    //            data = data.remove(0, 1);
    //            data = data.remove(data.length - 1, 1);
    //        }
    //        //print(data);
    //        string[] points = data.split(',');
    //        //print(points[0]);

    //        for (int i = 0; i < 21; i++)
    //        {
    //            float x = 6.5f - float.parse(points[i * 3]) / 90;
    //            float y = float.parse(points[i * 3 + 1]) / 100;
    //            float z = float.parse(points[i * 3 + 2]) / 100;



    //            // instant movement
    //            handpoints[i].transform.localposition = new vector3(-x, -z, y);

    //        }

    //        yield return null;
    //    }
    //}
}
