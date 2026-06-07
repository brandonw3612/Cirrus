using Cirrus.Commanding.Primitives;
using Cirrus.Generated.Attributes;
using CommunityToolkit.Mvvm.Input;

namespace Cirrus.Commanding;

public sealed partial class BlockEventPassCommand : CommandWrapper
{
    public BlockEventPassCommand()
    {
        InnerCommand = new RelayCommand<object>(static sender =>
        {
            if (sender is null) return;
            _ = SetHandled(sender, true);
        });
    }

    [DuckPropertySetter("Handled", Assemblies = [
        "Microsoft.WinUI",
        "Microsoft.InteractiveExperiences.Projection"
    ])]
    private static partial bool SetHandled(object args, bool handled);
}