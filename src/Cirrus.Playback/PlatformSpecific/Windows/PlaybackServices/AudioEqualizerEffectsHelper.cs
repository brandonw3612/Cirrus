using System.Collections.ObjectModel;
using Windows.Media.Audio;

namespace Cirrus.Playback.PlaybackServices;

/// <summary>
/// Helper class for equalizer effects in AudioGraph.
/// </summary>
public static class AudioEqualizerEffectsHelper
{
    /// <summary>
    /// The maximum gain in dB.
    /// </summary>
    private const double MaxGainAbsolute = 12d;

    /// <summary>
    /// Equalizer band frequency centers.
    /// </summary>
    private static readonly double[] FrequencyCenters =
        [32d, 64d, 125d, 250d, 500d, 1000d, 2000d, 4000d, 8000d, 16000d];

    /// <summary>
    /// Equalizer effect gains for preset effects.
    /// </summary>
    private static readonly ReadOnlyDictionary<string, double[]> PresetEffectGains = new(new Dictionary<string, double[]>
        {
            ["Off"] = [+0.00, +0.00, +0.00, +0.00, +0.00, +0.00, +0.00, +0.00, +0.00, +0.00],
            ["Acoustic"] = [+5.00, +4.90, +3.95, +1.05, +2.15, +1.75, +3.50, +4.10, +3.55, +2.15],
            ["BassBooster"] = [+5.50, +4.25, +3.50, +2.50, +1.25, +0.00, +0.00, +0.00, +0.00, +0.00],
            ["BassReducer"] = [-5.50, -4.25, -3.50, -2.50, -1.25, +0.00, +0.00, +0.00, +0.00, +0.00],
            ["Classical"] = [+4.75, +3.75, +3.00, +2.50, -1.50, -1.50, +0.00, +2.25, +3.25, +3.75],
            ["Dance"] = [+3.57, +6.55, +4.99, +0.00, +1.92, +3.65, +5.15, +4.54, +3.59, +0.00],
            ["Deep"] = [+4.95, +3.55, +1.75, +1.00, +2.85, +2.50, +1.45, -2.15, -3.55, -4.60],
            ["Electronic"] = [+4.25, +3.80, +1.20, +0.00, -2.15, +2.25, +0.85, +1.25, +3.95, +4.80],
            ["HipHop"] = [+5.00, +4.25, +1.50, +3.00, -1.00, -1.00, +1.50, -0.50, +2.00, +3.00],
            ["Jazz"] = [+4.00, +3.00, +1.50, +2.25, -1.50, -1.50, +0.00, +1.50, +3.00, +3.75],
            ["Latin"] = [+4.50, +3.00, +0.00, +0.00, -1.50, -1.50, -1.50, +0.00, +3.00, +4.50],
            ["Loudness"] = [+6.00, +4.00, +0.00, +0.00, -2.00, +0.00, -1.00, -5.00, +5.00, +1.00],
            ["Lounge"] = [-3.00, -1.50, -0.50, +1.50, +4.00, +2.50, +0.00, -1.50, +2.00, +1.00],
            ["Piano"] = [+3.00, +2.00, +0.00, +2.50, +3.00, +1.50, +3.50, +4.50, +3.00, +3.50],
            ["Pop"] = [-1.50, -1.00, +0.00, +2.00, +4.00, +4.00, +2.00, +0.00, -1.00, -1.50],
            ["RNB"] = [+2.62, +6.92, +5.65, +1.33, -2.19, -1.50, +2.32, +2.65, +3.00, +3.75],
            ["Rock"] = [+5.00, +4.00, +3.00, +1.50, -0.50, -1.00, +0.50, +2.50, +3.50, +4.50],
            ["SmallSpeakers"] = [+5.50, +4.25, +3.50, +2.50, +1.25, +0.00, -1.25, -2.50, -3.50, -4.25],
            ["SpokenWord"] = [-3.46, -0.47, +0.00, +0.69, +3.46, +4.61, +4.84, +4.28, +2.54, +0.00],
            ["TrebleBooster"] = [+0.00, +0.00, +0.00, +0.00, +0.00, +1.25, +2.50, +3.50, +4.25, +5.50],
            ["TrebleReducer"] = [+0.00, +0.00, +0.00, +0.00, +0.00, -1.25, -2.50, -3.50, -4.25, -5.50],
            ["VocalBooster"] = [-1.50, -3.00, -3.00, +1.50, +3.75, +3.75, +3.00, +1.50, +0.00, -1.50],
        });

    /// <summary>
    /// Build equalizer effect definitions in graph initialization.
    /// </summary>
    /// <param name="graph">AudioGraph where we create the effects.</param>
    /// <returns>Equalizer effect definitions.</returns>
    public static List<EqualizerEffectDefinition> BuildEqualizerEffectDefinitions(AudioGraph graph)
    {
        List<EqualizerEffectDefinition> result = new();
        for (var i = 0; i < 3; i++)
        {
            EqualizerEffectDefinition definition = new(graph);
            for (var j = 0; j < 4; j++)
            {
                definition.Bands[j].FrequencyCenter = FrequencyCenters[3 * i + j];
                // Bandwidth should fall in [0.1, 2].
                // They say, Q, typically, should be sqrt(2)
                // And Q = sqrt(2^B) / (2^B - 1)
                // So I guess B = 1.
                // TODO: Is this correct? I have no idea.
                definition.Bands[j].Bandwidth = 1d;
                // Default to 1, which means no effect on the band.
                definition.Bands[j].Gain = 1d;
            }
            result.Add(definition);
        }
        return result;
    }

    /// <summary>
    /// Update equalizer effect gains.
    /// </summary>
    /// <param name="effectDefinitions">Equalizer effect definitions to update.</param>
    /// <param name="effectName">Name of the effect. Preset or custom.</param>
    /// <param name="customEffectInput">Custom effect band gain input.</param>
    public static void UpdateEffectGains(List<EqualizerEffectDefinition> effectDefinitions, string effectName,
        double[]? customEffectInput)
    {
        var gains = PresetEffectGains.TryGetValue(effectName, out var preset)
            ? ParseFromTenBands(preset)
            : ParseFromTenBands(effectName is "Custom"
                ? customEffectInput ?? Enumerable.Repeat(0d, 10).ToArray()
                : PresetEffectGains["Off"]);
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                effectDefinitions[i].Bands[j].Gain = gains[i, j];
            }
        }
        return;

        // Parse gains from 10 bands to 3x4 bands.
        double[,] ParseFromTenBands(double[] inputGains)
        {
            var result = new double[3, 4];
            if (inputGains is not { Length: 10 }) return result;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    var originalGain = inputGains[i * 3 + j];
                    // These bands are separated into 2 definitions.
                    // TODO: Correct? No idea, either.
                    if (i * 3 + j is 3 or 6) originalGain /= 2;
                    // Output gain should fall in [10^-0.9, 10^0.9].
                    result[i, j] = Math.Pow(10, originalGain / MaxGainAbsolute * 0.9d);
                }
            }
            return result;
        }
    }
}