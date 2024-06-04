using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csOffCalender : MonoBehaviour
{
    public Button back;
    public GameObject calender;
    // Start is called before the first frame update
    void Start()
    {
        back.onClick.AddListener(OnButtonClick);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnButtonClick()
    {
        Debug.Log("go back");
        calender.SetActive(false);
    }
}
