using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastManager : SingletonBase<RaycastManager>
{
    RaycastHit hit;
    public void RayCastUpdate(){

        if(Input.GetMouseButtonDown(0)){
            Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray,out hit)){
                if(hit.collider.tag.Equals("PointCheck")&&CameraController._instance.isController){
                    Transform collider=hit.collider.transform;
                    EventMgr.GetInstance().InvokeEvent(EventName.Click3D,collider);
                    //CameraController._instance.MoveTarget(collider,1.5f);
                    MonoMgr.GetInstance().StartCoroutine(back(collider));
                }
            }
        }
        
    }
    private IEnumerator back(Transform collider){
        while (true)
        {
            yield return null;
            if(Input.GetMouseButtonDown(1)){
                yield return new WaitForSeconds(1f);
                while(true){
                    yield return null;
                    if(Input.GetMouseButtonDown(1)){
                        EventMgr.GetInstance().InvokeEvent<Transform>(EventName.BackAllView,collider);
                        yield break;
                    }
                }
            }
        }
    }
}
