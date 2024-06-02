using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class csOffSelectActivities : MonoBehaviour
{
    public Button back;
    public GameObject selectRewardActivity;
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
        selectRewardActivity.SetActive(false);
    }
}
