using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoolData{
    public Transform fatherObj;
    public List<GameObject> objList;
    public PoolData(string name,Transform Pool){
        fatherObj=new GameObject(name).transform;
        fatherObj.parent=Pool;
        objList=new List<GameObject>();
    }

    public GameObject GetObject(string name){
        GameObject gameObject=null;

        gameObject=objList[0];
        objList.RemoveAt(0);
                                    
        gameObject.SetActive(true);
        gameObject.transform.parent=null;

        return gameObject;
    }

    public void PushObj(GameObject gameObject){
        objList.Add(gameObject);
        gameObject.transform.parent=fatherObj;
        gameObject.SetActive(false);
    }

}

/// <summary>
/// 缓存池
/// </summary>
public class PoolMgr : SingletonBase<PoolMgr>
{
    private Transform Pool;
     public Dictionary<string,PoolData> poolDic=new Dictionary<string, PoolData>();
    /// <summary>
    /// 拿出物体
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
     public void GetObject(string name,UnityAction<GameObject> action){
         if(poolDic.ContainsKey(name)&&poolDic[name].objList.Count>0){
             action(poolDic[name].GetObject(name));
         }else{
             Resmgr.GetInstance().LoadAsync<GameObject>(name,(obj)=>{
                 obj.name=name;
                 action(obj);
             });
         }
     }
     /// <summary>
     /// 回收物体
     /// </summary>
     /// <param name="name">路径名字</param>
     /// <param name="obj"></param>
    public void PushObj(string name,GameObject obj,UnityAction action){
        if(Pool==null)
            Pool=new GameObject("Pool").transform;
        //obj.transform.parent=Pool;
        
        obj.SetActive(false);
        if(!poolDic.ContainsKey(name))
            poolDic.Add(name,new PoolData(name,Pool));
        poolDic[name].PushObj(obj);
        action.Invoke();
    }
}
