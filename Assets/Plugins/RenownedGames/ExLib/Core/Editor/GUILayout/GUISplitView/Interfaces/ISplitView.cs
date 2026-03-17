/* ================================================================
   ----------------------------------------------------------------
   Project   :   ExLib
   Company   :   Renowned Games
   Developer :   Tamerlan Shakirov
   ----------------------------------------------------------------
   Copyright 2022 Tamerlan Shakirov All rights reserved.
   ================================================================ */

namespace RenownedGames.ExLibEditor
{
    public interface ISplitView
    {
        void BeginSplitView();

        void Split();

        void EndSplitView();
    }
}