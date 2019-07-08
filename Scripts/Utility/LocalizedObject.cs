/******************************************************************************/
/*!    \brief  特定言語のときだけ表示されるオブジェクト.
*******************************************************************************/

using UnityEngine;

public sealed class LocalizedObject : MonoBehaviour 
{
    /// <summary>
    /// 初期化.
    /// </summary>
    public void Start()
    {
        gameObject.SetActive(Application.systemLanguage != SystemLanguage.Japanese);
    }
}
