using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoMgr : SingletonMonoAuto<MonoMgr>
{
    private event UnityAction updateEvent;

    public Coroutine StartMCoroutine(IEnumerator ienumerator){
        return StartCoroutine(ienumerator);
    }
    public Coroutine StopMCoroutine(IEnumerator ienumerator){
        return StopMCoroutine(ienumerator);
    }
    void Update(){
        if(updateEvent!=null){
            updateEvent();
        }
    } 
    public void AddUpdateListener(UnityAction action){
        updateEvent+=action;
    }
    public void RemoveUpdateListener(UnityAction action){
        updateEvent-=action;
    }
}
