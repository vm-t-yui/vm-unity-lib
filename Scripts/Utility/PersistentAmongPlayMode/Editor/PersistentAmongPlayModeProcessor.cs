//  PersistentAmongPlayModeProcessor.cs
//  http://kan-kikuchi.hatenablog.com/entry/PersistentAmongPlayModeAttribute
//
//  Created by kan.kikuchi on 2019.05.14.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
///   PersistentAmongPlayModeの処理を実際にするクラス
/// </summary>
[InitializeOnLoad] //エディター起動時に初期化されるように
public class PersistentAmongPlayModeProcessor
{

    //エディタ停止直前の値を記録するためのDict(InstanceIDとフィールド名をKeyにし、その値を設定する感じ)
    private static readonly Dictionary<int, Dictionary<string, object>> _valueDictDict = new Dictionary<int, Dictionary<string, object>>();

    //=================================================================================
    //初期化
    //=================================================================================

    static PersistentAmongPlayModeProcessor()
    {

        //プレイモードが変更された時の処理を設定
        EditorApplication.playModeStateChanged += state => {

            //終了ボタンを押した時に、その時の値を保存
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _valueDictDict.Clear();
                ExecuteProcessToAllMonoBehaviour(SaveValue);
            }
            //実際に終了した時(シーン再生前の値に戻った時)に、保存してた値を反映
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                ExecuteProcessToAllMonoBehaviour(ApplyValue);
            }
        };

    }

    //全MonoBehaviourを取得し、指定した処理を実行する
    private static void ExecuteProcessToAllMonoBehaviour(Action<MonoBehaviour> action)
    {
        Object.FindObjectsOfType(typeof(MonoBehaviour)).ToList().ForEach(o => action((MonoBehaviour)o));
    }

    //=================================================================================
    //共通
    //=================================================================================

    //PersistentAmongPlayModeが付いてる全フィールドに処理を実行する
    private static void ExecuteProcessToAllPersistentAmongPlayModeField(MonoBehaviour component, Action<FieldInfo> action)
    {
        //Publicとそれ以外のフィールドに対して処理を実行
        ExecuteProcessToAllPersistentAmongPlayModeField(component, action, BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
        ExecuteProcessToAllPersistentAmongPlayModeField(component, action, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
    }

    //PersistentAmongPlayModeが付いてる、かつ、BindingFlagsで指定した全フィールドに処理を実行する
    private static void ExecuteProcessToAllPersistentAmongPlayModeField(MonoBehaviour component, Action<FieldInfo> action, BindingFlags bindingFlags)
    {
        //コンポーネントから全フィールドを取得
        component.GetType()
          .GetFields(bindingFlags)
          .ToList()
          .ForEach(fieldInfo => {
          //PersistentAmongPlayModeが付いてるものにだけ処理を実行
          if (fieldInfo.GetCustomAttributes(typeof(PersistentAmongPlayModeAttribute), true).Length != 0)
                  action(fieldInfo);
          });
    }

    //=================================================================================
    //保存
    //=================================================================================

    //PersistentAmongPlayModeの属性が付いた値を保存
    private static void SaveValue(MonoBehaviour component)
    {
        //各フィールドの値を保存するためのDict
        var valueDict = new Dictionary<string, object>();

        //PersistentAmongPlayModeの属性が付いた値だけをDictに登録
        ExecuteProcessToAllPersistentAmongPlayModeField(component, fieldInfo => { valueDict.Add(fieldInfo.Name, fieldInfo.GetValue(component)); });

        //インスタンスIDをKeyにして、値をまとめたDictを追加
        _valueDictDict.Add(component.GetInstanceID(), valueDict);
    }

    //=================================================================================
    //反映
    //=================================================================================

    //PersistentAmongPlayModeの属性が付いた値を反映
    private static void ApplyValue(MonoBehaviour component)
    {
        //終了ボタンを押した時に存在しなかった(シーン再生中に削除されたとかで)やつはスルー
        if (!_valueDictDict.ContainsKey(component.GetInstanceID()))
        {
            return;
        }

        //各フィールドの値を保存したDictを取得
        var valueDict = _valueDictDict[component.GetInstanceID()];

        //PersistentAmongPlayModeの属性が付いた値だけ反映
        var isChangedValue = false; //値に変更があったか

        ExecuteProcessToAllPersistentAmongPlayModeField(component, fieldInfo => {
                var fieldName = fieldInfo.Name;

                if (valueDict.ContainsKey(fieldName))
                {
                    //値が変化したかを判定(値の変更を直接比較するとうまく行かないのでStringにして比較)
                    var value = fieldInfo.GetValue(component);
                    var dictValue = valueDict[fieldName];
                    if (value != null && dictValue != null)
                    {
                        isChangedValue = isChangedValue || value.ToString() != dictValue.ToString();

                        //値の反映
                        fieldInfo.SetValue(component, valueDict[fieldName]);
                    }
                }
        });

        //値の変更があったら保存出来るようにするため、シーンに変更があったこと(米印)を設定
        if (isChangedValue)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}