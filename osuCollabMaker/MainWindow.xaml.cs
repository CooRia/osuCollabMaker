using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace osuCollabMaker;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    
    private Point _startPoint;
    private Rectangle? _currentRect;
    private readonly DispatcherTimer _drawTimer;
    private bool _longPress;
    private SelectionRectangleController _selectionController;
    
    //选区集合
    public ObservableCollection<Selection> Selections { get; } = new ObservableCollection<Selection>();
    private Selection? _selectedSelection;
    public Selection? SelectedSelection
    {
        get => _selectedSelection;
        set
        {
            _selectedSelection = value;
            OnPropertyChanged();
        }
    }
    
    // 实现 INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public MainWindow()
    {
        InitializeComponent();
        _drawTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.15) };
        _drawTimer.Tick += OnDrawTimerTick;

        _selectionController = new SelectionRectangleController(ImageCanvas);
        
        Selections.CollectionChanged += Selections_CollectionChanged;
    }
    
    private void OpenImage_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "图片文件|*.jpg;*.png;*.bmp|所有文件|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            LoadImage(openFileDialog.FileName);
        }
    }

    private void LoadImage(string path)
    {
        var bitmap = new BitmapImage(new Uri(path));
        MainImage.Source = bitmap;
        MainImage.Width = bitmap.PixelWidth;
        MainImage.Height = bitmap.PixelHeight;
        ImageCanvas.Width = bitmap.PixelWidth;
        ImageCanvas.Height = bitmap.PixelHeight;
    }
    

    private void ImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(ImageCanvas);
        
        _drawTimer.Start();
    }

    private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_longPress) return;

        var currentPoint = e.GetPosition(ImageCanvas);
        var x = Math.Min(_startPoint.X, currentPoint.X);
        var y = Math.Min(_startPoint.Y, currentPoint.Y);
    
        _currentRect.Width = Math.Abs(currentPoint.X - _startPoint.X);
        _currentRect.Height = Math.Abs(currentPoint.Y - _startPoint.Y);
    
        Canvas.SetLeft(_currentRect, x);
        Canvas.SetTop(_currentRect, y);
    }

    private void ImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_longPress)
        {
            var selection = new Selection
            {
                X = Canvas.GetLeft(_currentRect)/ImageCanvas.Width*100F,
                Y = Canvas.GetTop(_currentRect)/ImageCanvas.Height*100F,
                Width = _currentRect.Width/ImageCanvas.Width*100F,
                Height = _currentRect.Height/ImageCanvas.Height*100F,
                Name = "text",
                Url = "link"
            };
            Selections.Add(selection);
            SelectedSelection = selection;
            var rect = _currentRect;
            rect.Tag = selection;
            rect.PreviewMouseDown += (o, args) =>
            {
                if (args.RightButton == MouseButtonState.Pressed)
                {
                    Selections.Remove(selection);
                    SelectedSelection = null;
                    ImageCanvas.Children.Remove(rect);
                    args.Handled = true;
                    return;
                }
                if (args.LeftButton != MouseButtonState.Pressed)
                    return;
                SelectedSelection = rect.Tag as Selection;
            };
            _selectionController.InitSelectionRectangle(rect);

            _currentRect = null;
            
        }
        if(_drawTimer.IsEnabled) _drawTimer.Stop();
        _longPress = false;
    }
    
    private void OnDrawTimerTick(object? sender, EventArgs e)
    {
        _drawTimer.Stop();
        // 长按0.3s绘制矩形
        _currentRect = new Rectangle
        {
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 2,
            StrokeDashArray = [4, 2],
            Fill = new SolidColorBrush(Color.FromArgb(30, 0, 100, 250))
        };
    
        Canvas.SetLeft(_currentRect, _startPoint.X);
        Canvas.SetTop(_currentRect, _startPoint.Y);
        ImageCanvas.Children.Add(_currentRect);
        
        _longPress = true;
    }

    //缩放因子
    private const double ZoomFactor = 1.1;
    private const double MinScale = 0.1;
    private const double MaxScale = 10.0;
    private void Scroll_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var scale = ImageScaleTransform;
        Point center = e.GetPosition(ImageCanvas);
        double oldScale = scale.ScaleX;
        double newScale = e.Delta > 0 ? oldScale * ZoomFactor : oldScale / ZoomFactor;
        newScale = Math.Max(MinScale, Math.Min(MaxScale, newScale));

        var sv = ScrollViewerContainer;
        double offsetX = center.X * (newScale -  oldScale);
        double offsetY = center.Y * (newScale -  oldScale);
        scale.ScaleX = newScale;
        scale.ScaleY = newScale;
        sv.ScrollToHorizontalOffset(sv.HorizontalOffset + offsetX);
        sv.ScrollToVerticalOffset(sv.VerticalOffset + offsetY);
        
        e.Handled = true;
    }
    
    private void Selections_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Selection sel in e.NewItems)
            {
                sel.PropertyChanged += Selection_PropertyChanged; // 监听属性变化
            }
        }

        if (e.OldItems != null)
        {
            foreach (Selection sel in e.OldItems)
            {
                sel.PropertyChanged -= Selection_PropertyChanged;
            }
        }

        UpdateSelectionTextBox();
    }

    private void Selection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdateSelectionTextBox();
    }
    
    private void UpdateSelectionTextBox()
    {
        var lines = Selections.Select(s => 
            $"{s.X:F3}% {s.Y:F3}% {s.Width:F3}% {s.Height:F3}% {s.Url} {s.Name}");
        SelectionsTextBox.Text = string.Join("\n", lines);
    }

    
}