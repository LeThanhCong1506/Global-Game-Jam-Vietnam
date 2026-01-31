// File: Scripts/Core/MaskType.cs
using System;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Enum defining all available mask types in the game.
    /// OFF = default view, A = walkable paths, B = traps, C = hidden objects (drains timer faster).
    /// </summary>
    [Serializable]
    public enum MaskType
    {
        Off = 0,    // Default view - no special visibility
        MaskA = 1,  // Shows walkable path tiles (highlight)
        MaskB = 2,  // Shows trap tiles (traps hidden otherwise)
        MaskC = 3   // Shows hidden objects (key, hidden bridge) - drains timer faster
    }
}
