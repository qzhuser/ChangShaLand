using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public struct DistanceSetting{
    public float Min;
    public float Max;
};
[System.Serializable]
public struct VerticalAngleSetting{
    public float Min;
    public float Max;
};
[System.Serializable]
public struct MoveScope{
    public float Width;
    public float Length;
};

public class CameraController : MonoBehaviour
{
    public static CameraController _instance;
    public Transform cameraTarget;
    public Transform center;
    //扭转
    public float angleSpeed;
    //缩放
    public float distanceSpeed;
    //平移
    public float moveSpeed;
    public DistanceSetting distance;
    public VerticalAngleSetting angle;
    public MoveScope moveScope;
    public Transform CameraPos;
    public float MoveTargetDuration=1f;
    public bool isController=true;
    private void Awake()
    {
        _instance = this;
    }

    void Start(){
        GetCameraTransform();
        EventMgr.GetInstance().AddEventListener<Transform>(EventName.Click3D,MoveTarget);
        EventMgr.GetInstance().AddEventListener<Transform>(EventName.BackAllView,BackPos);
    }
    // Update is called once per frame
    void Update()
    {
        CameraNormalCon();
    }
    void Destroy(){
        EventMgr.GetInstance().RemoveEventListener<Transform>(EventName.Click3D,MoveTarget);
        EventMgr.GetInstance().RemoveEventListener<Transform>(EventName.BackAllView,BackPos);
    }
    private void GetCameraTransform(){
        CameraPos.position=cameraTarget.position;
        CameraPos.eulerAngles=cameraTarget.eulerAngles;
        CameraPos.localScale=cameraTarget.localScale;       
    }
    private void CameraNormalCon(){
        //if(!isController)
         //   return;
        transform.LookAt(cameraTarget);
        if(Input.GetMouseButton(0)){
            cameraTarget.Rotate(0,Input.GetAxis("Mouse X")*angleSpeed*Time.deltaTime,0);
            cameraTarget.Rotate(-Input.GetAxis("Mouse Y")*angleSpeed*Time.deltaTime,0,0);
            cameraTarget.eulerAngles=new Vector3(Mathf.Clamp(cameraTarget.eulerAngles.x,angle.Min,angle.Max),cameraTarget.eulerAngles.y,0);
        }
        if(Input.GetMouseButton(1)){
            Vector3 forward=Vector3.Normalize(Vector3.Cross(Vector3.up,cameraTarget.transform.right));
            cameraTarget.transform.Translate(-Input.GetAxis("Mouse X")*moveSpeed*Time.deltaTime,0,0);
            cameraTarget.transform.position+=forward*Input.GetAxis("Mouse Y")*moveSpeed*Time.deltaTime;
        }
        cameraTarget.localScale+=new Vector3(0,0,Input.GetAxis("Mouse ScrollWheel")*distanceSpeed*Time.deltaTime);
        cameraTarget.localScale=new Vector3(1,1,Mathf.Clamp(cameraTarget.localScale.z,distance.Min,distance.Max));
        
        cameraTarget.transform.position=new Vector3(Mathf.Clamp(cameraTarget.transform.position.x,center.position.x-moveScope.Width/2,center.position.x+moveScope.Width/2), cameraTarget.transform.position.y, Mathf.Clamp(cameraTarget.transform.position.z,center.position.z-moveScope.Length/2,center.position.z+moveScope.Length/2));
    }
    /// <summary>
    /// 移动动画
    /// </summary>
    /// <param name="target"></param>
    /// <param name="duraion"></param>
    /// <param name="delay"></param>
    public void MoveTarget(Transform target){
        isController=false;
        GetCameraTransform();
        if (target.tag.Equals("PointCheck")) {
            target = target.GetChild(0);
        }
        cameraTarget.DOMove(target.position,MoveTargetDuration);
        Vector3 endEuler=new Vector3(25.0f,target.eulerAngles.y,target.eulerAngles.z);
        cameraTarget.DORotateQuaternion(Quaternion.Euler(endEuler),MoveTargetDuration);
        cameraTarget.DOScaleZ(0.3f,MoveTargetDuration);
    }

    public void BackPos(Transform obj) {
        StartCoroutine(BackPosAni(obj,2.0f));
    }
    /// <summary>
    /// 回到全局视角
    /// </summary>
    private IEnumerator BackPosAni(Transform obj,float delay){
        isController=true;
        yield return new WaitForSeconds(delay);
        cameraTarget.DOMove(CameraPos.position,1f);
        cameraTarget.DORotateQuaternion(Quaternion.Euler(CameraPos.eulerAngles),1f);
        cameraTarget.DOScaleZ(CameraPos.localScale.z,1f);
    }
    void OnDrawGizmos(){
        Gizmos.color=Color.yellow;
        Gizmos.DrawWireCube(center.position,new Vector3(moveScope.Width,0.5f,moveScope.Length));
        Gizmos.color=Color.green;
        Gizmos.DrawLine(transform.position,cameraTarget.position);
    }
}
