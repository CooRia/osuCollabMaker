using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace osuCollabMaker;

public class Selection: INotifyPropertyChanged
{
    private string _name;
    private string _url;
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    public double X 
    { 
        get => _x;
        set { _x = value; OnPropertyChanged(); }
    }
    public double Y 
    { 
        get => _y;
        set { _y = value; OnPropertyChanged(); }
    }
    public double Width 
    { 
        get => _width;
        set { _width = value; OnPropertyChanged(); }
    }
    public double Height
    { 
        get => _height;
        set { _height = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string Url
    {
        get => _url;
        set { _url = value; OnPropertyChanged(); }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}