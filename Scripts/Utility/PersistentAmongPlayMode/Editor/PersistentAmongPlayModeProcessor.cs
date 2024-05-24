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

    // トランスフォーム保存用データ
    struct TransformData
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
    }

    // エディタ停止直前のTransformを記録するためのDict。キーはグローバルオブジェクトのID
    private static readonly Dictionary<string, TransformData> _transformDataDict = new Dictionary<string, TransformData>();
    private static readonly Dictionary<string, Transform>     _transformDict = new Dictionary<string, Transform>();

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

                // transform適用
                SaveTransformValue();
                _transformDict.Clear();
            }
            //実際に終了した時(シーン再生前の値に戻った時)に、保存してた値を反映
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                ExecuteProcessToAllMonoBehaviour(ApplyValue);

                // transform適用
                ApplyTransformValue();
                _transformDataDict.Clear();
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

    //トランスフォームを実行中に保存
    [MenuItem("CONTEXT/Transform/実行中のTransformを終了後も保持")]
    private static void AddTransform(MenuCommand menuCommand)
    {
        // 実行中以外は無視
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        var target = menuCommand.context as Transform;

        var id = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
        if (!_transformDict.ContainsKey(id))
        {
            _transformDataDict.Add(id, new TransformData());
            _transformDict.Add(id, target);
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    private static void SaveTransformValue()
    {
        if(_transformDict.Count == 0)
        {
            return;
        }

        var sceneObjects = GameObject.FindObjectsOfType<Transform>();
        var sceneObjectIds = new GlobalObjectId[sceneObjects.Length];
        GlobalObjectId.GetGlobalObjectIdsSlow(sceneObjects, sceneObjectIds);

        for (var i = 0; i < sceneObjectIds.Length; i++)
        {
            var idString = sceneObjectIds[i].ToString();
            var target = sceneObjects[i];
            if (_transformDict.ContainsKey(idString))
            {
                TransformData targetTransformData = new TransformData();

                targetTransformData.pos = target.position;
                targetTransformData.scale = target.localScale;
                targetTransformData.rot = target.rotation;

                _transformDataDict[idString] = targetTransformData;
            }
        }
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
                        var changed = value.ToString() != dictValue.ToString();

                        //値の反映
                        if(changed)
                        {
                            fieldInfo.SetValue(component, valueDict[fieldName]);
                            isChangedValue = true;
                        }
                    }
                }
        });

        //値の変更があったら保存出来るようにするため、シーンに変更があったこと(米印)を設定
        if (isChangedValue)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
    }

    /// <summary>
    /// Transformの反映
    /// </summary>
    private static void ApplyTransformValue()
    {
        if(_transformDataDict.Count == 0)
        {
            return;
        }

        //PersistentAmongPlayModeの属性が付いた値だけ反映
        var isChangedValue = false; //値に変更があったか

        var sceneObjects = GameObject.FindObjectsOfType<Transform>();
        var sceneObjectIds = new GlobalObjectId[sceneObjects.Length];
        GlobalObjectId.GetGlobalObjectIdsSlow(sceneObjects, sceneObjectIds);

        for (var i = 0; i < sceneObjectIds.Length; i++)
        {
            var idString = sceneObjectIds[i].ToString();
            var target = sceneObjects[i];
            if (_transformDataDict.ContainsKey(idString))
            {
                var value = _transformDataDict[idString];

                if(target.position != value.pos
                    || target.localScale != value.scale
                    || target.rotation != value.rot)
                {
                    target.position = value.pos;
                    target.localScale = value.scale;
                    target.rotation = value.rot;
                    isChangedValue = true;
                }
            }
        }

        //値の変更があったら保存出来るようにするため、シーンに変更があったこと(米印)を設定
        if (isChangedValue)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}