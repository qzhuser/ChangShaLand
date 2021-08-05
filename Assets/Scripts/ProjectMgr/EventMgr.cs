using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEventInfo{}
public class EventInfo<T>:IEventInfo{
    public UnityAction<T> action1;
    public EventInfo(UnityAction<T> action){
        action1+=action;
    }
}
public class EventMgr : SingletonBase<EventMgr>
{
    private Dictionary<string ,IEventInfo> eventDic=new Dictionary<string, IEventInfo>();
  /// <summary>
  /// 添加事件监听
  /// </summary>
  /// <param name="name">事件名字</param>
  /// <param name="action">用来处理事件的委托</param>
  public void AddEventListener<T>(string name,UnityAction<T> action){
      if(eventDic.ContainsKey(name)){
          (eventDic[name] as EventInfo<T>).action1+=action;
      }
      else{
          eventDic.Add(name,new EventInfo<T>(action));
      }
  }
  /// <summary>
  /// 移除事件监听
  /// </summary>
  /// <param name="name"></param>
  /// <param name="action"></param>
  public void RemoveEventListener<T>(string name,UnityAction<T> action){
    if(eventDic.ContainsKey(name)){
        (eventDic[name] as EventInfo<T>).action1-=action;
    } 
  }
  /// <summary>
  /// 事件触发
  /// </summary> 
  /// <param name="name">事件名字</param>
  public void InvokeEvent<T>(string name,T obj){
        (eventDic[name] as EventInfo<T>).action1.Invoke(obj);
  }
  public void Clear(){
      eventDic.Clear();
  }
}
