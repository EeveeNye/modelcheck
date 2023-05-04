using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IVector3Editor
    {
        bool? IsInteractable { get; set; }

        bool IsXInteractable { get; set; }

        bool IsYInteractable { get; set; }
        bool IsZInteractable { get; set; }
    }

    public class TransformEditor : ComponentEditor
    {
        public static Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();
        public static Dictionary<Transform, Quaternion> initialRotations = new Dictionary<Transform, Quaternion>();
        public static Dictionary<Transform, Vector3> initialScales = new Dictionary<Transform, Vector3>();

        private void StoreInitialTransforms()
        {
            GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allGameObjects)
            {
                Transform transform = go.transform;
                initialPositions.Add(transform, transform.localPosition);
                initialRotations.Add(transform, transform.localRotation);
                initialScales.Add(transform, transform.localScale);
            }
        }


        protected override void InitEditor(PropertyEditor editor, PropertyDescriptor descriptor)
        {
            currentPropertyDescriptor = descriptor;


            base.InitEditor(editor, descriptor);


            bool canTransform = true;
            if (Components != null && Components.Length > 0)
            {
                IEnumerable<ExposeToEditor> exposeToEditor =
                    NotNullComponents.Select(component => component.GetComponentInParent<ExposeToEditor>());
                if (exposeToEditor.Any(o => o != null && !o.CanTransform))
                {
                    canTransform = false;
                }
            }


            if (initialScales.Count == 0)
            {
                StoreInitialTransforms();
            }

            if (Editor.Tools.LockAxes == null && canTransform)
            {
                return;
            }

            if (descriptor.ComponentMemberInfo ==
                Strong.PropertyInfo((Transform x) => x.localPosition, "localPosition"))
            {
                IVector3Editor vector3Editor = editor as IVector3Editor;
                if (vector3Editor != null)
                {
                    if (!canTransform)
                    {
                        vector3Editor.IsXInteractable = false;
                        vector3Editor.IsYInteractable = false;
                        vector3Editor.IsZInteractable = false;
                    }
                    else if (Editor.Tools.LockAxes != null)
                    {
                        vector3Editor.IsXInteractable = !Editor.Tools.LockAxes.PositionX;
                        vector3Editor.IsYInteractable = !Editor.Tools.LockAxes.PositionY;
                        vector3Editor.IsZInteractable = !Editor.Tools.LockAxes.PositionZ;
                    }
                }
            }

            if (descriptor.ComponentMemberInfo ==
                Strong.PropertyInfo((Transform x) => x.localRotation, "localRotation"))
            {
                IVector3Editor vector3Editor = editor as IVector3Editor;
                if (vector3Editor != null)
                {
                    if (!canTransform)
                    {
                        vector3Editor.IsXInteractable = false;
                        vector3Editor.IsYInteractable = false;
                        vector3Editor.IsZInteractable = false;
                    }
                    else if (Editor.Tools.LockAxes != null)
                    {
                        vector3Editor.IsXInteractable = !Editor.Tools.LockAxes.RotationX;
                        vector3Editor.IsYInteractable = !Editor.Tools.LockAxes.RotationY;
                        vector3Editor.IsZInteractable = !Editor.Tools.LockAxes.RotationZ;
                    }
                }
            }

            if (descriptor.ComponentMemberInfo == Strong.PropertyInfo((Transform x) => x.localScale, "localScale"))
            {
                IVector3Editor vector3Editor = editor as IVector3Editor;
                if (vector3Editor != null)
                {
                    if (!canTransform)
                    {
                        vector3Editor.IsXInteractable = false;
                        vector3Editor.IsYInteractable = false;
                        vector3Editor.IsZInteractable = false;
                    }
                    else if (Editor.Tools.LockAxes != null)
                    {
                        vector3Editor.IsXInteractable = !Editor.Tools.LockAxes.ScaleX;
                        vector3Editor.IsYInteractable = !Editor.Tools.LockAxes.ScaleY;
                        vector3Editor.IsZInteractable = !Editor.Tools.LockAxes.ScaleZ;
                    }
                }
            }
        }

        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            RefreshTransformHandles();
        }

        protected override void OnEndEdit()
        {
            base.OnEndEdit();
            ResetTransformHandles();
        }

        private PropertyDescriptor currentPropertyDescriptor;

        protected override void OnResetClick()
        {
            Transform transform = (Transform)Components[0];

            transform.localPosition = initialPositions[transform];
            transform.localRotation = initialRotations[transform];
            transform.localScale = initialScales[transform];

            ResetTransformHandles();
        }


        private static void RefreshTransformHandles()
        {
            BaseHandle[] handles = FindObjectsOfType<BaseHandle>();
            foreach (BaseHandle handle in handles)
            {
                handle.Refresh();
            }
        }

        private static void ResetTransformHandles()
        {
            BaseHandle[] handles = FindObjectsOfType<BaseHandle>();
            foreach (BaseHandle handle in handles)
            {
                handle.Targets = handle.RealTargets;
            }
        }
    }
}