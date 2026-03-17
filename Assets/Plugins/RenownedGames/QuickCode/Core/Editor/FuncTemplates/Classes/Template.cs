/* ================================================================
   ---------------------------------------------------
   Project   :    Quick Code
   Publisher :    Renowned Games
   Author    :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using UnityEngine;

namespace RenownedGames.QuickCodeEditor
{
    [System.Serializable]
    public class Template
    {
        [SerializeField]
        private string name;

        [SerializeField]
        private string code;

        [SerializeField]
        private QuickFunc quickFunc;

        public Template(string name, string code, QuickFunc quickFunc)
        {
            this.name = name;
            this.code = code;
            this.quickFunc = quickFunc;
        }

        public void Run()
        {
            if (!quickFunc.IsCompiled())
            {
                quickFunc.Compile(code);
            }
            quickFunc.Execute();
        }

        #region [Getter / Setter]
        public string GetName()
        {
            return name;
        }

        public void SetName(string value)
        {
            name = value;
        }

        public string GetCode()
        {
            return code;
        }

        public void SetCode(string value)
        {
            code = value;
        }

        public QuickFunc GetQuickFunc()
        {
            return quickFunc;
        }

        public void SetQuickFunc(QuickFunc value)
        {
            quickFunc = value;
        }
        #endregion
    }
}