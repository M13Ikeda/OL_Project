using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FacialMorphing : MonoBehaviour
{
    //Face
    public string TargetGameObjectName_Face;
    public string TargetGameObjectName_Brow;
    public string TargetGameObjectName_L_Eye;
    public string TargetGameObjectName_R_Eye;
    public string TargetGameObjectName_Accent;

    private SkinnedMeshRenderer FACIAL_Face;
    private SkinnedMeshRenderer FACIAL_Brow;
    private SkinnedMeshRenderer FACIAL_L_Eye;
    private SkinnedMeshRenderer FACIAL_R_Eye;
    private SkinnedMeshRenderer FACIAL_Accent;

    private Mesh MESH_Face;
    private Mesh MESH_Brow;
    private Mesh MESH_L_Eye;
    private Mesh MESH_R_Eye;
    private Mesh MESH_Accent;

    //表示変化の数値
    public float BlendingPower = 30f;

    //Keyboard操作の有効・無効
    public bool enableKeybord = false;

    [System.NonSerialized] public float maxValue = 100f;
    [System.NonSerialized] public MorphingState morphingState;
    [System.NonSerialized] public KeyCode holdingInputKey = KeyCode.None;
    [System.NonSerialized] public string holdingBlendShape = "";    //[WIP]Paozゲームで使用パラメータのため一旦据え置き

    private bool isEnableBlink;
    public bool IsEnableBlink
    {
        get
        {
            return isEnableBlink;
        }
    }

    private UnityEvent KeyUp_event;
    public List<ExpressionList> expressionList;

    public enum MorphingState
    {
        off,
        changing,
        on
    };

    [Serializable] public class OneFaceMeshParam_Expression : UnityEvent<float, string> { }
    [Serializable] public class OneFaceMeshParamList_Expression : UnityEvent<float, List<string>> { }
    [Serializable] public class FacialExpression : UnityEvent<FacialList> { }

    [System.SerializableAttribute]
    public class FacialList
    {
        public List<string> Face;
        public List<string> Eyebrow;
        public List<string> L_Eye;
        public List<string> R_Eye;
        public List<string> Accent;
        public FacialList(List<string> Face, List<string> Eyebrow, List<string> L_Eye, List<string> R_Eye, List<string> Accent)
        {
            this.Face = Face;
            this.Eyebrow = Eyebrow;
            this.L_Eye = L_Eye;
            this.R_Eye = R_Eye;
            this.Accent = Accent;
        }
    }

    [System.SerializableAttribute]
    public class ExpressionList
    {
        public string name;
        public FacialList facial;
        public bool EnableAutoEyeBlink = true;
        public int BlendingBlink = 100;
        public KeyCode key;
        public FacialExpression OnChangeFacial;

        public ExpressionList(string name, FacialList facial, KeyCode key, bool EnableAutoEyeBlink, int BlendingBlink, FacialExpression OnChangeFacial)
        {
            this.name = name;
            this.facial = facial;
            this.key = key;
            this.EnableAutoEyeBlink = EnableAutoEyeBlink;
            this.BlendingBlink = BlendingBlink;
            this.OnChangeFacial = OnChangeFacial;
        }
    }

    private void InitialFace()
    {
        isEnableBlink = true;
        FACIAL_Face = SkinnedMeshObject(TargetGameObjectName_Face);
        FACIAL_Brow = SkinnedMeshObject(TargetGameObjectName_Brow);
        FACIAL_L_Eye = SkinnedMeshObject(TargetGameObjectName_L_Eye);
        FACIAL_R_Eye = SkinnedMeshObject(TargetGameObjectName_R_Eye);
        FACIAL_Accent = SkinnedMeshObject(TargetGameObjectName_Accent);
        MESH_Face = MeshObject(FACIAL_Face);
        MESH_Brow = MeshObject(FACIAL_Brow);
        MESH_L_Eye = MeshObject(FACIAL_L_Eye);
        MESH_R_Eye = MeshObject(FACIAL_R_Eye);
        MESH_Accent = MeshObject(FACIAL_Accent);
    }

    void Start()
    {
        InitialFace();

        if (KeyUp_event == null)
        {
            KeyUp_event = new UnityEvent();
        }
    }

    void Update()
    {
        if (!enableKeybord)
            return;

        if (Input.anyKey)
        {
            ExpressionByKeyInputDown();
            return;
        }
        CheckKeyInputUp();
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /// <summary>
    /// Index指定での表情切替（外部呼び出し向け）
    /// <summary>
    public void FaceExpressionOn(int index)
    {
        if (index > expressionList.Count - 1)
        {
            print("not found index");
            return;
        }
        var expression = expressionList[index];
        if (holdingInputKey == expression.key || holdingInputKey == KeyCode.None)
        {
            holdingInputKey = expression.key;
            holdingBlendShape = expression.facial.Face[0];  
            isEnableBlink = expression.EnableAutoEyeBlink;
            expression.OnChangeFacial.Invoke(expression.facial);
        }
    }
    /// <summary>
    /// 表情を戻す処理
    /// </summary>
    public void FaceExpressionOff()
    {
        CallEvent();
    }
    private void CallEvent()
    {
        if (KeyUp_event != null)
        {
            KeyUp_event.Invoke();
        }
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /// <summary>
    // 複数Mesh・BlendShapesパラメータの制御
    /// <summary>
    public void OnFacialAction(FacialList facialList)
    {
        if (morphingState == MorphingState.on)
        {
            return;
        }

        if (KeyUp_event == null || KeyUp_event.GetPersistentEventCount() == 0)
        {
            SetFacialKeyUpEvent(facialList.Face, FACIAL_Face);
            SetFacialKeyUpEvent(facialList.Eyebrow, FACIAL_Brow);
            SetFacialKeyUpEvent(facialList.L_Eye, FACIAL_L_Eye);
            SetFacialKeyUpEvent(facialList.R_Eye, FACIAL_R_Eye);
            SetFacialKeyUpEvent(facialList.Accent, FACIAL_Accent);
        }

        //※現状、表情パラメータ値の管理は個別でなく、facialListのFaceの[0]の値をマスターにしています※
        float value = 0;
        if (facialList.Face.Count > 0 && FACIAL_Face)
        {
            value = FACIAL_Face.GetBlendShapeWeight(MESH_Face.GetBlendShapeIndex(facialList.Face[0]));
        }

        if (value <= 100.0f)
        {
            UpdateMultiBlendShape(facialList.Face, FACIAL_Face, MESH_Face);
            UpdateMultiBlendShape(facialList.Eyebrow, FACIAL_Brow, MESH_Brow);
            UpdateMultiBlendShape(facialList.L_Eye, FACIAL_L_Eye, MESH_L_Eye);
            UpdateMultiBlendShape(facialList.R_Eye, FACIAL_R_Eye, MESH_R_Eye);
            UpdateMultiBlendShape(facialList.Accent, FACIAL_Accent, MESH_Accent);

            morphingState = MorphingState.changing;
            return;
        }
        morphingState = MorphingState.on;
    }

    public void OffFaceAction(string meshName, SkinnedMeshRenderer FACIAL)
    {
        StartCoroutine(OffFaceActionCoroutine(meshName, FACIAL));
    }

    IEnumerator OffFaceActionCoroutine(string meshName, SkinnedMeshRenderer FACIAL)
    {
        var MESH = FACIAL.sharedMesh;
        var value = FACIAL.GetBlendShapeWeight(MESH.GetBlendShapeIndex(meshName));
        for (int i = (int)value; i >= 0; i -= (int)(BlendingPower * Time.deltaTime * 20f))
        {
            FACIAL.SetBlendShapeWeight(MESH.GetBlendShapeIndex(meshName), Mathf.Clamp(i, 0, value));
            morphingState = MorphingState.changing;
            yield return null;
        }
        //端数処理
        FACIAL.SetBlendShapeWeight(MESH.GetBlendShapeIndex(meshName), Mathf.Clamp(0, 0, value));

        KeyUp_event.RemoveAllListeners();
        holdingInputKey = KeyCode.None;
        isEnableBlink = true;
        holdingBlendShape = ""; 
        morphingState = MorphingState.off;
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private void SetFacialKeyUpEvent(List<string> meshList, SkinnedMeshRenderer FACIAL)
    {
        if (meshList.Count > 0 && FACIAL)
        {
            foreach (var meshName in meshList)
            {
                KeyUp_event.AddListener(() => OffFaceAction(meshName, FACIAL));
            }
        }
    }

    private void UpdateMultiBlendShape(List<string> blendShapesPramList, SkinnedMeshRenderer FACIAL, Mesh MESH)
    {
        if (!(blendShapesPramList.Count > 0) || !(FACIAL) || !(MESH))
        {
            return;
        }

        var meshName = blendShapesPramList.GetEnumerator();
        try
        {
            while (meshName.MoveNext())
            {
                UpdateBlendShape(FACIAL, MESH.GetBlendShapeIndex(meshName.Current), BlendingPower, maxValue);
            }
        }
        finally
        {
            meshName.Dispose();
        }
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    /// <summary>
    /// Update BlendShape
    /// </summary>
    /// <param name="skinnedMeshRenderer">Skinned mesh renderer.</param>
    /// <param name="blendShapeIndex">Blend shape index.</param>
    /// <param name="amount">Amount.</param>
    /// <param name="maxValue">Max Value.</param>
    /// <param name="forceSet">If set to <c>true</c> force set.</param>
    private void UpdateBlendShape(SkinnedMeshRenderer skinnedMeshRenderer, int blendShapeIndex, float amount, float maxValue, bool forceSet = false)
    {
        if (!forceSet)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(
                    blendShapeIndex,
                    Mathf.Clamp(
                        skinnedMeshRenderer.GetBlendShapeWeight(blendShapeIndex) + amount * Time.deltaTime * 10f,
                        0,
                        maxValue
                    )
                );
        }
        else
        {
            skinnedMeshRenderer.SetBlendShapeWeight(
                    blendShapeIndex,
                    amount
                );
        }
    }

    /// <summary>
    /// 名前指定でTransform取得
    /// </summary>
    /// <returns>The target.</returns>
    /// <param name="name">Name.</param>
    public Transform FindTarget(string name)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Equals(name))
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// 名前指定でSkinnedMeshRenderer取得
    /// </summary>
    /// <returns>The target.</returns>
    /// <param name="TargetGameObjectName">TargetGameObjectName.</param>
    public SkinnedMeshRenderer SkinnedMeshObject(string TargetGameObjectName)
    {
        if (TargetGameObjectName != null && TargetGameObjectName.Length > 0)
        {
            Transform target = FindTarget(TargetGameObjectName);
            if (target != null)
            {
                return target.GetComponent<SkinnedMeshRenderer>();
            }
        }
        return null;
    }

    /// <summary>
    /// SkinnedMeshRenderer指定でMesh取得
    /// </summary>
    public Mesh MeshObject(SkinnedMeshRenderer SkinnedMeshObject)
    {
        if (SkinnedMeshObject != null)
        {
            return SkinnedMeshObject.sharedMesh;
        }
        return null;
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /// <summary>
    /// FacialMorphing単体でKeyboad操作による表情切替（デバッグ用途等） 
    /// </summary>
    private void ExpressionByKeyInputDown()
    {
        var enumerator = expressionList.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                var expression = enumerator.Current;
                if (Input.GetKey(expression.key))
                {
                    if (holdingInputKey == expression.key || holdingInputKey == KeyCode.None)
                    {
                        holdingInputKey = expression.key;
                        holdingBlendShape = expression.facial.Face[0];
                        isEnableBlink = expression.EnableAutoEyeBlink;
                        expression.OnChangeFacial.Invoke(expression.facial);
                    }
                }
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    private void CheckKeyInputUp()
    {
        var enumerator = expressionList.GetEnumerator();
        try
        {
            while (enumerator.MoveNext())
            {
                var expression = enumerator.Current;
                if (Input.GetKeyUp(expression.key))
                {
                    CallEvent();
                }
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    /// <summary>
    /// リセット処理
    /// </summary>
    public void ResetFacialMorphing()
    {
        ResetFacial(FACIAL_Face);
        ResetFacial(FACIAL_Brow);
        ResetFacial(FACIAL_L_Eye);
        ResetFacial(FACIAL_R_Eye);
        ResetFacial(FACIAL_Accent);
    }

    private void ResetFacial(SkinnedMeshRenderer FaceMeshRenderer)
    {
        if (FaceMeshRenderer != null)
        {
            for (int index = 0; index < FaceMeshRenderer.sharedMesh.blendShapeCount; index++)
            {
                FaceMeshRenderer.SetBlendShapeWeight(index, 0);
            }
        }
    }

}
