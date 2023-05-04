using System.Collections.Generic;
using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.UIElements;

namespace Battlehub.RTHandles
{
    public class RuntimeSceneInput : RuntimeSelectionInput
    {
        public KeyCode FocusKey = KeyCode.F;
        public KeyCode FocusActiveKey = KeyCode.LeftShift;
        public KeyCode SnapToGridKey = KeyCode.G;
        public KeyCode SnapToGridKey2 = KeyCode.LeftShift;
        public KeyCode RotateKey = KeyCode.LeftAlt;
        public KeyCode RotateKey2 = KeyCode.RightAlt;
        public KeyCode RotateKey3 = KeyCode.AltGr;
        public KeyCode MoveDownKey = KeyCode.Q;
        public KeyCode MoveUpKey = KeyCode.E;
        public MouseButton PanButton = MouseButton.MiddleMouse;

        public float RotateXSensitivity = 5.0f;
        public float RotateYSensitivity = 5.0f;
        public float MoveZSensitivity = 0.1f;
        public float FreeMoveSensitivity = 1.0f;
        public float FreeZoomSensitivity = 1.0f;
        public float FreeRotateSensitivity = 5.0f;

        public bool SwapLRMB = false;

        [SerializeField] private bool m_beginRotateImmediately = true;

        protected bool BeginRotateImmediately
        {
            get { return m_beginRotateImmediately; }
            set { m_beginRotateImmediately = value; }
        }

        [SerializeField] private bool m_beginFreeMoveImmediately = true;

        protected bool BeginFreeMoveImmediately
        {
            get { return m_beginFreeMoveImmediately; }
            set { m_beginFreeMoveImmediately = value; }
        }

        private bool m_rotate;
        private bool m_rotateActive;
        private bool m_pan;
        private bool m_freeMove;
        private bool m_freeMoveActive;
        private bool m_isActive;

        protected RuntimeSceneComponent SceneComponent
        {
            get { return (RuntimeSceneComponent)m_component; }
        }

        protected virtual bool AllowRotateAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetPointer(SwapLRMB ? 1 : 0);
        }

        protected virtual bool RotateAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(RotateKey) ||
                   input.GetKey(RotateKey2) ||
                   input.GetKey(RotateKey3);
        }

        protected virtual bool PanAction()
        {
            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;
            return input.GetPointer((int)PanButton) || input.GetPointer(SwapLRMB ? 1 : 0) &&
                tools.Current == RuntimeTool.View && tools.ActiveTool == null;
        }

        protected virtual bool FreeMoveAction()
        {
            IInput input = m_component.Editor.Input;

            if (SwapLRMB)
            {
                return input.GetPointer(0) && RotateAction();
            }

            return input.GetPointer(1);
        }

        protected virtual bool FocusAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(FocusKey);
        }

        protected virtual bool FocusActiveAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(FocusActiveKey);
        }

        protected virtual bool SnapToGridAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(SnapToGridKey) && input.GetKey(SnapToGridKey2);
        }

        protected virtual Vector2 RotateAxes()
        {
            IInput input = m_component.Editor.Input;
            float deltaX = input.GetAxis(InputAxis.X);
            float deltaY = input.GetAxis(InputAxis.Y);
            return new Vector2(deltaX, deltaY);
        }

        protected virtual float ZoomAxis()
        {
            IInput input = m_component.Editor.Input;
            float deltaZ = input.GetAxis(InputAxis.Z);
            return deltaZ;
        }

        protected virtual Vector3 MoveAxes()
        {
            IInput input = m_component.Editor.Input;
            float deltaX = input.GetAxis(InputAxis.Horizontal);
            float deltaY = input.GetAxis(InputAxis.Vertical);

            float deltaZ = 0;
            if (input.GetKey(MoveUpKey))
            {
                deltaZ = 0.5f;
            }
            else if (input.GetKey(MoveDownKey))
            {
                deltaZ = -0.5f;
            }

            return new Vector3(deltaX, deltaY, deltaZ);
        }

        protected override void Start()
        {
            base.Start();
            m_component.Editor.ActiveWindowChanged += Editor_ActiveWindowChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_component != null && m_component.Editor != null)
            {
                m_component.Editor.ActiveWindowChanged -= Editor_ActiveWindowChanged;
            }
        }

        protected virtual void UpdateCursorState(bool isPointerOverEditorArea, bool pan, bool rotate, bool freeMove)
        {
            SceneComponent.UpdateCursorState(isPointerOverEditorArea, pan, rotate, freeMove);
        }

        private void Editor_ActiveWindowChanged(RuntimeWindow deactivatedWindow)
        {
            if (m_component != null)
            {
                if (m_isActive)
                {
                    UpdateCursorState(false, false, false, false);
                    m_pan = false;
                    m_rotate = false;
                    m_freeMove = false;
                }

                m_isActive = m_component.IsWindowActive;
            }
        }

        // 添加一个计时器以检测双击
        private float m_lastClickTime;
        private const float doubleClickDelay = 0.25f;

        // 添加一个布尔变量来跟踪当前透明状态
        private bool m_isTransparent = true;

        public KeyCode SpaceKey = KeyCode.Space;
        public KeyCode ToggleVisibilityKey = KeyCode.H;

        protected override void LateUpdate()
        {
            if (!m_component.IsWindowActive)
            {
                return;
            }

            bool isPointerOver = m_component.Window.IsPointerOver;

            IInput input = m_component.Editor.Input;
            RuntimeTools tools = m_component.Editor.Tools;

            bool canRotate = AllowRotateAction();
            bool rotate = (RotateAction() || SwapLRMB && canRotate) && SceneComponent.CanRotate;
            bool pan = PanAction() && SceneComponent.CanPan;
            bool freeMove = FreeMoveAction() && SceneComponent.CanFreeMove;

            if (pan && tools.Current != RuntimeTool.View)
            {
                rotate = false;
            }

            bool beginRotate = m_rotate != rotate && rotate;
            if (beginRotate && !isPointerOver)
            {
                rotate = false;
                beginRotate = false;
            }

            bool endRotate = m_rotate != rotate && !rotate;
            m_rotate = rotate;
            if (!m_rotate)
            {
                m_rotateActive = false;
            }

            bool beginPan = m_pan != pan && pan;
            if (beginPan && !isPointerOver)
            {
                pan = false;
            }

            bool endPan = m_pan != pan && !pan;
            m_pan = pan;

            bool beginFreeMove = m_freeMove != freeMove && freeMove;
            if (beginFreeMove && !isPointerOver)
            {
                freeMove = false;
            }

            bool endFreeMove = m_freeMove != freeMove && !freeMove;
            m_freeMove = freeMove;
            if (!m_freeMove)
            {
                m_freeMoveActive = false;
            }

            Vector3 pointerPosition = input.GetPointerXY(0);
            tools.IsViewing = m_rotate || m_pan || m_freeMove;

            bool space = SpaceAction() && SceneComponent.CanRotate;


            if (beginPan || endPan || beginRotate && m_beginRotateImmediately || endRotate ||
                beginFreeMove && m_beginFreeMoveImmediately || endFreeMove)
            {
                UpdateCursorState(isPointerOver, m_pan, m_rotate && m_beginRotateImmediately,
                    m_freeMove && m_beginFreeMoveImmediately);
            }

            if (ToggleVisibilityAction())
            {
                ToggleVisibilityOfSelectedObjects();
            }


            if (m_freeMove)
            {
                Vector2 rotateAxes = RotateAxes() * FreeRotateSensitivity;
                Vector3 moveAxes = MoveAxes() * FreeMoveSensitivity * 0.005f;
                float zoomAxis = ZoomAxis() * FreeZoomSensitivity * 0.025f;
                SceneComponent.FreeMove(rotateAxes, moveAxes, zoomAxis);
                if (!m_freeMoveActive && (rotateAxes != Vector2.zero || moveAxes != Vector3.zero || zoomAxis != 0))
                {
                    UpdateCursorState(isPointerOver, m_pan, m_rotate, m_freeMove);
                    m_freeMoveActive = true;
                }
            }
            else if (m_rotate)
            {
                if (canRotate)
                {
                    Vector2 orbitAxes = RotateAxes();
                    float zoomAxis = ZoomAxis();
                    SceneComponent.Orbit(orbitAxes.x * RotateXSensitivity, orbitAxes.y * RotateYSensitivity,
                        zoomAxis * MoveZSensitivity);
                    if (!m_rotateActive && (orbitAxes != Vector2.zero || zoomAxis != 0))
                    {
                        UpdateCursorState(isPointerOver, m_pan, m_rotate, m_freeMove);
                        m_rotateActive = true;
                    }
                }
                else
                {
                    Transform camTransform = m_component.Window.Camera.transform;
                    Ray pointer = m_component.Window.Pointer;
                    SceneComponent.Zoom(ZoomAxis() * MoveZSensitivity,
                        Quaternion.FromToRotation(Vector3.forward,
                            camTransform.InverseTransformVector(pointer.direction).normalized));
                }

                SceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);
            }
            else if (space)
            {
                Vector2 orbitAxes = RotateAxes();
                SceneComponent.Orbit(orbitAxes.x * RotateXSensitivity, orbitAxes.y * RotateYSensitivity, 0);
                if (isPointerOver)
                {
                    SceneComponent.Zoom(ZoomAxis() * MoveZSensitivity, Quaternion.identity);
                }
            }
            else if (m_pan)
            {
                if (beginPan)
                {
                    SceneComponent.BeginPan(pointerPosition);
                }

                SceneComponent.Pan(pointerPosition);
            }
            else
            {
                SceneComponent.FreeMove(Vector2.zero, Vector3.zero, 0);

                if (isPointerOver)
                {
                    SceneComponent.Zoom(ZoomAxis() * MoveZSensitivity, Quaternion.identity);

                    BeginSelectAction();
                    if (SelectAction())
                    {
                        SelectGO();
                    }

                    if (SnapToGridAction())
                    {
                        SceneComponent.SnapToGrid();
                    }

                    if (FocusAction())
                    {
                        if (FocusActiveAction())
                        {
                            SceneComponent.Focus(FocusMode.AllActive);
                        }
                        else
                        {
                            if (SceneComponent.Selection.activeTransform != null &&
                                SceneComponent.Selection.activeTransform.GetComponent<Terrain>() == null)
                            {
                                SceneComponent.Focus(FocusMode.Selected);
                            }
                        }
                    }

                    if (SelectAllAction())
                    {
                        SceneComponent.SelectAll();
                    }

                    // 检查双击事件
                    if (FocusAction())
                    {
                        if (SceneComponent.Selection.activeTransform != null &&
                            SceneComponent.Selection.activeTransform.GetComponent<Terrain>() == null)
                        {
                            if (Time.time - m_lastClickTime < doubleClickDelay)
                            {
                                // 双击时切换透明状态
                                m_isTransparent = !m_isTransparent;
                                SetTransparencyForAllModels(m_isTransparent);
                            }

                            m_lastClickTime = Time.time;
                        }
                    }
                }
            }
        }

        // 新增一个方法来实现半透明和不透明之间的切换
        private void SetTransparencyForAllModels(bool isTransparent)
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            List<GameObject> selectedObjects = new List<GameObject>(SceneComponent.Selection.gameObjects);

            foreach (GameObject obj in allObjects)
            {
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    if (!selectedObjects.Contains(obj))
                        renderer.enabled = isTransparent;
                    else
                        renderer.enabled = true;
                }
            }
        }


        protected virtual bool SpaceAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKey(SpaceKey);
        }

        protected virtual bool ToggleVisibilityAction()
        {
            IInput input = m_component.Editor.Input;
            return input.GetKeyDown(ToggleVisibilityKey);
        }


        private void ToggleVisibilityOfSelectedObjects()
        {
            if (SceneComponent.Selection.gameObjects != null)
            {
                foreach (GameObject obj in SceneComponent.Selection.gameObjects)
                {
                    obj.SetActive(!obj.activeSelf);
                }
            }
        }
    }
}