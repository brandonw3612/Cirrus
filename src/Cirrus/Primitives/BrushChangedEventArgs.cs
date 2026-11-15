using Microsoft.UI.Composition;

namespace Cirrus.Primitives;

public class BrushChangedEventArgs : EventArgs
{
    public required CompositionBrush? OldBrush { get; init; }
    public required CompositionBrush? NewBrush { get; init; }
}