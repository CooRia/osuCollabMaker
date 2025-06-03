using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace osuCollabMaker;

public class CanvasAdorner : Adorner
{
    //4条边
    Thumb _leftThumb, _topThumb, _rightThumb, _bottomThumb;
    //4个角
    Thumb _lefTopThumb, _rightTopThumb, _rightBottomThumb, _leftbottomThumb;
    Ellipse _centerPoint;
    private const double thumbSize = 6;
    private const double centerPointRadius = 3;
    Grid _grid;
    UIElement _adornedElement;
    UIElement _parentElement;
    //绑定的选区数据
    private readonly Selection _selection;
    public CanvasAdorner(UIElement adornedElement, UIElement adornedParentElement, Selection selection) : base(adornedElement)
    {
        _adornedElement = adornedElement;
        _parentElement = adornedParentElement;
        _selection =  selection;

        // 中心点
        _centerPoint = new Ellipse
        {
            Width = centerPointRadius * 2,
            Height = centerPointRadius * 2,
            Fill = Brushes.SkyBlue,
            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")),
            StrokeThickness = 2,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        //初始化thumb缩放手柄
        _leftThumb = CreateThumb(HorizontalAlignment.Left, VerticalAlignment.Center, Cursors.SizeWE);
        _topThumb = CreateThumb(HorizontalAlignment.Center, VerticalAlignment.Top, Cursors.SizeNS);
        _rightThumb = CreateThumb(HorizontalAlignment.Right, VerticalAlignment.Center, Cursors.SizeWE);
        _bottomThumb = CreateThumb(HorizontalAlignment.Center, VerticalAlignment.Bottom, Cursors.SizeNS);

        _lefTopThumb = CreateThumb(HorizontalAlignment.Left, VerticalAlignment.Top, Cursors.SizeNWSE);
        _rightTopThumb = CreateThumb(HorizontalAlignment.Right, VerticalAlignment.Top, Cursors.SizeNESW);
        _rightBottomThumb = CreateThumb(HorizontalAlignment.Right, VerticalAlignment.Bottom, Cursors.SizeNWSE);
        _leftbottomThumb = CreateThumb(HorizontalAlignment.Left, VerticalAlignment.Bottom, Cursors.SizeNESW);

        _grid = new Grid();
        _grid.Children.Add(_leftThumb);
        _grid.Children.Add(_topThumb);
        _grid.Children.Add(_rightThumb);
        _grid.Children.Add(_bottomThumb);
        _grid.Children.Add(_lefTopThumb);
        _grid.Children.Add(_rightTopThumb);
        _grid.Children.Add(_rightBottomThumb);
        _grid.Children.Add(_leftbottomThumb);
        AddVisualChild(_grid);

        // 绘制中心点和x，y坐标轴
        _grid.Children.Add(_centerPoint);
        DrawAxisWithArrow(0,15,0,0,isXAxis: true);
        DrawAxisWithArrow(0, 0, 10, 25, isXAxis: false);
    }
    protected override Visual GetVisualChild(int index)
    {
        return _grid;
    }
    protected override int VisualChildrenCount
    {
        get
        {
            return 1;
        }
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        _grid.Arrange(new Rect(new Point(-_leftThumb.Width / 2, -_leftThumb.Height / 2), new Size(finalSize.Width + _leftThumb.Width, finalSize.Height + _leftThumb.Height)));
        return finalSize;
    }
    
    private Thumb CreateThumb(HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, Cursor cursor)
    {
        var thumb = new Thumb 
        {
            Width = thumbSize,
            Height = thumbSize,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Background = Brushes.Green,
            Cursor = cursor,
            Template = new ControlTemplate(typeof(Thumb))
            {
                VisualTree = GetFactory(new SolidColorBrush(Colors.White))
            },
        };
        thumb.DragDelta += Thumb_DragDelta;
        return thumb;
    }
    // 缩放手柄
    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        var c = _adornedElement as FrameworkElement;
        var p = _parentElement as FrameworkElement;
        var thumb = sender as FrameworkElement;
        double left, top, width, height;
        if (thumb.HorizontalAlignment == HorizontalAlignment.Left)
        {
            left = double.IsNaN(Canvas.GetLeft(c)) ? 0 : Canvas.GetLeft(c) + e.HorizontalChange;
            width = c.Width - e.HorizontalChange;
            // 不超出 Canvas 左边界
            if (left < 0)
            {
                width += left;
                left = 0;
            }
        }
        else
        {
            left = Canvas.GetLeft(c);
            width = c.Width + e.HorizontalChange;

            // 不超出 Canvas 右边界
            if (left + width > p.Width)
            {
                width = p.Width - left;
            }
        }
        if (thumb.VerticalAlignment == VerticalAlignment.Top)
        {
            top = double.IsNaN(Canvas.GetTop(c)) ? 0 : Canvas.GetTop(c) + e.VerticalChange;
            height = c.Height - e.VerticalChange;

            // 不超出 Canvas 上边界
            if (top < 0)
            {
                height += top;
                top = 0;
            }
        }
        else
        {
            top = Canvas.GetTop(c);
            height = c.Height + e.VerticalChange;

            // 不超出 Canvas 下边界
            if (top + height > p.Height)
            {
                height = p.Height - top;
            }
        }
        if (thumb.HorizontalAlignment != HorizontalAlignment.Center)
        {
            if (width >= 0)
            {
                Canvas.SetLeft(c, left);
                c.Width = width;
            }
        }
        if (thumb.VerticalAlignment != VerticalAlignment.Center)
        {
            if (height >= 0)
            {
                Canvas.SetTop(c, top);
                c.Height = height;
            }
        }
        _selection.X = Canvas.GetLeft(c)/p.Width*100F;
        _selection.Y = Canvas.GetTop(c)/p.Height*100F;
        _selection.Width = width/p.Width*100F;
        _selection.Height = height/p.Height*100F;
    }
    private void DrawAxisWithArrow(int x1, int x2, int y1, int y2, bool isXAxis)
    {
        // 绘制主轴线
        Line axisLine = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = Brushes.Lime,
            StrokeThickness = 1,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness { Left = x2, Top = 0, Right = 0, Bottom = 0 }
        };
        _grid.Children.Add(axisLine);

        // 绘制箭头
        TextBlock textBlock = new TextBlock 
        { 
            Text=isXAxis?"> x": "∨y",
            Foreground = Brushes.Lime,
            HorizontalAlignment= HorizontalAlignment.Center,
            VerticalAlignment= VerticalAlignment.Center,
            Margin= new Thickness { Left= isXAxis ? x2 * 2:5, Top=y2, Right=0, Bottom= isXAxis ? 2.5:0 }
        };
        _grid.Children.Add(textBlock);
    }
    
    //thumb的样式
    FrameworkElementFactory GetFactory(Brush back)
    {
        var fef = new FrameworkElementFactory(typeof(Ellipse));
        fef.SetValue(Ellipse.FillProperty, back);
        fef.SetValue(Ellipse.StrokeProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")));
        fef.SetValue(Ellipse.StrokeThicknessProperty, (double)2);
        return fef;
    }
}
