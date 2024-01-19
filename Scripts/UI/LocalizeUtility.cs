using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using I2.Loc;
using TMPro;

/// <summary>
/// ���[�J���C�Y�֗̕��@�\
/// </summary>
public static class LocalizeUtility
{
    /// <summary>
    /// �����[�J���C�Y��Term���ړ��͂ɑΉ�����SetTerm
    /// </summary>
    public static void SetTerm(TextMeshProUGUI ugui, Localize localize, string term, string debugText)
    {
        // Term���Ȃ������璼�ڕ�����ݒ�
        if (LocalizationManager.GetTermsList().Contains(term) == false)
        {
#if UNITY_EDITOR // �G�f�B�^�ł̓G�f�B�^�p�̃f�o�b�O�e�L�X�g����
            ugui.text = debugText + term;
#else
            ugui.text = player.TargetInteractArea.InteractUITerm;
#endif
        }
        else
        {
            localize.SetTerm(term);
        }
    }
}
