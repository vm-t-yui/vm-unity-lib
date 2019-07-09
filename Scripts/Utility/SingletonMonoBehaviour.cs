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
                if (instance == null)
                {
                    instance = (T)FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        Logger.Error(typeof(T) + "is nothing");
                    }
                }

                return instance;
            }
        }
    }
}