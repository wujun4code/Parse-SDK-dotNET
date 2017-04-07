﻿using LeanCloud.Realtime.Internal;
using LeanCloud.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeanCloud.Realtime
{
    /// <summary>
    /// 
    /// </summary>
    [AVIMMessageClassName("_AVIMTypedMessage")]
    public class AVIMTypedMessage: AVIMMessage
    {
        public override string Serialize()
        {
            var result = AVRealtime.FreeStyleMessageClassingController.EncodeProperties(this);
            var resultStr = Json.Encode(result);
            return base.Serialize();
        }

        public override bool Validate(string msgStr)
        {
            try
            {
                var msg = Json.Parse(msgStr) as IDictionary<string, object>;
                return msg.ContainsKey(AVIMProtocol.LCTYPE);
            }
            catch
            {

            }
            return false;
        }
        public override IAVIMMessage Deserialize(string msgStr)
        {
            base.Deserialize(msgStr);
            var msg = Json.Parse(msgStr) as IDictionary<string, object>;
            var className = AVRealtime.FreeStyleMessageClassingController.GetClassName(this.GetType());
            var PropertyMappings = AVRealtime.FreeStyleMessageClassingController.GetPropertyMappings(className);
            var messageFieldProperties = PropertyMappings.Where(prop => msg.ContainsKey(prop.Value))
                  .Select(prop => Tuple.Create(ReflectionHelpers.GetProperty(this.GetType(), prop.Key), msg[prop.Value]));

            foreach (var property in messageFieldProperties)
            {
                property.Item1.SetValue(this, property.Item2, null);
            }
            return this;
        }
    }
}
