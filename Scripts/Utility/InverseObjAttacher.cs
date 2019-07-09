/******************************************************************************/
/*!    \brief  モデルの頂点を逆配置にしてアタッチする.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public sealed class InverseObjAttacher : MonoBehaviour
    {
        /// <summary>
        /// 計算後Update.
        /// </summary>
        void LateUpdate()
        {
            Quaternion inv = transform.parent.rotation;

            /*
            // 虚数部をZX平面で鏡面反射.
            Vector3 v = new Vector3(inv.x, inv.y, inv.z);

            Vector3 n = new Vector3(0,1,0);

            v = GetPlaneMirrorMatrix(n).MultiplyVector(v);

            inv.x = v.x;
            inv.y = v.y;
            inv.z = v.z;
            */

            transform.parent.rotation = inv;
        }

        Matrix4x4 GetPlaneMirrorMatrix(Vector3 n)
        {
            Matrix4x4 ret = Matrix4x4.identity;

            ret.m00 = 1 - 2 * n.x * n.x;
            ret.m01 = -2 * n.x * n.y;
            ret.m02 = -2 * n.x * n.z;
            ret.m03 = 0;

            ret.m10 = -2 * n.y * n.x;
            ret.m11 = 1 - 2 * n.y * n.y;
            ret.m12 = -2 * n.y * n.z;
            ret.m13 = 0;

            ret.m20 = -2 * n.z * n.x;
            ret.m21 = -2 * n.z * n.y;
            ret.m22 = 1 - 2 * n.z * n.z;
            ret.m23 = 0;

            ret.m30 = 0;
            ret.m31 = 0;
            ret.m32 = 0;
            ret.m33 = 1;

            return ret;
        }
    }
}
