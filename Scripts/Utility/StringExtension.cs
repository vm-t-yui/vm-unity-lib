using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Stringの拡張クラス
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// スネークケースをアッパーキャメルケースに変換します
    /// </summary>
    public static string SnakeToUpperCamel(this string self)
    {
        if (string.IsNullOrEmpty(self)) return self;

        return self
            .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }
}
