using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DKF
{
    public class JoyconController : MonoBehaviour
    {
        private static readonly Joycon.Button[] m_buttons =
            Enum.GetValues(typeof(Joycon.Button)) as Joycon.Button[];

        private List<Joycon> m_joycons;
        public int joycon_L_Id = 0;
        public int joycon_R_Id = 1;
        public bool enableGUI = false;
        public enum JoyconHand { L, R, None };
        public enum StickDirection { Left, Right, Up, Down, None };
        private StickDirection stick_L;
        private StickDirection stick_R;

        [System.Serializable]
        public class PhysicalEvent : UnityEvent<Vector3, Vector3> { };  //T0: ,T1:

        public List<JoyconAction> joyconActionList;
        [System.SerializableAttribute]
        public class JoyconAction
        {
            //Title
            public string name;

            //Hand 主キー
            public bool enablePrimary = true;
            public JoyconHand primary_hand = JoyconHand.R;
            public Joycon.Button primary_button;

            //Hand 修飾キー
            public bool enableModifier = false;
            public JoyconHand modifier_hand = JoyconHand.L;
            public Joycon.Button modifier_button;

            //Stick
            public bool enableStick = false;
            public JoyconHand stickHand = JoyconHand.None;
            public StickDirection stickDirection = StickDirection.None;

            //ButtonEvent
            public UnityEvent press;
            public UnityEvent up;
            public UnityEvent down;
            public PhysicalEvent upWithPhysical;

            public JoyconAction(string name, JoyconHand primary_hand, Joycon.Button primary_button, JoyconHand modifier_hand, Joycon.Button modifier_button,
                JoyconHand stickHand, StickDirection stickDirection, bool enableStick,
                bool enableModifier, UnityEvent press,  UnityEvent up, UnityEvent down, PhysicalEvent upWithPhysical)
            {
                this.name = name;
                this.primary_hand = primary_hand;
                this.primary_button = primary_button;
                this.modifier_hand = modifier_hand;
                this.modifier_button = modifier_button;
                this.enableModifier = enableModifier;
                this.stickHand = stickHand;
                this.stickDirection = stickDirection;
                this.enableStick = enableStick;
                this.press = press;
                this.up = up;
                this.down = down;
                this.upWithPhysical = upWithPhysical;
            }
        }

        private void Start()
        {
            m_joycons = JoyconManager.Instance.j;
            Debug.Log("接続中Joycon : " + m_joycons.Count);
        }

        private void Update()
        {
            if (m_joycons == null || m_joycons.Count <= 0) return;
            CheckJoycons();
        }

        private void CheckJoycons()
        {
            var enumerator = joyconActionList.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    var action = enumerator.Current;
                    CheckButtonEvent(action);
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private void CheckButtonEvent(JoyconAction action)
        {
            var Id_pri = JoyconHandId(action.primary_hand);
            var Id_mod = JoyconHandId(action.modifier_hand);
            var Id_Stk = JoyconHandId(action.stickHand);

            //ButtonPress
            if (!(m_joycons.Count - 1 < Id_pri) && !(m_joycons.Count - 1 < Id_mod) && !(m_joycons.Count - 1 < Id_Stk) &&
                CheckButtonPress(m_joycons[Id_pri], m_joycons[Id_mod], m_joycons[Id_Stk], action)
                )
            {
                ////Debug
                //print("JoyconPrimary:Press " + Id_pri + " "+ action.primary_button.ToString() + " >>Invoke()");
                //print("JoyconModifier:Press " + Id_mod + " " + action.modifier_button.ToString() + " >>Invoke()");
                //print("JoyconStick:Press " + Id_Stk + " " + action.stickDirection.ToString() + " >>Invoke()");
                action.press.Invoke();
            }

            //ButtonUp
            if (!(m_joycons.Count - 1 < Id_pri) && !(m_joycons.Count - 1 < Id_mod) &&
                CheckButtonUp(m_joycons[Id_pri], m_joycons[Id_mod], m_joycons[Id_Stk], action)
                )
            {
                ////Debug
                //print("JoyconPrimary:Up " + Id_pri + " " + action.primary_button.ToString() + " >>Invoke()");
                //print("JoyconModifier:Up " + Id_mod + " " + action.modifier_button.ToString() + " >>Invoke()");
                //print("JoyconStick:Up " + Id_Stk + " " + action.stickDirection.ToString() + " >>Invoke()");
                action.up.Invoke();
                action.upWithPhysical.Invoke(m_joycons[Id_pri].GetAccel(), m_joycons[Id_pri].GetGyro());
            }

            //ButtonDown
            if (!(m_joycons.Count - 1 < Id_pri) && !(m_joycons.Count - 1 < Id_mod) &&
                CheckButtonDown(m_joycons[Id_pri], m_joycons[Id_mod], m_joycons[Id_Stk], action)
                )
            {
                ////Debug
                //print("JoyconPrimary:Down " + Id_pri + " " + action.primary_button.ToString() + " >>Invoke()");
                //print("JoyconModifier:Down " + Id_mod + " " + action.modifier_button.ToString() + " >>Invoke()");
                //print("JoyconStick:Down " + Id_Stk + " " + action.stickDirection.ToString() + " >>Invoke()");
                action.down.Invoke();
            }
        }

        private int JoyconHandId(JoyconHand hand)
        {
            if (hand == JoyconHand.L)
                return joycon_L_Id;
            if (hand == JoyconHand.R)
                return joycon_R_Id;
            return 0;
        }

        private bool CheckButtonPress(Joycon joyconPrimary, Joycon joyconModifier, Joycon joyconStick, JoyconAction action)
        {
            if (!action.enablePrimary && !action.enableModifier && !action.enableStick) return false;

            var direction = JoyconStickDirection(JoystickPosition(joyconStick));

            if (((!action.enablePrimary) || joyconPrimary.GetButton(action.primary_button)) &&
                ((!action.enableModifier) || joyconModifier.GetButton(action.modifier_button)) &&
                ((!action.enableStick) || direction == action.stickDirection)
                )
            {
                //Debug.Log("BINGO!:Press");
                return true;
            }
            return false;
        }

        private bool CheckButtonUp(Joycon joyconPrimary, Joycon joyconModifier, Joycon joyconStick, JoyconAction action)
        {
            if (!action.enablePrimary && !action.enableModifier && (!action.enableStick || action.enableStick)) return false;

            var direction = JoyconStickDirection(JoystickPosition(joyconStick));

            if (((!action.enablePrimary) || joyconPrimary.GetButtonUp(action.primary_button)) &&
                ((!action.enableModifier) || joyconModifier.GetButtonUp(action.modifier_button)) &&
                ((!action.enableStick) || direction == action.stickDirection)
                )
            {
                //Debug.Log("BINGO!:Up");
                return true;
            }
            return false;
        }

        private bool CheckButtonDown(Joycon joyconPrimary, Joycon joyconModifier, Joycon joyconStick, JoyconAction action)
        {
            if (!action.enablePrimary && !action.enableModifier && (!action.enableStick || action.enableStick)) return false;

            var direction = JoyconStickDirection(JoystickPosition(joyconStick));

            if (((!action.enablePrimary) || joyconPrimary.GetButtonDown(action.primary_button)) &&
                ((!action.enableModifier) || joyconModifier.GetButtonDown(action.modifier_button)) &&
                ((!action.enableStick) || direction == action.stickDirection)
                )
            {
                //Debug.Log("BINGO!:Down");
                return true;
            }
            return false;
        }
        
        //Vibe Feedback
        public void Feedback(int JoyconId)
        {
            if (JoyconId <= -1 || m_joycons.Count - 1 < JoyconId) return;
            m_joycons[JoyconId].SetRumble(160, 320, 0.6f, 200);
        }

        /// <summary>
        /// StickDirection
        /// </summary>
        /// <param name="stickPosition"></param>
        /// <returns></returns>
        private StickDirection JoyconStickDirection(Vector2 stickPosition)
        {
            if (stickPosition.x == 0 && stickPosition.y == 0)
            {
                return StickDirection.None;
            }
            if (stickPosition.y / stickPosition.x > 1 || stickPosition.y / stickPosition.x < -1)
            {
                if (stickPosition.y > 0)
                {
                    return StickDirection.Up;   //Debug.Log(name + ":" + "Press UP");
                }
                else
                {
                    return StickDirection.Down; //Debug.Log(name + ":" + "Press DOWN");
                }
            }
            else
            {
                if (stickPosition.x > 0)
                {
                    return StickDirection.Right;    //Debug.Log(name + ":" + "Press RIGHT");
                }
                else
                {
                    return StickDirection.Left; //Debug.Log(name + ":" + "Press LEFT");
                }
            }
        }

        public Vector2 JoystickPosition(Joycon joycon)
        {
            var stick = joycon.GetStick();
            return new Vector2(stick[0], stick[1]);
        }

        /// <summary>
        /// GUI
        /// </summary>
        private void OnGUI()
        {
            if (!enableGUI) return;

            var style = GUI.skin.GetStyle("label");
            style.fontSize = 24;

            if (m_joycons == null || m_joycons.Count <= 0)
            {
                GUILayout.Label("Joy-Con が接続されていません");
                return;
            }

            GUILayout.BeginHorizontal(GUILayout.Width(960));

            var i = 0;
            foreach (var joycon in m_joycons)
            {
                var isLeft = joycon.isLeft;
                var hashCode = joycon.GetHashCode();
                var joyconId = i;
                var name = isLeft ? "Joy-Con (L)" : "Joy-Con (R)";
                var button = "";
                var buttonUp = "";
                var buttonDown = "";

                foreach (var b in m_buttons)
                {
                    if (joycon.GetButton(b))
                    {
                        button = "ID:" + joyconId + " / " + b.ToString();
                    }
                    if (joycon.GetButtonUp(b))
                    {
                        buttonUp = "ID:" + joyconId + " / " + b.ToString();
                    }
                    if (joycon.GetButtonDown(b))
                    {
                        buttonDown = "ID:" + joyconId + " / " + b.ToString();
                    }
                }

                var direction = JoyconStickDirection(JoystickPosition(joycon));
                var stick = joycon.GetStick();
                var gyro = joycon.GetGyro();
                var accel = joycon.GetAccel();
                var orientation = joycon.GetVector();

                GUILayout.BeginVertical(GUILayout.Width(320));
                GUILayout.Label("JoyCon.No：" + joyconId.ToString());
                GUILayout.Label(name);
                GUILayout.Label("HashCode：" + hashCode.ToString());
                GUILayout.Label("Press：" + button);
                GUILayout.Label("Up：" + buttonUp);
                GUILayout.Label("Down：" + buttonDown);
                GUILayout.Label("StickDirection：" + direction);
                GUILayout.Label(string.Format("スティック：({0}, {1})", stick[0], stick[1]));
                GUILayout.Label("ジャイロ：" + gyro);
                GUILayout.Label("加速度：" + accel);
                GUILayout.Label("傾き：" + orientation);
                GUILayout.EndVertical();
                i++;
            }

            GUILayout.EndHorizontal();
        }


        //DEBUG
        public void ButtonPressDebug()
        {
            Debug.Log("Joycon Button Press");
        }
        public void ButtonUpDebug()
        {
            Debug.Log("Joycon Button Up");
        }
        public void ButtonDownDebug()
        {
            Debug.Log("Joycon Button Down");
        }

    }
}