/******************************************************************************/
/*!    \brief  シングルトンのジェネリック.
*******************************************************************************/

namespace VMUnityLib
{
    public class Singleton<T> where T : class, new()
    {
        static T instance;
        public static T Inst
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }

                return instance;
            }
        }

    }
}