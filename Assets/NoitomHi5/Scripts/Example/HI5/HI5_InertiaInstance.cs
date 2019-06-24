using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HI5
{
    public class HI5_InertiaInstance : HI5_Instance
    {
        [SerializeField] private Renderer m_Renderer;

        [SerializeField] private List<float> thumbAngles = new List<float> { 0f, 0f, 0f, 0f };
        [SerializeField] private List<float> indexAngles = new List<float> { 0f, 0f, 0f, 0f };
        [SerializeField] private List<float> middleAngles = new List<float> { 0f, 0f, 0f, 0f };
        [SerializeField] private List<float> ringAngles = new List<float> { 0f, 0f, 0f, 0f };
        [SerializeField] private List<float> pinkyAngles = new List<float> { 0f, 0f, 0f, 0f };

        [SerializeField] private bool invertFingerRotation;
        [SerializeField] private List<Vector3> thumbMirrorPlaneNormals = new List<Vector3> {
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward
        };

        public bool IsValid { get { return isValid; } }
        private bool isValid = false;

        private HI5_GloveStatus m_Status;
        private static int m_INDEX_Hand = (int)Bones.Hand;

        new void OnEnable()
        {
            base.OnEnable();
        }

        new void OnDisable()
        {
            base.OnDisable();
        }

        private void Start()
        {
            m_Status = HI5_Manager.GetGloveStatus();
        }

        new void Update()
        {
            base.Update();

            if (m_BindSource != null)
            {
                ApplyHandMotion_Rotation(m_BindSource);
            }

            if (m_BindSource != null)
            {
                ApplyFingerMotion(m_BindSource);
            }
        }

        private void CheckIsValid()
        {
            bool isAvailable = m_Status.IsGloveAvailable(HandType);
            bool isBinded = HI5_BindInfoManager.IsGloveBinded(HandType);
            if (isAvailable && isBinded)
                isValid = true;
            else
                isValid = false;

            if (HI5_Calibration.IsCalibratingBPose)
                isValid = false;
        }

        private void UpdateModelRenderer(bool value)
        {
            m_Renderer.gameObject.SetActive(value);
        }

        private void ApplyFingerMotion(HI5_Source source)
        {
            for (int i = (m_INDEX_Hand + 1); i < (int)Bones.NumOfHI5Bones && i < HandBones.Length; i++)
            {
                if (HandBones[i] != null)
                {
                    SetRotation(HandBones, i, source.GetReceivedRotation(i, HandType));
                }
            }
        }

        private void ApplyHandMotion_Rotation(HI5_Source source)
        {
            if (HandBones[m_INDEX_Hand] != null)
            {
                HandBones[m_INDEX_Hand].localEulerAngles = HI5_DataTransform.ToUnityEulerAngles(source.GetReceivedRotation(m_INDEX_Hand, HandType));
            }
        }

        private void ApplyHandMotion_Position(Vector3 pos, Quaternion rot)
        {
            Vector3 offset = HandType == Hand.LEFT ? HI5_Manager.LeftOffset : HI5_Manager.RightOffset;
            Vector3 handPos = pos + rot * offset;

            if (HandBones[m_INDEX_Hand] != null)
            {
                HandBones[m_INDEX_Hand].localPosition = handPos;
            }
        }

        private void SetPosition(Transform[] bones, int bone, Vector3 position)
        {
            Transform t = bones[(int)bone];
            if (t != null)
            {
                Vector3 pos = position;
                if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y) && !float.IsNaN(pos.z))
                {
                    t.localPosition = pos;
                }
            }
        }

        private void SetRotation(Transform[] bones, int bone, Vector3 rotation)
        {
            Transform t = bones[(int)bone];
            if (t != null)
            {
                if (invertFingerRotation) {
                    Vector3 planeNormal = GetInvertPlaneNormal(bone);
                    rotation = 2 * Vector3.ProjectOnPlane(rotation, planeNormal) - rotation;
                }
                Quaternion rot = Quaternion.Euler(rotation);
                float angle = GetFingerAngle(bone);
                rot *= Quaternion.AngleAxis(angle, Vector3.left);
                if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z) && !float.IsNaN(rot.w))
                {
                    t.localRotation = rot;
                }
            }
        }

        private Vector3 GetInvertPlaneNormal(int bone)
        {
            int fingerIndex = (bone - 2) / 4;
            int fingerJointIndex = (bone - 2) % 4;
            if (fingerIndex != 0) { // not thumb
                return Vector3.forward;
            }
            return thumbMirrorPlaneNormals[fingerJointIndex];
        }

        private float GetFingerAngle(int bone)
        {
            if (bone < 2) {
                return 0f;
            }
            int fingerIndex = (bone - 2) / 4;
            int fingerJointIndex = (bone - 2) % 4;
            return fingerIndex == 0 ? thumbAngles[fingerJointIndex]
                 : fingerIndex == 1 ? indexAngles[fingerJointIndex]
                 : fingerIndex == 2 ? middleAngles[fingerJointIndex]
                 : fingerIndex == 3 ? ringAngles[fingerJointIndex]
                 : fingerIndex == 4 ? pinkyAngles[fingerJointIndex]
                 : 0f;
        }
    }
}
