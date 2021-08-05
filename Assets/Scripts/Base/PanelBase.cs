using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelBase : MonoBehaviour
{
    private Dictionary<string,List<UIBehaviour>> dicComponents=new Dictionary<string, List<UIBehaviour>>();
    void Awake()
    {
        FindAllComponent<Button>();
        FindAllComponent<Image>();
    }
    protected T GetComponentByName<T>(string name)where T:UIBehaviour{
        if(dicComponents.ContainsKey(name)){
            for (var i = 0; i < dicComponents[name].Count; i++)
            {
                if(dicComponents[name][i] is T)
                    return dicComponents[name][i] as T;
            }
        }
        else
        {
            return null;
        }
        return null;
    }
    public virtual void ShowMe(){}
    public virtual void HideMe(){}

    private void FindAllComponent<T>()where T:UIBehaviour{
        T[] components=GetComponentsInChildren<T>();
        for (var i = 0; i < components.Length; i++)
        {
            if(dicComponents.ContainsKey(components[i].gameObject.name))
                dicComponents[components[i].gameObject.name].Add(components[i]);
            else
                dicComponents.Add(components[i].gameObject.name,new List<UIBehaviour>(){components[i]});
        }
    }
}
