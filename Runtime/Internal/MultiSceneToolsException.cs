// *   Multi Scene Tools For Unity
// *
// *   Copyright (C) 2024 Hustoft Digital
// *
// *   Licensed under the Apache License, Version 2.0 (the "License");
// *   you may not use this file except in compliance with the License.
// *   You may obtain a copy of the License at
// *
// *       http://www.apache.org/licenses/LICENSE-2.0
// *
// *   Unless required by applicable law or agreed to in writing, software
// *   distributed under the License is distributed on an "AS IS" BASIS,
// *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// *   See the License for the specific language governing permissions and
// *   limitations under the License.

using UnityEditor;
using System;
using System.Runtime.Serialization;

public class MultiSceneToolsException : Exception
{
    const int Result = unchecked ((int)0x80004003);
#pragma warning disable 169


    public MultiSceneToolsException() :  base("A MultiSceneTools Runtime error occurred!")
    {
        HResult = Result;
    }

    public MultiSceneToolsException(string message)
        : base(message)
    {
        HResult = Result;
    }

    public MultiSceneToolsException(string message, UnityEngine.Object context) : base(message)
    {
        HResult = Result;

        #if UNITY_EDITOR
            EditorGUIUtility.PingObject(context);
        #endif
    }


    public MultiSceneToolsException(string message, Exception innerException)
        : base(message, innerException)
    {
        HResult = Result;
    }

    protected MultiSceneToolsException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
