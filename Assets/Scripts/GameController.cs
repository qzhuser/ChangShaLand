using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EventName{
    /// <summary>
    /// 移动到某个3D物体
    /// </summary>
    public const string Click3D="Move_3DTarget";
    /// <summary>
    /// 回到全局视角
    /// </summary>
    public const string BackAllView="Back_AllView";
    //public string 

}
public class GameController : SingletonMono<GameController>
{
    //public bool isPostScanCity = true;
    public Material normalMaterial,ImpMaterial;
    void Start()
    {
        Resmgr.GetInstance().LoadAsync<GameObject>("ChangSha",(obj)=>{
            ChangeAllMaterial(obj.transform.Find("building").gameObject, normalMaterial);
        });

        //给射线管理添加update
        MonoMgr.GetInstance().AddUpdateListener(RaycastManager.GetInstance().RayCastUpdate);

        //UIManager.GetInstance().ShowPanel<test>("Panel",UI_Layer.Left);
    }
    
    private void ChangeAllMaterial(GameObject gameObject, Material material){
        Renderer[]  renderers = gameObject.GetComponentsInChildren<Renderer>();
        //Debug.Log(renderers.Length);
        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials=new Material[]{material};
        }
    }
    void Destroy(){
        MonoMgr.GetInstance().RemoveUpdateListener(RaycastManager.GetInstance().RayCastUpdate);
    }

}
