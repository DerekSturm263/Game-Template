/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Tamerlan Shakirov
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

using UnityEditor;

namespace RenownedGames.ExLibEditor
{
    public static class SerializedObjectUtility
    {
        #region [Extension Methods]
        /// <summary>
        /// Find member metadata by path.
        /// </summary>
        /// <param name="memberPath">Path to member.</param>
        /// <returns>SerializedMemberData of member.</returns>
        public static MemberData FindMember(this SerializedObject serializedObject, string memberPath)
        {
            return new MemberData(serializedObject, memberPath);
        }
        #endregion
    }
}
