using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
[SerializeField]
public struct ParticalPos{
    public Vector3 startPos;
    public Vector3 pos;
    public float lifetime;
}

public class ParticalController : MonoBehaviour
{
    public Texture texture;
    public ComputeShader cs;
    public float _NoiseFrequency;
    [Range(0,1)]
    public float _NoiseAbs;
    public float _NoisePower;
    public Vector2 _ParticalSpeed;
    public Vector3 simulationSpace;
    [Range(0,1)]
    public float cellSize;
    public GameObject trail;
    public float distance;
    private int Kneral;
    private ComputeBuffer computeBuffer;
    public Vector3 noiseSpeed;
    public float MinLifeTime,MaxLifeTime;
    public Material material;
    /// <summary>
    /// 拖尾
    /// </summary>
    /// <typeparam name="GameObject"></typeparam>
    /// <returns></returns>
    private List<GameObject> trailList=new List<GameObject>();
    /// <summary>
    ///  要更新的拖尾属性
    /// </summary>
     private ParticalPos[] listpos;
    // Start is called before the first frame update
    void Start()
    {
        material.SetTexture("_MainTex",texture);
        material.SetFloat("_SpaceX",simulationSpace.x);
       for (var i = -simulationSpace.x/2; i <= simulationSpace.x/2; i+=cellSize)
       {
           GameObject gameObject=Instantiate(trail,transform);
           gameObject.transform.localPosition=new Vector3(i,0,0);
            gameObject.GetComponent<TrailRenderer>().time = Random.Range(MinLifeTime, MaxLifeTime);
           trailList.Add(gameObject);
       }
       listpos=new ParticalPos[trailList.Count];
       for (var i = 0; i < listpos.Length; i++)
       {
           listpos[i].startPos = trailList[i].transform.localPosition;
           listpos[i].pos=trailList[i].transform.position;
           //listpos[i].noiseSpeed=new Vector3(0,0,1.0f);
           listpos[i].lifetime = trailList[i].GetComponent<TrailRenderer>().time;
       }
        Kneral=cs.FindKernel("MyCompute");
        computeBuffer=new ComputeBuffer(trailList.Count,Marshal.SizeOf(typeof(ParticalPos)));
        computeBuffer.SetData(listpos);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position+=new Vector3(0,0,_ParticalSpeed);
        //for (var i = 0; i < listpos.Length; i++)
        //{
        //    listpos[i].pos = trailList[i].transform.position;
        //    listpos[i].noiseSpeed = new Vector3(0, 0, 1.0f);
        //}
        //computeBuffer.SetData(listpos);
        
        cs.SetInt("trailCount",trailList.Count);
        cs.SetFloat("_NoiseFrequency",_NoiseFrequency);
        cs.SetFloat("_NoiseAbs",_NoiseAbs);
        cs.SetFloat("_NoisePower",_NoisePower);
        cs.SetFloats("_ParticalSpeed", new float[] { _ParticalSpeed.x, _ParticalSpeed.y });
        cs.SetFloat("time",Time.time);
        cs.SetFloat("deltaTime",Time.deltaTime);
        cs.SetBuffer(Kneral,"Result",computeBuffer);
        cs.SetFloats("noiseSpeed",new float[] { noiseSpeed.x,noiseSpeed.y,noiseSpeed.z});

        cs.Dispatch(Kneral, trailList.Count, 1,1);
        computeBuffer.GetData(listpos);
        for (var i = 0; i < trailList.Count; i++)
        {
            if (listpos[i].lifetime <= 0)
            {
                desParticle(i);
            }
            else
            {
                trailList[i].transform.position = listpos[i].pos;
                trailList[i].GetComponent<TrailRenderer>().time = listpos[i].lifetime;
            }
        }
    }
    void InsParticle(int i) {
        GameObject gameObject = Instantiate(trail, transform);
        gameObject.transform.localPosition = listpos[i].startPos;
        trailList[i]=gameObject;
        listpos[i].pos= gameObject.transform.position;
        listpos[i].lifetime = Random.Range(MinLifeTime, MaxLifeTime);
        trailList[i].GetComponent<TrailRenderer>().time = listpos[i].lifetime;
        computeBuffer.SetData(listpos);
    }
    void desParticle(int i) {
        DestroyImmediate(trailList[i],false);
        InsParticle(i);
    }

}
