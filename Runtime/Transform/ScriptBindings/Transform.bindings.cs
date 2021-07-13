// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine.Bindings;
using UnityEngine.Scripting;

using System;
using System.Collections;

namespace UnityEngine
{
    //*undocumented
    internal enum RotationOrder { OrderXYZ, OrderXZY, OrderYZX, OrderYXZ, OrderZXY, OrderZYX }

    // Position, rotation and scale of an object.
    [NativeHeader("Configuration/UnityConfigure.h")]
    [NativeHeader("Runtime/Transform/Transform.h")]
    [NativeHeader("Runtime/Transform/ScriptBindings/TransformScriptBindings.h")]
    [RequiredByNativeCode]
    public partial class Transform : Component, IEnumerable
    {
        protected Transform() {}

        // 该物体相对于世界坐标系坐标原点的坐标
        public extern Vector3 position { get; set; }

        // 该物体相对于父物体为坐标原点相对应的坐标
        public extern Vector3 localPosition { get; set; }

        // Get local euler angles with rotation order specified
        internal extern Vector3 GetLocalEulerAngles(RotationOrder order);

        // Set local euler angles with rotation order specified
        internal extern void SetLocalEulerAngles(Vector3 euler, RotationOrder order);

        // Set local euler hint
        [NativeConditional("UNITY_EDITOR")]
        internal extern void SetLocalEulerHint(Vector3 euler);

        // 该物体在世界坐标系中的旋转角度
        public Vector3 eulerAngles { get { return rotation.eulerAngles; } set { rotation = Quaternion.Euler(value); } }

        // 该物体相对于父物体为坐标原点的旋转角度，该数值对应inspector面板Rotation数值
        public Vector3 localEulerAngles { get { return localRotation.eulerAngles; } set { localRotation = Quaternion.Euler(value); } }

        // 在世界坐标系中红色坐标轴所对应的方向
        public Vector3 right { get { return rotation * Vector3.right; } set { rotation = Quaternion.FromToRotation(Vector3.right, value); } }

        // 在世界坐标系中绿色轴所对应的方向
        public Vector3 up { get { return rotation * Vector3.up; } set { rotation = Quaternion.FromToRotation(Vector3.up, value); } }

        // 在世界坐标系中蓝色轴所对应的方向
        public Vector3 forward { get { return rotation * Vector3.forward; } set { rotation = Quaternion.LookRotation(value); } }

        // 该物体在世界坐标系中旋转所对应的四元数
        public extern Quaternion rotation { get; set; }

        // 该物体在父物体坐标系旋转所对应的四元数
        public extern Quaternion localRotation { get; set; }

        // The euler rotation order for this transform
        [NativeConditional("UNITY_EDITOR")]
        internal RotationOrder rotationOrder
        {
            get { return (RotationOrder)GetRotationOrderInternal(); }
            set { SetRotationOrderInternal(value); }
        }

        [NativeConditional("UNITY_EDITOR")]
        [NativeMethod("GetRotationOrder")]
        internal extern int GetRotationOrderInternal();
        [NativeConditional("UNITY_EDITOR")]
        [NativeMethod("SetRotationOrder")]
        internal extern void SetRotationOrderInternal(RotationOrder rotationOrder);

        // 该物体对应父物体坐标系的缩放值
        public extern Vector3 localScale { get; set; }

        // 该物体的父物体的Transform
        public Transform parent
        {
            get { return parentInternal; }
            set
            {
                if (this is RectTransform)
                    Debug.LogWarning("Parent of RectTransform is being set with parent property. Consider using the SetParent method instead, with the worldPositionStays argument set to false. This will retain local orientation and scale rather than world orientation and scale, which can prevent common UI scaling issues.", this);
                parentInternal = value;
            }
        }

        internal Transform parentInternal
        {
            get { return GetParent(); }
            set { SetParent(value); }
        }

        private extern Transform GetParent();
        //将该物体设为参数p的子物体
        public void SetParent(Transform p)
        {
            SetParent(p, true);
        }

        [FreeFunction("SetParent", HasExplicitThis = true)]
        public extern void SetParent(Transform parent, bool worldPositionStays);

        // Matrix that transforms a point from world space into local space (RO).
        public extern Matrix4x4 worldToLocalMatrix { get; }
        // Matrix that transforms a point from local space into world space (RO).
        public extern Matrix4x4 localToWorldMatrix { get; }

        //设置该物体在世界坐标系中的位置和旋转（四元数）
        public extern void SetPositionAndRotation(Vector3 position, Quaternion rotation);

        // 将该物体的位置移动translation 向量的位置
        public void Translate(Vector3 translation, [UnityEngine.Internal.DefaultValue("Space.Self")] Space relativeTo)
        {
            if (relativeTo == Space.World)
                position += translation;
            else
                position += TransformDirection(translation);
        }

        public void Translate(Vector3 translation)
        {
            Translate(translation, Space.Self);
        }

        // 通过三个轴向的数值把该物体进行移动一定的距离
        public void Translate(float x, float y, float z, [UnityEngine.Internal.DefaultValue("Space.Self")] Space relativeTo)
        {
            Translate(new Vector3(x, y, z), relativeTo);
        }

        public void Translate(float x, float y, float z)
        {
            Translate(new Vector3(x, y, z), Space.Self);
        }

        // 相对于某个物体方向上，移动该物体.
        public void Translate(Vector3 translation, Transform relativeTo)
        {
            if (relativeTo)
                position += relativeTo.TransformDirection(translation);
            else
                position += translation;
        }

        // 通过三个轴向的大小移动相对于物体的距离
        public void Translate(float x, float y, float z, Transform relativeTo)
        {
            Translate(new Vector3(x, y, z), relativeTo);
        }

        // 相对与自身坐标，该物体在三个轴向上旋转某个角度
        public void Rotate(Vector3 eulers, [UnityEngine.Internal.DefaultValue("Space.Self")] Space relativeTo)
        {
            Quaternion eulerRot = Quaternion.Euler(eulers.x, eulers.y, eulers.z);
            if (relativeTo == Space.Self)
                localRotation = localRotation * eulerRot;
            else
            {
                rotation = rotation * (Quaternion.Inverse(rotation) * eulerRot * rotation);
            }
        }
        //相对于自身的坐标，该物体旋转某个向量
        public void Rotate(Vector3 eulers)
        {
            Rotate(eulers, Space.Self);
        }

        // 通过三个轴向的大小，该物体旋转某个角度
        public void Rotate(float xAngle, float yAngle, float zAngle, [UnityEngine.Internal.DefaultValue("Space.Self")] Space relativeTo)
        {
            Rotate(new Vector3(xAngle, yAngle, zAngle), relativeTo);
        }
        // 通过三个轴向的大小，该物体旋转某个角度
        public void Rotate(float xAngle, float yAngle, float zAngle)
        {
            Rotate(new Vector3(xAngle, yAngle, zAngle), Space.Self);
        }

        [NativeMethod("RotateAround")]
        internal extern void RotateAroundInternal(Vector3 axis, float angle);

        // 该物体在某个轴向上，旋转某个角度
        public void Rotate(Vector3 axis, float angle, [UnityEngine.Internal.DefaultValue("Space.Self")] Space relativeTo)
        {
            if (relativeTo == Space.Self)
                RotateAroundInternal(transform.TransformDirection(axis), angle * Mathf.Deg2Rad);
            else
                RotateAroundInternal(axis, angle * Mathf.Deg2Rad);
        }
        // 该物体在某个轴向上，旋转某个角度
        public void Rotate(Vector3 axis, float angle)
        {
            Rotate(axis, angle, Space.Self);
        }

        // 该物体围绕某个点，在某个轴向上，旋转一定的角度
        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            Vector3 worldPos = position;
            Quaternion q = Quaternion.AngleAxis(angle, axis);
            Vector3 dif = worldPos - point;
            dif = q * dif;
            worldPos = point + dif;
            position = worldPos;
            RotateAroundInternal(axis, angle * Mathf.Deg2Rad);
        }

        // 通过旋转，物体的蓝色坐标轴指向某个物体
        public void LookAt(Transform target, [UnityEngine.Internal.DefaultValue("Vector3.up")] Vector3 worldUp) { if (target) LookAt(target.position, worldUp); }
        public void LookAt(Transform target) { if (target) LookAt(target.position, Vector3.up); }

        // 通过旋转，物体的蓝色坐标轴指向世界坐标系中的点
        public void LookAt(Vector3 worldPosition, [UnityEngine.Internal.DefaultValue("Vector3.up")] Vector3 worldUp) { Internal_LookAt(worldPosition, worldUp); }
        public void LookAt(Vector3 worldPosition) { Internal_LookAt(worldPosition, Vector3.up); }

        [FreeFunction("Internal_LookAt", HasExplicitThis = true)]
        private extern void Internal_LookAt(Vector3 worldPosition, Vector3 worldUp);

        // Transforms /direction/ from local space to world space.
        public extern Vector3 TransformDirection(Vector3 direction);

        // Transforms direction /x/, /y/, /z/ from local space to world space.
        public Vector3 TransformDirection(float x, float y, float z) { return TransformDirection(new Vector3(x, y, z)); }

        // Transforms a /direction/ from world space to local space. The opposite of Transform.TransformDirection.
        public extern Vector3 InverseTransformDirection(Vector3 direction);

        // Transforms the direction /x/, /y/, /z/ from world space to local space. The opposite of Transform.TransformDirection.
        public Vector3 InverseTransformDirection(float x, float y, float z) { return InverseTransformDirection(new Vector3(x, y, z)); }


        // Transforms /vector/ from local space to world space.
        public extern Vector3 TransformVector(Vector3 vector);

        // Transforms vector /x/, /y/, /z/ from local space to world space.
        public Vector3 TransformVector(float x, float y, float z) { return TransformVector(new Vector3(x, y, z)); }

        // Transforms a /vector/ from world space to local space. The opposite of Transform.TransformVector.
        public extern Vector3 InverseTransformVector(Vector3 vector);

        // Transforms the vector /x/, /y/, /z/ from world space to local space. The opposite of Transform.TransformVector.
        public Vector3 InverseTransformVector(float x, float y, float z) { return InverseTransformVector(new Vector3(x, y, z)); }


        // 将该物体本地坐标的点转化成世界坐标
        public extern Vector3 TransformPoint(Vector3 position);

        // 将该物体本地坐标的X值，Y值，Z值组成的坐标转化成世界坐标
        public Vector3 TransformPoint(float x, float y, float z) { return TransformPoint(new Vector3(x, y, z)); }

        // 将世界坐标的点转换成本地坐标
        public extern Vector3 InverseTransformPoint(Vector3 position);

        // 通过世界坐标的X,Y,Z组成的点转换成本地坐标
        public Vector3 InverseTransformPoint(float x, float y, float z) { return InverseTransformPoint(new Vector3(x, y, z)); }

        // 获得Herachy面板是该物体最上层的物体的Transform
        public Transform root { get { return GetRoot(); } }

        private extern Transform GetRoot();

        // 获得该物体子物体的数量
        public extern int childCount
        {
            [NativeMethod("GetChildrenCount")]
            get;
        }

        // Unparents all children.
        [FreeFunction("DetachChildren", HasExplicitThis = true)]
        public extern void DetachChildren();

        // 将该物体移动到父物体最上面的子物体的位置
        public extern void SetAsFirstSibling();

        // 将该物体移动到父物体最下面的子物体的位置
        public extern void SetAsLastSibling();
        // 将该物体移动到父物体某个子物体的位置
        public extern void SetSiblingIndex(int index);

        [NativeMethod("MoveAfterSiblingInternal")]
        internal extern void MoveAfterSibling(Transform transform, bool notifyEditorAndMarkDirty);
        //获得该物体在父物体中的子物体序列的序号
        public extern int GetSiblingIndex();

        [FreeFunction]
        private static extern Transform FindRelativeTransformWithPath(Transform transform, string path, [UnityEngine.Internal.DefaultValue("false")] bool isActiveOnly);

        //通过名称查找Hierarchy面板中Transform
        public Transform Find(string n)
        {
            if (n == null)
                throw new ArgumentNullException("Name cannot be null");
            return FindRelativeTransformWithPath(this, n, false);
        }

        //*undocumented
        [NativeConditional("UNITY_EDITOR")]
        internal extern void SendTransformChangedScale();

        // 获得该物体在世界坐标系中的缩放值
        public extern Vector3 lossyScale
        {
            [NativeMethod("GetWorldScaleLossy")]
            get;
        }

        // 返回该物体是否是某个Transfrom 的子物体
        [FreeFunction("Internal_IsChildOrSameTransform", HasExplicitThis = true)]
        public extern bool IsChildOf([NotNull] Transform parent);

        // Has the transform changed since the last time the flag was set to 'false'?
        [NativeProperty("HasChangedDeprecated")]
        public extern bool hasChanged { get; set; }

        //*undocumented*
        [Obsolete("FindChild has been deprecated. Use Find instead (UnityUpgradable) -> Find([mscorlib] System.String)", false)]
        public Transform FindChild(string n) { return Find(n); }

        //*undocumented* Documented separately
        public IEnumerator GetEnumerator()
        {
            return new Transform.Enumerator(this);
        }

        private class Enumerator : IEnumerator
        {
            Transform outer;
            int currentIndex = -1;

            internal Enumerator(Transform outer)
            {
                this.outer = outer;
            }

            //*undocumented*
            public object Current
            {
                get { return outer.GetChild(currentIndex); }
            }

            //*undocumented*
            public bool MoveNext()
            {
                int childCount = outer.childCount;
                return ++currentIndex < childCount;
            }

            //*undocumented*
            public void Reset() { currentIndex = -1; }
        }

        // *undocumented* DEPRECATED
        [Obsolete("warning use Transform.Rotate instead.")]
        public extern void RotateAround(Vector3 axis, float angle);

        // *undocumented* DEPRECATED
        [Obsolete("warning use Transform.Rotate instead.")]
        public extern void RotateAroundLocal(Vector3 axis, float angle);

        // Get a transform child by index
        [NativeThrows]
        [FreeFunction("GetChild", HasExplicitThis = true)]
        public extern Transform GetChild(int index);

        //*undocumented* DEPRECATED
        [Obsolete("warning use Transform.childCount instead (UnityUpgradable) -> Transform.childCount", false)]
        [NativeMethod("GetChildrenCount")]
        public extern int GetChildCount();

        public int hierarchyCapacity
        {
            get { return internal_getHierarchyCapacity(); }
            set { internal_setHierarchyCapacity(value); }
        }

        [FreeFunction("GetHierarchyCapacity", HasExplicitThis = true)]
        private extern int internal_getHierarchyCapacity();

        [FreeFunction("SetHierarchyCapacity", HasExplicitThis = true)]
        private extern void internal_setHierarchyCapacity(int value);

        public int hierarchyCount { get { return internal_getHierarchyCount(); } }

        [FreeFunction("GetHierarchyCount", HasExplicitThis = true)]
        private extern int internal_getHierarchyCount();

        [NativeConditional("UNITY_EDITOR")]
        [FreeFunction("IsNonUniformScaleTransform", HasExplicitThis = true)]
        internal extern bool IsNonUniformScaleTransform();
    }
}
