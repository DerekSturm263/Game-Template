/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Tamerlan Shakirov
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using RenownedGames.ExLib.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace RenownedGames.ExLibEditor
{
    public sealed class MemberData
    {
        private object declaringObject;
        private Type type;
        private MemberInfo memberInfo;

        internal MemberData(SerializedObject serializedObject, string memberPath)
        {
            declaringObject = serializedObject.targetObject;
            type = declaringObject.GetType();

            string[] pathSplit = memberPath.Split('.');
            Queue<string> paths = new Queue<string>(pathSplit);

            void Search()
            {
                string memberName = paths.Dequeue();
                if (memberName.Contains("data["))
                {
                    string result = Regex.Match(memberName, @"\d+").Value;
                    int index = Convert.ToInt32(result);

                    int count = 0;
                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        IEnumerable enumerable = fieldInfo.GetValue(declaringObject) as IEnumerable;
                        foreach (object element in enumerable)
                        {
                            if(element != null)
                            {
                                if (index == count)
                                {
                                    type = element.GetType();
                                    declaringObject = element;
                                    if (paths.Count > 0)
                                    {
                                        Search();
                                        break;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                }


                MemberInfo member = null;
                foreach (MemberInfo item in type.AllMembers())
                {
                    if (item.Name == memberName)
                    {
                        member = item;
                        break;
                    }
                }

                if (member != null)
                {
                    memberInfo = member;

                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        type = fieldInfo.FieldType;
                        Type ienum = type.GetInterface("IEnumerable`1");
                        if (ienum == null && paths.Count > 0)
                        {
                            UnityEngine.Debug.Log($"Member: {memberName}, Type: {type.Name}");
                            declaringObject = fieldInfo.GetValue(declaringObject);
                        }
                    }
                    else if (memberInfo is MethodInfo methodInfo)
                    {
                        type = methodInfo.ReturnType;
                    }
                }

                if (paths.Count > 0)
                {
                    Search();
                }
            }

            Search();
        }

        /// <summary>
        /// Object that declares the current nested member.
        /// </summary>
        public object GetDeclaringObject()
        {
            return declaringObject;
        }

        /// <summary>
        /// Type of member.
        /// </summary>
        public Type GetMemberType()
        {
            return type;
        }

        /// <summary>
        /// Obtains information about the attributes of a member and provides access to member metadata.
        /// </summary>
        public MemberInfo GetMemberInfo()
        {
            return memberInfo;
        }

        public override string ToString()
        {
            return $"Name: [{memberInfo.Name}], Declaring Object: [{declaringObject}], Type: [{type}]";
        }
    }
}
