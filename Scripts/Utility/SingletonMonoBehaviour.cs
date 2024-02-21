/******************************************************************************/
/*!    \brief  シングルトンのMonoBehaviourジェネリック.
*******************************************************************************/

using UnityEngine;
namespace VMUnityLib
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        public static T Inst
        {
            get
            {
                if (!HasInstance())
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (!HasInstance())
                    {
                        Logger.Error(typeof(T) + "is nothing");
                    }
                }

                return instance;
            }
        }

        public static bool HasInstance()
        {
            return instance != null;
        }
    }
}