//  PersistentAmongPlayModeAttribute.cs
//  http://kan-kikuchi.hatenablog.com/entry/PersistentAmongPlayModeAttribute
//
//  Created by kan.kikuchi on 2019.05.14.

using UnityEngine;
using System;

/// <summary>
/// エディタ再生中に変更した値をエディタ停止後もそのまま保持する属性
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class PersistentAmongPlayModeAttribute : PropertyAttribute {}