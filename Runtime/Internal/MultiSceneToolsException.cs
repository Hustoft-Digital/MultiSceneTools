using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.Serialization;

public class MultiSceneToolsException : System.Exception
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
