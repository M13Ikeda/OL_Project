using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyInputManager : MonoBehaviour
{
    public List<KeyboardAction> keyActionList;

    [System.SerializableAttribute]
    public class KeyboardAction
    {
        public string name;
        public KeyCode keyCode;
        public string param;
        public UnityEvent onKey;
        public UnityEvent offKey;
        public KeyboardAction(string name, KeyCode keyCode, string param, UnityEvent onKey, UnityEvent offKey)
        {
            this.name = name;
            this.keyCode = keyCode;
            this.param = param;
            this.onKey = onKey;
            this.offKey = offKey;
        }
    }

    void Awake()
    {
    }

    void Update()
    {
        if (Input.anyKey)
        {
            OnInputKey();
        }
        ObserveAnyKeyUp();
    }

    private void OnInputKey()
    {
        var enumerator = keyActionList.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                var action = enumerator.Current;
                if (Input.GetKey(action.keyCode))
                {
                    //print("KeyInput: " + action.keyCode.ToString() + " / onKey.Invoke()");
                    action.onKey.Invoke();
                }
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    private void ObserveAnyKeyUp()
    {
        var enumerator = keyActionList.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                var action = enumerator.Current;
                if (Input.GetKeyUp(action.keyCode))
                {
                    if (0 == action.offKey.GetPersistentEventCount())
                    {
                        //print("KeyUp:" + action.keyCode.ToString() + " / No offKey");
                        return;
                    }
                    //print("KeyUp: " + action.keyCode.ToString() + " / offKey.Invoke()");
                    action.offKey.Invoke();
                }
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }
}

