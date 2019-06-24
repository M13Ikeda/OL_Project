using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Facial
{
    public class AutoBlink : MonoBehaviour
    {
        public bool isActive = true;                //オート目パチ有効

        public SkinnedMeshRenderer ref_FACIAL;
        public List<string> ref_BlendShapesList;    //目パチ対象のBlendShapesのパラメータ名リスト
        private Mesh ref_MESH;

        public FacialMorphing facialMorphing;       //特殊表情時に目パチさせるかの参照用

        public float ratio_Close = 85.0f;           //閉じ目ブレンドシェイプ比率
        public float ratio_HalfClose = 20.0f;       //半閉じ目ブレンドシェイプ比率
        [HideInInspector]
        public float ratio_Open = 0.0f;
        public float timeBlink = 0.4f;              //目パチの時間

        public float threshold = 0.3f;              // ランダム判定の閾値
        public float interval = 3.0f;               // ランダム判定のインターバル

        class EyelidAnimator
        {
            private float timeBlinkSec;
            private float timeRemaining = 0f;
            private SkinnedMeshRenderer ref_FACIAL;
            private string ref_BlendShapeName;
            private int ref_blendShapesId;

            enum Status
            {
                Open = 0,
                HalfClose,
                Close,
                HalfOpen,
                NotAnimating,

                STATUS_LENGTH
            }
            private Status eyeStatus;   //現在の目パチステータス

            struct StateTransition
            {
                public Status NextState;
                public float StartWeight;
                public float EndWeight;
            }
            StateTransition[] stateTable = new StateTransition[(int)Status.STATUS_LENGTH];

            public EyelidAnimator(SkinnedMeshRenderer facial, Mesh mesh, int blendShapesId)
            {
                ref_FACIAL = facial;
                ref_blendShapesId = blendShapesId;

                stateTable[0].NextState = Status.HalfClose;
                stateTable[1].NextState = Status.Close;
                stateTable[2].NextState = Status.HalfOpen;
                stateTable[3].NextState = Status.NotAnimating;
            }

            public void Start(float timeBlinkSec, float ratioClose, float ratioHalfClose, float ratioOpen)
            {
                this.timeBlinkSec = timeBlinkSec / 4f;
                timeRemaining = this.timeBlinkSec;

                eyeStatus = Status.Open;
                stateTable[3].EndWeight = stateTable[0].StartWeight = ratioOpen;
                stateTable[0].EndWeight = stateTable[1].StartWeight = ratioHalfClose;
                stateTable[1].EndWeight = stateTable[2].StartWeight = ratioClose;
                stateTable[2].EndWeight = stateTable[3].StartWeight = ratioHalfClose;
            }

            private void setRatio(float ratio)
            {
                ref_FACIAL.SetBlendShapeWeight(ref_blendShapesId, ratio);
            }

            // Must call every frame
            public void Update()
            {
                if (!IsAnimating())
                {
                    return;
                }

                timeRemaining -= Time.deltaTime;
                // 残り時間が少なくなるにつれて 1 に近づく
                var animWeight = 1f - Mathf.Clamp(timeRemaining / this.timeBlinkSec, 0, 1);

                var stateData = stateTable[(int)eyeStatus];
                if (timeRemaining < 0f)
                {
                    eyeStatus = stateData.NextState;
                    timeRemaining += timeBlinkSec;
                }

                var ratio = Mathf.Lerp(stateData.StartWeight, stateData.EndWeight, animWeight);
                setRatio(ratio);
            }

            public bool IsAnimating()
            {
                return eyeStatus != Status.NotAnimating;
            }
        }
        private List<EyelidAnimator> eyelidAnimatorList = new List<EyelidAnimator>();

        private bool IsBlockFacial()
        {
            if (facialMorphing.IsEnableBlink)
            {
                return false;
            }
            return true;
        }

        void Awake()
        {
            ref_MESH = ref_FACIAL.sharedMesh;
            foreach (var ref_name in ref_BlendShapesList)
            {
                var id = ref_MESH.GetBlendShapeIndex(ref_name);
                eyelidAnimatorList.Add(new EyelidAnimator(ref_FACIAL, ref_MESH, id));
            }
        }

        void Start()
        {
            StartCoroutine("RandomChange");
        }

        void Update()
        {
            if (isActive)
            {
                EyeUpdate();
            }
        }

        private void EyeUpdate()
        {
            var eyeAnim = eyelidAnimatorList.GetEnumerator();
            try
            {
                while (eyeAnim.MoveNext())
                {
                    eyeAnim.Current.Update();
                }
            }
            finally
            {
                eyeAnim.Dispose();
            }
        }


        // ランダム判定用関数
        IEnumerator RandomChange()
        {
            // 無限ループ開始
            while (true)
            {
                //ランダム判定用シード発生
                float _seed = Random.Range(0.0f, 1.0f);

                if (!IsAnimation())
                {
                    if (_seed > threshold && !IsBlockFacial())
                    {
                        EyeAnimation();
                    }
                }
                // 次の判定までインターバルを置く
                yield return new WaitForSeconds(interval);
            }
        }

        private bool IsAnimation()
        {
            foreach (EyelidAnimator eyelidAnimator in eyelidAnimatorList)
            {
                if (eyelidAnimator.IsAnimating())
                {
                    return true;
                }
            }
            return false;
        }

        private void EyeAnimation()
        {
            var eyeAnim = eyelidAnimatorList.GetEnumerator();
            try
            {
                while (eyeAnim.MoveNext())
                {
                    eyeAnim.Current.Start(timeBlink, ratio_Close, ratio_HalfClose, ratio_Open);
                }
            }
            finally
            {
                eyeAnim.Dispose();
            }
        }
    }
}