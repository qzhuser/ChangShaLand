using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Resmgr : SingletonBase<Resmgr>
{
    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Load<T>(string name) where T:Object{
        T res=Resources.Load<T>(name);
        if(res is GameObject){
            return GameObject.Instantiate(res);
        }else
            return res;
    }

    public void LoadAsync<T>(string name,UnityAction<T> action) where T:Object{
        MonoMgr.GetInstance().StartMCoroutine(LoadAsyncIenu<T>(name,action));
    }
    private IEnumerator LoadAsyncIenu<T>(string name,UnityAction<T> action )where T:Object{
        ResourceRequest obj=Resources.LoadAsync<T>(name);
        yield return obj;
        if(obj.asset is GameObject){
            action.Invoke(GameObject.Instantiate(obj.asset) as T);
        }else
            action.Invoke(obj.asset as T);
    }
}
