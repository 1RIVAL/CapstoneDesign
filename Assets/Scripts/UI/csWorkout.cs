using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csWorkout : MonoBehaviour
{
    public GameObject playWorkout;
    public UDPTCPReceive udptcpReceive;

    // Start is called before the first frame update
    void Start()
    {
        playWorkout.SetActive(false);

        Button buttonWorkout = GetComponent<Button>();
        buttonWorkout.onClick.AddListener(OnButtonClick);

        if (udptcpReceive != null) 
        {
            udptcpReceive.playWorkout = playWorkout;
        }

        else
        {
            Debug.LogError("TCPReceive 인스턴스가 설정 X");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnButtonClick()
    {
        Debug.Log("clicked");
        
        playWorkout.SetActive(true);

        Transform parent = transform.parent;
        if (parent != null) 
        {
            foreach (Transform child in parent)
            {
                if (child.gameObject != playWorkout)
                {
                    child.gameObject.SetActive(false);
                }
            }
            parent.gameObject.SetActive(true);
        }
        
    }
}
