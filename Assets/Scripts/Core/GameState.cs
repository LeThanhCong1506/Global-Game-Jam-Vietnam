// File: Scripts/Core/GameState.cs
using System;

namespace Visioneer.MaskPuzzle
{
    [Serializable]
    public enum GameState
    {
        Playing,
        Win,
        Lose,
        Paused
    }
}
