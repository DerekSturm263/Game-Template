/* ================================================================
   ---------------------------------------------------
   Project   :    Quick Code
   Publisher :    Renowned Games
   Author    :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using Microsoft.CSharp;
using RenownedGames.ExLibEditor;
using System;
using System.CodeDom.Compiler;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RenownedGames.QuickCodeEditor
{
    [Serializable]
    public sealed class QuickFunc
    {
        [SerializeField]
        [HideInInspector]
        private string classWrapper;

        [SerializeField]
        private string[] assemblies;

        [SerializeField]
        private string[] namespaces;

        [SerializeField]
        private bool hotCompile;

        // Stored required properties.
        private MethodInfo methodInfo;

        /// <summary>
        /// Constructor of QuickFunc.
        /// </summary>
        public QuickFunc(string classWrapper)
        {
            SetClassWrapper(classWrapper);

            assemblies = new string[4]
            {
                "System.dll",
                "System.Core.dll",
                typeof(Editor).Assembly.Location,
                typeof(GameObject).Assembly.Location
            };

            namespaces = new string[4]
            {
                "UnityEngine",
                "UnityEditor",
                "System.Collections",
                "System.Collections.Generic"
            };

            hotCompile = false;
        }

        /// <summary>
        /// Constructor of QuickFunc.
        /// </summary>
        public QuickFunc(QuickFunc func)
        {
            classWrapper = func.classWrapper;
            assemblies = func.assemblies;
            namespaces = func.namespaces;
            hotCompile = func.hotCompile;
            methodInfo = func.methodInfo;
        }

        /// <summary>
        /// Execute compiled function code.
        /// </summary>
        public void Execute()
        {
            methodInfo?.Invoke(null, null);
        }

        /// <summary>
        /// Compile specified function code.
        /// </summary>
        /// <param name="code">Text of code.</param>
        /// <returns>Compiler results.</returns>
        public CompilerResults Compile(string code)
        {
            CompilerParameters compilerParameters = new CompilerParameters(assemblies);
            compilerParameters.GenerateInMemory = true;
            compilerParameters.GenerateExecutable = false;

            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerResults compilerResult = codeProvider.CompileAssemblyFromSource(compilerParameters, string.Format(classWrapper, CollectNamespaces(), code));
            if (hotCompile || compilerResult.Errors.Count == 0)
            {
                Type type = compilerResult.CompiledAssembly.GetType("DefaultTemplate");
                methodInfo = type.GetMethod("Main");
            }

            for (int i = 0; i < compilerResult.Errors.Count; i++)
            {
                CompilerError error = compilerResult.Errors[i];
                if (error.IsWarning)
                {
                    Debug.LogWarning(error.ErrorText);
                }
                else
                {
                    Debug.LogError(error.ErrorText);
                }
            }

            return compilerResult;
        }

        /// <summary>
        /// Has compiled code?
        /// </summary>
        /// <returns>True if has compiled code. Otherwise false.</returns>
        public bool IsCompiled()
        {
            return methodInfo != null;
        }

        private string CollectNamespaces()
        {
            string collect = string.Empty;
            if(namespaces != null)
            {
                for (int i = 0; i < namespaces.Length; i++)
                {
                    collect += string.Format("using {0};\n", namespaces[i]);
                }
            }
            return collect;
        }

        #region [Static Methods]
        /// <summary>
        /// QuickFunc instance created from default class wrapper template.
        /// </summary>
        /// <returns>QuickFunc instance.</returns>
        public static QuickFunc CreateFromDefaultTemplate()
        {
            TextAsset classWrapper = EditorResources.Load<TextAsset>("ClassWrapper/DefaultTemplate.txt");
            if(classWrapper != null)
            {
                return new QuickFunc(classWrapper.text);
            }
            return null;
        }
        #endregion

        #region [Getter / Setter]
        public string GetClassWrapper()
        {
            return classWrapper;
        }

        public void SetClassWrapper(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("<b>Quick Code</b> exception: Class wrapper can not be null or empty!");
            }

            classWrapper = value;
        }

        public string[] GetAssemblies()
        {
            return assemblies;
        }

        public void SetAssemblies(string[] value)
        {
            assemblies = value;
        }

        public string[] GetNamespaces()
        {
            return namespaces;
        }

        public void SetNamespaces(string[] value)
        {
            namespaces = value;
        }

        public bool HotCompile()
        {
            return hotCompile;
        }

        public void HotCompile(bool value)
        {
            hotCompile = value;
        }
        #endregion
    }
}