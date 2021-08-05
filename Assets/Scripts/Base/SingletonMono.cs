using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance;

    public static T GetInstance() { 
        if(instance==null){
            instance=GameObject.Find("ControlCenter").GetComponent<T>();
        }
        return instance;
    }
}
