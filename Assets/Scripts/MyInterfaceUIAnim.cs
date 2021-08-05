using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MyInterfaceUIAnim : InterfaceAnimManager
{

    /// <summary>
    /// 显示3DUI
    /// </summary>
    /// <param name="delay"></param>
    /// <param name="action"></param>
    public void Open3DUI(float delay,UnityAction action=null) {
        transform.gameObject.SetActive(true);
        StartCoroutine(Open3DUIAni(delay));
    }
    public void Close3DUI(float delay, UnityAction action)
    {
        StartCoroutine(Close3DUIAni(delay, action));
    }

    private IEnumerator Open3DUIAni(float delay) {
        yield return new WaitForSeconds(delay);
        startAppear();
    }
    private IEnumerator Close3DUIAni(float delay, UnityAction action) {
        startDisappear();
        yield return new WaitForSeconds(2.5f);
        action.Invoke();
    }

    public override void startAppear(bool direct = false)
    {
        base.startAppear(direct);
    }
    public override void startDisappear(bool direct = false)
    {
        base.startDisappear(direct);
    }
}
