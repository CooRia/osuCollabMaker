using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;

namespace osuCollabMaker;

public class SelectionRectangleController
{
    private readonly Canvas _canvas;

    private bool _isDragging;
    private Point _dragStartPos;
    private Point _rectanglePos;

    public SelectionRectangleController(Canvas canvas)
    {
        _canvas = canvas;
    }

    public void InitSelectionRectangle(Rectangle rectangle)
    {
        Selection selection = rectangle.Tag as Selection;
        var adorner = new CanvasAdorner(rectangle, _canvas, selection);
        var layer = AdornerLayer.GetAdornerLayer(rectangle);
        layer.Add(adorner);
        rectangle.MouseLeftButtonDown += OnMouseDown;
        rectangle.MouseLeftButtonUp += OnMouseUp;
        rectangle.MouseMove += OnMouseMove;
        rectangle.MouseEnter += OnMouseEnter;
    }
    
    
    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;
        
        var rect = sender as Rectangle;
        if (rect == null) return;

        _isDragging = true;
        _dragStartPos = e.GetPosition(_canvas);
        _rectanglePos = new Point(
            Canvas.GetLeft(rect),
            Canvas.GetTop(rect));
        rect.CaptureMouse();
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        
        var rect = sender as Rectangle;
        if (rect == null) return;
        
        var pos = e.GetPosition(_canvas);
        double dx = pos.X - _dragStartPos.X;
        double dy = pos.Y - _dragStartPos.Y;

        double newLeft = Math.Max(0, Math.Min(_rectanglePos.X + dx, _canvas.Width - rect.Width));
        double newTop  = Math.Max(0, Math.Min(_rectanglePos.Y + dy, _canvas.Height - rect.Height));

        Canvas.SetLeft(rect, newLeft);
        Canvas.SetTop(rect,  newTop);
        Selection selection = rect.Tag as Selection;
        selection.X = newLeft/_canvas.Width*100F;
        selection.Y = newTop/_canvas.Height*100F;
        e.Handled = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        var rect = sender as Rectangle;
        if (rect == null) return;
        
        _isDragging = false;
        rect.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        var rect = sender as Rectangle;
        if (rect == null) return;
        
        rect.Cursor = Cursors.SizeAll;
    }
}