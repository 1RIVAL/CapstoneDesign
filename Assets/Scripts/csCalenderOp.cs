using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class csCalenderOp : MonoBehaviour

    
{
    public Text monthText;
    // Start is called before the first frame update
    void Start()
    {
        DateTime today = DateTime.Today;

        int month = today.Month;
        monthText.text = month.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
