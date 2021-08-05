using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class ParticleController : SingletonMono<ParticleController>
{
    public Transform RoadsTrans;
    public Transform PointsTrans;
    /// <summary>
    /// 路网粒子父对象
    /// </summary>
    private Transform RoadPaths;
    /// <summary>
    /// 点位离线粒子父对象
    /// </summary>
    private Transform PointPaths;
    /// <summary>
    /// 路网例子速度
    /// </summary>
    public float roadSpeed;
    public float pointSpeed;
    private GameObject trailLine,trailBezier;
    void Start(){
        trailLine=Resources.Load<GameObject>("Particles/ParticleLinePath");
        trailBezier = Resources.Load<GameObject>("Particles/Trail");

        InsRoadParticle();
        InsPointPath();

        EventMgr.GetInstance().AddEventListener<Transform>(EventName.Click3D,ClickPoint);
        EventMgr.GetInstance().AddEventListener<Transform>(EventName.BackAllView, Back);
    }
    private void OnDestroy()
    {
        EventMgr.GetInstance().RemoveEventListener<Transform>(EventName.Click3D, ClickPoint);
        EventMgr.GetInstance().RemoveEventListener<Transform>(EventName.BackAllView, Back);
    }
    /// <summary>
    /// 生成路网
    /// </summary>
    private void InsRoadParticle(){
        RoadPaths=new GameObject().transform;
        RoadPaths.name="RoadPaths";
        ParticleSystem trailObj;
        for (var k = 0; k < RoadsTrans.childCount; k++)
        {
            trailObj=Instantiate(trailLine,RoadPaths).GetComponent<ParticleSystem>();
            trailObj.GetComponent<ParticlePath>().IsBezier=false;
            trailObj.transform.position=RoadsTrans.GetChild(k).GetChild(0).position;
            List<Vector3> waylist=new List<Vector3>();
            for (var i = 0; i < RoadsTrans.GetChild(k).childCount; i++)
            {
                waylist.Add(RoadsTrans.GetChild(k).GetChild(i).position);
            }
            trailObj.GetComponent<ParticlePath>().Waypoints=waylist;
            trailObj.GetComponent<ParticlePath>().Speed=roadSpeed;
            trailObj.name=RoadsTrans.GetChild(k).name+"_Trail";
        }
        
   }
    /// <summary>
    /// 生成点位轨迹
    /// </summary>
    private void InsPointPath(){
       PointPaths=new GameObject().transform;
       PointPaths.name="PointsTrans";
        
       for (var i = 0; i < PointsTrans.childCount; i++)
       {
            GameObject trail = Instantiate(trailBezier,PointPaths);
            trail.name += "Trail";
            List<Vector3> waylist=new List<Vector3>();
            waylist.Add(PointsTrans.GetChild(i).position);
            Vector3 endPos=PointsTrans.GetChild(Random.Range(0,PointsTrans.childCount)).position;
            waylist.Add((PointsTrans.GetChild(i).position+endPos)/2+new Vector3(0,10,0));
            waylist.Add(endPos);
            DrawBezierLine(trail.GetComponent<TrailRenderer>(),waylist,20);
       }

   }
    private void DrawBezierLine(TrailRenderer trail,List<Vector3> points, int smooth)
    {
        List<Vector3> list = CalculateCurve(points, smooth, false);
        float time = Random.Range(2f,4f);
        StartCoroutine(DrawBezierLineAnim(trail,list,0.1f,time));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="points">点位坐标</param>
    /// <param name="frameTime">每两个点位的时间</param>
    /// <param name="time">trail存在时间</param>
    /// <returns></returns>
    private IEnumerator DrawBezierLineAnim(TrailRenderer trail ,List<Vector3> points,float frameTime,float time)
    {
        trail.transform.position = points[0];
        while (true)
        {
            trail.gameObject.SetActive(true);
            trail.time = time;
            for (int i = 0; i < points.Count; i++)
            {
                yield return new WaitForSeconds(frameTime);
                trail.transform.DOMove(points[i], frameTime);
            }
            yield return new WaitForSeconds(frameTime*points.Count);

            trail.time = 0;
            trail.gameObject.SetActive(false);
            trail.transform.position = points[0];
            yield return new WaitForSeconds(time);

        }
        
    }

    /// <summary>
    /// Calculate Catmul-Rom Curve（平滑的线）在点位间平滑插值，生成连续多个平滑点位
    /// </summary>
    /// <param name="points">key points</param>
    /// <param name="smooth">how many segments between two nearby point</param>
    /// <param name="curveClose">whether curve is a circle</param>
    /// <returns></returns>
    private List<Vector3> CalculateCurve(IList<Vector3> points, int smooth, bool curveClose)
    {
        int pointCount = points.Count;
        int segmentCount = curveClose ? pointCount : pointCount - 1;
        List<Vector3> allVertices = new List<Vector3>((smooth + 1) * segmentCount);
        Vector3[] tempVertices = new Vector3[smooth + 1];
        float smoothReciprocal = 1f / smooth;
        for (int i = 0; i < segmentCount; ++i)
        {
            // get 4 adjacent point in points to calculate position between p1 and p2
            Vector3 p0, p1, p2, p3;
            p1 = points[i];
            if (curveClose)
            {
                p0 = i == 0 ? points[segmentCount - 1] : points[i - 1];
                p2 = i + 1 < pointCount ? points[i + 1] : points[i + 1 - pointCount];
                p3 = i + 2 < pointCount ? points[i + 2] : points[i + 2 - pointCount];
            }
            else
            {
                p0 = i == 0 ? p1 : points[i - 1];
                p2 = points[i + 1];
                p3 = i == segmentCount - 1 ? p2 : points[i + 2];
            }
            Vector3 pA = p1;
            Vector3 pB = 0.5f * (-p0 + p2);
            Vector3 pC = p0 - 2.5f * p1 + 2f * p2 - 0.5f * p3;
            Vector3 pD = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);
            float t = 0;
            for (int j = 0; j <= smooth; j++)
            {
                tempVertices[j] = pA + t * (pB + t * (pC + t * pD));
                t += smoothReciprocal;
            }
            for (int j = allVertices.Count == 0 ? 0 : 1; j < tempVertices.Length; j++)
            {
                allVertices.Add(tempVertices[j]);
            }
        }
        return allVertices;
    }

    private void ClickPoint(Transform transform)
    {

        //HideIcon(transform.GetChild(0),1f,()=> {
        //    ShowPartical(transform.GetChild(1).GetComponent<ParticleSystem>());

        //    transform.GetChild(2).GetComponent<MyInterfaceUIAnim>().Open3DUI(0.5f,()=> {
        //        transform.GetChild(2).gameObject.SetActive(true);
        //    });
        //});
        Transform target = transform.GetChild(0).GetChild(1);
        target.GetComponent<MyInterfaceUIAnim>().Open3DUI(0.5f, () =>
        {
            target.gameObject.SetActive(true);
        });
    }
    private void Back(Transform transform)
    {
        Transform target = transform.GetChild(0).GetChild(1);
        //transform.GetChild(2).GetComponent<MyInterfaceUIAnim>().Close3DUI(1.5f, () => {
        //    transform.GetChild(2).gameObject.SetActive(false);
        //    ShowIcon(transform.GetChild(0), Vector3.one * 3, 1f);
        //    HidePartical(transform.GetChild(1).GetComponent<ParticleSystem>());
        //});
        target.GetComponent<MyInterfaceUIAnim>().Close3DUI(1.5f, () => {
            target.gameObject.SetActive(false);
        });

    }

    /// <summary>
    /// 隐藏点位图标
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
        private void HideIcon(Transform transform,float duration, UnityAction action,float delay=0f){
        StartCoroutine(HideIconAnim(transform,delay,duration,action));
    }
    private IEnumerator HideIconAnim(Transform transform,float delay,float duration,UnityAction action){
        yield return new WaitForSeconds(delay);
        transform.DOScale(Vector3.zero, duration).OnComplete(()=> { action.Invoke(); });
        
    }
    private void ShowIcon(Transform transform,Vector3 endValue,float duration,float delay=0f){
        StartCoroutine(ShowIconAnim(transform,endValue,delay,duration));
    }
    private IEnumerator ShowIconAnim(Transform transform,Vector3 endValue, float delay,float duration){
        yield return new WaitForSeconds(delay);
        transform.DOScale(endValue,duration);
    }
    /// <summary>
    /// 显示粒子
    /// </summary>
    /// <param name="particleSystem"></param>
    private void ShowPartical(ParticleSystem particleSystem){
        particleSystem.gameObject.SetActive(true);
        particleSystem.Play();
        if(particleSystem.transform.childCount>0){
            ParticleSystem[] ps=particleSystem.GetComponentsInChildren<ParticleSystem>();
            for (var i = 0; i < ps.Length; i++)
            {
                ps[i].Play();
            }
        }

    }
    private void HidePartical(ParticleSystem particleSystem){
        particleSystem.Stop();
        if(particleSystem.transform.childCount>0){
            ParticleSystem[] ps=particleSystem.GetComponentsInChildren<ParticleSystem>();
            for (var i = 0; i < ps.Length; i++)
            {
                ps[i].Stop();
            }
        }
        particleSystem.gameObject.SetActive(false);
    }

}
