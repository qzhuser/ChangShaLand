using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public enum UI_Layer{
    Left,
    Right,
    Mid,
}
public class UIManager : SingletonBase<UIManager>
{
    private Transform left;
    private Transform right;
    private Transform mid;
    public Dictionary<string,PanelBase> panelDic=new Dictionary<string, PanelBase>();
    /// <summary>
    /// 构造方法 在实例化时被调用
    /// </summary>
    public UIManager(){
        GameObject gameObject=Resmgr.GetInstance().Load<GameObject>("UI/Canvas");
        Transform canvas=gameObject.transform;

        left=canvas.Find("Left");
        right=canvas.Find("Right");
        mid=canvas.Find("Mid");

    }

    /// <summary>
    /// 显示panel
    /// </summary>
    /// <param name="panelName"></param>
    /// <param name="layer"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public void ShowPanel<T>(string panelName,UI_Layer layer,UnityAction<T> action=null)where T:PanelBase{
        if(panelDic.ContainsKey(panelName))
            return;

        Resmgr.GetInstance().LoadAsync<GameObject>("UI/"+panelName,(obj)=>{
            Transform father=left;
            switch (layer)
            {
                case UI_Layer.Right:
                    father=right;
                    break;
                case UI_Layer.Mid:
                    father=mid;
                    break;
            }
            obj.transform.SetParent(father);

            obj.transform.localPosition=Vector3.zero;
            obj.transform.localScale=Vector3.one;

            (obj.transform as RectTransform).offsetMax=Vector2.zero;
            (obj.transform as RectTransform).offsetMin=Vector2.zero;
            obj.name=panelName;

            T panel=obj.GetComponent<T>();
            //重新设置布局或初始化数据
            panel.ShowMe();

            if(action!=null)
                action.Invoke(panel);
            
            panelDic.Add(panelName,panel);
        });
    }

    public void HidePanel(string panelName){
        if(panelDic.ContainsKey(panelName)){
            panelDic[panelName].HideMe();
            GameObject.Destroy(panelDic[panelName].gameObject);
            panelDic.Remove(panelName);
        }
    }

    public static void AddCustomEventListener(UIBehaviour control,EventTriggerType type,UnityAction<BaseEventData> action){
        EventTrigger trigger=control.gameObject.GetComponent<EventTrigger>();
        if(trigger==null){
            trigger=control.gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry=new EventTrigger.Entry();
        entry.eventID=type;
        entry.callback.AddListener(action);

        trigger.triggers.Add(entry);
    }
}
