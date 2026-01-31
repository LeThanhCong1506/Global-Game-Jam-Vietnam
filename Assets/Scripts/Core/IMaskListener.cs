// File: Scripts/Core/IMaskListener.cs

namespace Visioneer.MaskPuzzle
{
    /// <summary>
    /// Interface for objects that need to react when the mask changes.
    /// Implement this to receive mask change notifications.
    /// </summary>
    public interface IMaskListener
    {
        void OnMaskChanged(MaskType newMask);
    }
}
