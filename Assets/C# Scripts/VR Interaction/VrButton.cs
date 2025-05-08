using UnityEngine.Events;
using UnityEngine;
using System;

public class VrButton : MonoBehaviour
{
    public Action OnClick;


    public void Click()
    {
        OnClick?.Invoke();
    }
}