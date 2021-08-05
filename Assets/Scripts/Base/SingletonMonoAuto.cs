using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoAuto<T> : MonoBehaviour where T:MonoBehaviour
{
    private static T instance;

    public static T GetInstance() { 
        if(instance==null){
            GameObject gameObject=new GameObject();
            gameObject.name=typeof(T).ToString();
            instance=gameObject.AddComponent<T>();
        }
        return instance;
    }
}
