using CommunityToolkit.Mvvm.ComponentModel;

namespace Cirrus.Playback.PlaybackServices;

public partial class DualNodeFlow<TTrackIdentifier>(Action<AudioNode<TTrackIdentifier>> attachAction,
        Action<AudioNode<TTrackIdentifier>> detachAction)
    : ObservableObject where TTrackIdentifier : notnull
{
    private readonly AudioNode<TTrackIdentifier>?[] _nodes = new AudioNode<TTrackIdentifier>?[2];
    private int _validNodeCount;
    private int _currentIndex;
    private readonly SemaphoreSlim _flowSemaphore = new(1);

    public (AudioNode<TTrackIdentifier>? Current, AudioNode<TTrackIdentifier>? Next) Nodes
    {
        get
        {
            _flowSemaphore.Wait();
            try
            {
                return (_nodes[_currentIndex], _nodes[1 - _currentIndex]);
            }
            finally
            {
                _flowSemaphore.Release();
            }
        }
    }

    public async Task EnterAsync(AudioNode<TTrackIdentifier> node)
    {
        await _flowSemaphore.WaitAsync();
        try
        {
            switch (_validNodeCount)
            {
                case 0:
                {
                    _nodes[_currentIndex] = node;
                    attachAction.Invoke(node);
                    _validNodeCount = 1;
                    break;
                }
                case 1:
                {
                    _nodes[1 - _currentIndex] = node;
                    attachAction.Invoke(node);
                    _validNodeCount = 2;
                    break;
                }
                case 2:
                {
                    detachAction.Invoke(_nodes[1 - _currentIndex]!);
                    _nodes[1 - _currentIndex] = node;
                    attachAction.Invoke(node);
                    break;
                }
            }
        }
        finally
        {
            _flowSemaphore.Release();
        }
    }

    public async Task FlowAsync()
    {
        await _flowSemaphore.WaitAsync();
        try
        {
            switch (_validNodeCount)
            {
                case 1:
                {
                    detachAction.Invoke(_nodes[_currentIndex]!);
                    _nodes[_currentIndex] = null;
                    _validNodeCount = 0;
                    break;
                }
                case 2:
                {
                    detachAction.Invoke(_nodes[_currentIndex]!);
                    _nodes[_currentIndex] = null;
                    _currentIndex = 1 - _currentIndex;
                    _validNodeCount = 1;
                    break;
                }
            }
        }
        finally
        {
            _flowSemaphore.Release();
        }
    }

    public async Task ResetAsync()
    {
        await _flowSemaphore.WaitAsync();
        try
        {
            if (_nodes[0] is { } node0) detachAction.Invoke(node0);
            if (_nodes[1] is { } node1) detachAction.Invoke(node1);
            _nodes[0] = _nodes[1] = null;
            _validNodeCount = 0;
        }
        finally
        {
            _flowSemaphore.Release();
        }
    }
}