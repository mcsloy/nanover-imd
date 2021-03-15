// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Google.Protobuf.WellKnownTypes;
using JetBrains.Annotations;
using Narupa.Core;
using Narupa.Frame;
using Narupa.Frame.Event;
using Narupa.Protocol;
using Narupa.Protocol.Trajectory;
using Newtonsoft.Json;

namespace Narupa.Grpc.Frame
{
    /// <summary>
    /// Conversion methods for converting <see cref="FrameData" />, the standard
    /// protocol for transmitting frame information, into a <see cref="Frame" />
    /// representation used by the frontend.
    /// </summary>
    public static class AliasedFrameConverter
    {
        /// <summary>
        /// Convert data into a <see cref="Frame" />.
        /// </summary>
        /// <param name="previousFrame">
        /// A previous frame, from which to copy existing arrays if they exist.
        /// </param>
        /// 

        public static Narupa.Frame.Frame ApplyAliasToFrame(string alias, Narupa.Frame.Frame frame)
        {
            FrameData aliasedFrameData = BuildAliasedFrameData(alias, frame);

            return FrameConverter.ConvertFrame(aliasedFrameData, frame).Frame;
        }

        public static FrameData BuildAliasedFrameData(string alias, Narupa.Frame.Frame frame)
        {
            FrameData frameData = new FrameData();
            Narupa.Frame.Frame newFrame = new Narupa.Frame.Frame();

            List<object> valueKeys = GuessValueKeys(frameData);
            List<object> arrayKeys = GuessArrayKeys(frameData);

            foreach (string id in valueKeys)
            {
                string aliasedId = alias + id;

                if (frame.Data.Keys.Contains(aliasedId))
                {
                    object value = frame.Data[aliasedId];
                    frameData.Add(id, value);
                }
            }

            foreach (string id in arrayKeys)
            {
                string aliasedId = alias + id;

                if (frame.Data.Keys.Contains(aliasedId))
                {
                    object array = frame.Data[aliasedId];
                    frameData.Add(id, array);
                }
            }

            return frameData;
        }

        private static void AddToFrameData(string id, object obj, FrameData frameData) 
        {
            frameData.Add(id, obj);
        }

        private static List<object> GuessValueKeys(FrameData frameData)
        {
            List<object> valueKeys = new List<object>();
            FieldInfo[] allFields = frameData.GetType().GetFields();

            foreach (FieldInfo field in allFields)
                if (field.Name.EndsWith("ValueKey"))
                    valueKeys.Add(field.GetValue(frameData));

            return valueKeys;
        }

        private static List<object> GuessArrayKeys(FrameData frameData)
        {
            List<object> arrayKeys = new List<object>();
            FieldInfo[] allFields = frameData.GetType().GetFields();

            foreach (FieldInfo field in allFields)
                if (field.Name.EndsWith("ArrayKey"))
                    arrayKeys.Add(field.GetValue(frameData));

            return arrayKeys;
        }

        /// <summary>
        /// Deserialize a protobuf <see cref="ValueArray" /> to a C# object, using a
        /// converter if defined.
        /// </summary>
        private static object ProcessValue(string key, object value)
        {
            if (key == FrameData.ParticleCountValueKey |
                key == FrameData.ResidueCountValueKey |
                key == FrameData.ChainCountValueKey)
            {
                return value.ToProtobufValue().ToInt();
            }
            else
                return value;
        }

        private static object ProcessArray(string key, object array) 
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                ValueArray valueArray = new ValueArray();

                string json = JsonConvert.SerializeObject(array);

                if (key == FrameData.BondArrayKey)
                {
                    IndexArray indexValues = IndexArray.Parser.ParseJson(json);
                    valueArray.IndexValues = indexValues;

                    return valueArray.ToBondPairArray();
                }
                if (key == FrameData.ParticleElementArrayKey)
                {
                    StringArray stringValues = StringArray.Parser.ParseFrom(ms);
                    valueArray.StringValues = stringValues;

                    return valueArray.ToElementArray();
                }
                if (key == FrameData.ParticlePositionArrayKey)
                {
                    FloatArray floatValues = FloatArray.Parser.ParseFrom(ms);
                    valueArray.FloatValues = floatValues;

                    return valueArray.ToVector3Array();
                }
                else
                    return array; 
            }
        }
    }
}
