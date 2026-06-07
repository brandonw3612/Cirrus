using Windows.Storage;
using Cirrus.Base.Services.Abstract;

namespace Cirrus.Services;

public class PreferenceAccessService : IPreferenceAccessService
{
    private readonly ApplicationDataContainer _localSettings;
    private readonly string _externalStorageDirectory;

    public PreferenceAccessService()
    {
        var settingsRoot = ApplicationData.Current.LocalSettings;
        _localSettings = settingsRoot.CreateContainer("Preferences", ApplicationDataCreateDisposition.Always);
        _externalStorageDirectory = Path.Join(ApplicationData.Current.LocalFolder.Path, "Preferences");
        Directory.CreateDirectory(_externalStorageDirectory);
    }

    public bool TryGetValue<T>(string preferencePath, out T? value, bool isLarge = false)
    {
        if (isLarge)
        {
            preferencePath = preferencePath.Replace('/', '.').Replace('\\', '.');
            var filePath = Path.Join(_externalStorageDirectory, preferencePath);
            if (typeof(T) != typeof(string) || !File.Exists(filePath))
            {
                value = default;
                return false;
            }
            value = File.ReadAllText(filePath) is T text ? text : default;
            return true;
        }
        if (_localSettings.Values.TryGetValue(preferencePath, out var v) && v is T v1)
        {
            value = v1;
            return true;
        }
        value = default;
        return false;
    }

    public void SetValue<T>(string preferencePath, T? value, bool isLarge = false)
    {
        if (isLarge)
        {
            preferencePath = preferencePath.Replace('/', '.').Replace('\\', '.');
            var filePath = Path.Join(_externalStorageDirectory, preferencePath);
            if (typeof(T) != typeof(string)) return;
            if (File.Exists(filePath)) File.Delete(filePath);
            File.WriteAllText(filePath, value as string);
            return;
        }
        _localSettings.Values[preferencePath] = value;
    }

    public void RemoveValue(string preferencePath, bool isLarge = false)
    {
        if (isLarge)
        {
            preferencePath = preferencePath.Replace('/', '.').Replace('\\', '.');
            var filePath = Path.Join(_externalStorageDirectory, preferencePath);
            if (File.Exists(filePath)) File.Delete(filePath);
            return;
        }
        if (!_localSettings.Values.ContainsKey(preferencePath)) return;
        _localSettings.Values.Remove(preferencePath);
    }
}