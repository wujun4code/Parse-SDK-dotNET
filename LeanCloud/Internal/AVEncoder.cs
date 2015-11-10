// Copyright (c) 2015-present, Parse, LLC  All rights reserved.  This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace LeanCloud.Internal {
  /// <summary>
  /// A <c>AVEncoder</c> can be used to transform objects such as <see cref="AVObject"/> into JSON
  /// data structures.
  /// </summary>
  /// <seealso cref="AVDecoder"/>
  internal abstract class AVEncoder {
    public static bool IsValidType(object value) {
      return value == null ||
          value.GetType().IsPrimitive() ||
          value is string ||
          value is AVObject ||
          value is AVACL ||
          value is AVFile ||
          value is AVGeoPoint ||
          value is AVRelationBase ||
          value is DateTime ||
          value is byte[] ||
          AVClient.ConvertTo<IDictionary<string, object>>(value) is IDictionary<string, object> ||
          AVClient.ConvertTo<IList<object>>(value) is IList<object>;
    }

    public object Encode(object value) {
      // If this object has a special encoding, encode it and return the
      // encoded object. Otherwise, just return the original object.
      if (value is DateTime) {
        return new Dictionary<string, object> {
          {"iso", ((DateTime)value).ToString(AVClient.DateFormatString)},
          {"__type", "Date"}
        };
      }

      var bytes = value as byte[];
      if (bytes != null) {
        return new Dictionary<string, object> {
          {"__type", "Bytes"},
          {"base64", Convert.ToBase64String(bytes)}
        };
      }

      var obj = value as AVObject;
      if (obj != null) {
        return EncodeAVObject(obj);
      }

      var jsonConvertible = value as IJsonConvertible;
      if (jsonConvertible != null) {
        return jsonConvertible.ToJSON();
      }

      var dict = AVClient.ConvertTo<IDictionary<string, object>>(value) as IDictionary<string, object>;
      if (dict != null) {
        var json = new Dictionary<string, object>();
        foreach (var pair in dict) {
          json[pair.Key] = Encode(pair.Value);
        }
        return json;
      }

      var list = AVClient.ConvertTo<IList<object>>(value) as IList<object>;
      if (list != null) {
        return EncodeList(list);
      }

      // TODO (hallucinogen): convert IAVFieldOperation to IJsonConvertible
      var operation = value as IAVFieldOperation;
      if (operation != null) {
        return operation.Encode();
      }

      return value;
    }

    protected abstract IDictionary<string, object> EncodeAVObject(AVObject value);

    private object EncodeList(IList<object> list) {
      var newArray = new List<object>();
#if UNITY
      // We need to explicitly cast `list` to `List<object>` rather than
      // `IList<object>` because IL2CPP is stricter than the usual Unity AOT compiler pipeline.
      if (PlatformHooks.IsCompiledByIL2CPP && list.GetType().IsArray) {
        list = new List<object>(list);
      }
#endif
      foreach (var item in list) {
        if (!IsValidType(item)) {
          throw new ArgumentException("Invalid type for value in an array");
        }
        newArray.Add(Encode(item));
      }
      return newArray;
    }
  }
}
