/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Tamerlan Shakirov
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace RenownedGames.ExLib.Reflection
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Iterate through all subclasses of specific type.
        /// </summary>
        /// <param name="directDescendants">Select only direct descendants or all subclasses.</param>
        public static IEnumerable<Type> Subclasses(this Type type, bool directDescendants = false)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                foreach (Type t in assembly.DefinedTypes)
                {
                    if ((directDescendants && t.BaseType == type) || t.IsSubclassOf(type))
                    {
                        yield return t;
                    }
                }
            }
        }

        /// <summary>
        /// Iterate through all subclasses of specific type.
        /// </summary>
        /// <param name="directDescendants">Select only direct descendants or all subclasses.</param>
        public static IEnumerable<Type> Subclasses(this Type type, Assembly assembly, bool directDescendants = false)
        {
            foreach (Type t in assembly.DefinedTypes)
            {
                if ((directDescendants && t.BaseType == type) || t.IsSubclassOf(type))
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Iterate through all the members of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<MemberInfo> AllMembers(this Type type)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (MemberInfo memberInfo in typeInfo.DeclaredMembers)
                {
                    yield return memberInfo;
                }
                type = type.BaseType;
            }
            while (type != null);
        }

        /// <summary>
        /// Iterate through all the fields of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<FieldInfo> AllFields(this Type type)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (FieldInfo fieldInfo in typeInfo.DeclaredFields)
                {
                    yield return fieldInfo;
                }
                type = type.BaseType;
            }
            while (type != null);
        }

        /// <summary>
        /// Iterate through all the methods of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<MethodInfo> AllMethods(this Type type)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (MethodInfo methodInfo in typeInfo.DeclaredMethods)
                {
                    yield return methodInfo;
                }
                type = type.BaseType;
            }
            while (type != null);
        }

        /// <summary>
        /// Iterate through all the properties of the current Type and all base types. 
        /// </summary>
        public static IEnumerable<PropertyInfo> AllProperties(this Type type)
        {
            do
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (PropertyInfo propertyInfo in typeInfo.DeclaredProperties)
                {
                    yield return propertyInfo;
                }
                type = type.BaseType;
            }
            while (type != null);
        }
    }
}