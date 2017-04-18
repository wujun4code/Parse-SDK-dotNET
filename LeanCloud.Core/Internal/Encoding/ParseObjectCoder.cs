// Copyright (c) 2015-present, Parse, LLC.  All rights reserved.  This Source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this Source tree.  An additional grant of patent rights can be found in the PATENTS file in the same directory.

using System;
using System.Collections.Generic;

namespace LeanCloud.Core.Internal
{
    // TODO: (richardross) refactor entire parse coder interfaces.
    public class AVObjectCoder
    {
        private static readonly AVObjectCoder instance = new AVObjectCoder();
        public static AVObjectCoder Instance
        {
            get
            {
                return instance;
            }
        }

        // Prevent default constructor.
        private AVObjectCoder() { }

        public IDictionary<string, object> Encode<T>(T state,
            IDictionary<string, IAVFieldOperation> operations,
            AVEncoder encoder) where T : IObjectState
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in operations)
            {
                // Serialize the data
                var operation = pair.Value;

                result[pair.Key] = encoder.Encode(operation);
            }

            return result;
        }

        public IObjectState Decode(IDictionary<string, object> data,
            AVDecoder decoder)
        {
            IDictionary<string, object> serverData = new Dictionary<string, object>();
            var mutableData = new Dictionary<string, object>(data);
            string objectId = extractFromDictionary<string>(mutableData, "objectId", (obj) =>
            {
                return obj as string;
            });
            DateTime? createdAt = extractFromDictionary<DateTime?>(mutableData, "createdAt", (obj) =>
            {
                return AVDecoder.ParseDate(obj as string);
            });
            DateTime? updatedAt = extractFromDictionary<DateTime?>(mutableData, "updatedAt", (obj) =>
            {
                return AVDecoder.ParseDate(obj as string);
            });

            if (mutableData.ContainsKey("ACL"))
            {
                serverData["ACL"] = extractFromDictionary<AVACL>(mutableData, "ACL", (obj) =>
                {
                    return new AVACL(obj as IDictionary<string, object>);
                });
            }

            if (createdAt != null && updatedAt == null)
            {
                updatedAt = createdAt;
            }

            // Bring in the new server data.
            foreach (var pair in mutableData)
            {
                if (pair.Key == "__type" || pair.Key == "className")
                {
                    continue;
                }

                var value = pair.Value;
                serverData[pair.Key] = decoder.Decode(value);
            }

            return new MutableObjectState
            {
                ObjectId = objectId,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ServerData = serverData
            };
        }

        private T extractFromDictionary<T>(IDictionary<string, object> data, string key, Func<object, T> action)
        {
            T result = default(T);
            if (data.ContainsKey(key))
            {
                result = action(data[key]);
                data.Remove(key);
            }

            return result;
        }
    }
}
