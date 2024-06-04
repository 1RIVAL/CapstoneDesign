using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csCalender : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Calender;

    // Start is called before the first frame update
    void Start()
    {
        Button buttonCalender = GetComponent<Button>();
        buttonCalender.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnButtonClick()
    {
        Debug.Log("clicked");
        Calender.SetActive(true);
    }
}
