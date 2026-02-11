using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClientPlugin;

public class Config : INotifyPropertyChanged
{
    #region Options

    private int checkIntervalSeconds = 3;

    #endregion

    #region User interface

    public readonly string Title = "AutoJump";

    [Slider(1f, 10f, 1f, SliderAttribute.SliderType.Integer, description: "How often to check if jump drives are charged (in seconds)")]
    public int CheckIntervalSeconds
    {
        get => checkIntervalSeconds;
        set => SetField(ref checkIntervalSeconds, value);
    }

    #endregion

    #region Property change notification boilerplate

    public static readonly Config Default = new Config();
    public static readonly Config Current = ConfigStorage.Load();

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
