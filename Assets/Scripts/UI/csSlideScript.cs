using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csSlideScript : MonoBehaviour
{
    public Slider slider;
    int temperature;
    int temperatureMax;

    void Awake()
    {
        temperature = 5;
        temperatureMax = 10;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = (float)temperature / temperatureMax;
    }
}
