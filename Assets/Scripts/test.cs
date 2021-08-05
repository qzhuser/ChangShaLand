using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;
public class test : PanelBase
{
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        button=GetComponentByName<Button>("Btn");
        button.onClick.AddListener(click);
        UIManager.AddCustomEventListener(button,EventTriggerType.PointerEnter,(data)=>{Debug.Log("鼠标进入");});
        UIManager.AddCustomEventListener(button,EventTriggerType.PointerExit,(data)=>{Debug.Log("鼠标离开");});
    }
    void click(){
        UIManager.GetInstance().HidePanel(transform.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
