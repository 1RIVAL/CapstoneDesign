using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class csSlideScript : MonoBehaviour
{
    public Slider slider; // 슬라이더
    public UDPTCPReceive udptcpReceive; 
    public Button back; 
    public GameObject popcorn;
    public GameObject corn;
    public bool good = false; // 성공 유무 변수
    public int aimCounter; // 목표 횟수 변수

    //int temperature;
    //int temperatureMax;


    void Awake()
    {
        //temperature = 5;
        //temperatureMax = 10;

        aimCounter = 5;
    }

    // Start is called before the first frame update
    void Start()
    {
        back.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        

        //slider.value = (float)temperature / temperatureMax;

    }

    void OnButtonClick()
    {
        Debug.Log("go back");
        // 목표 달성 X
        if (!good && udptcpReceive.counter < aimCounter)
        {
            slider.value += slider.maxValue / aimCounter;
            if (slider.value >= slider.maxValue)
            {
                popcorn.SetActive(true);
                corn.SetActive(false);
            }
            
        }

        // 목표 달성 O
        else if (!good && udptcpReceive.counter >= aimCounter)
        {
           
            slider.value -= slider.maxValue / aimCounter;
            if (slider.value <= slider.minValue)
            {
                good = true;
            }

        }
        
    }
}
