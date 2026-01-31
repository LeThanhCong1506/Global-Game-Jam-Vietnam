// File: Scripts/Core/MaskType.cs
using System;

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Enum defining all available mask types in the game.
    /// OFF = default view, A = walkable paths, B = traps, C = hidden objects.
    /// 
    /// Usage Rules:
    /// - Mask A: Can be used 2 times, each use allows 10 steps before auto-deactivating
    /// - Mask B: Can be used 1 time only  
    /// - Mask C: Drains timer faster while active
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
