using Crystal.Plot2D.Common;

namespace Crystal.Plot2D
{
  internal sealed class Viewport2dDeferredPanningProxy : Viewport2D
  {
    private readonly Viewport2D viewport;
    internal Viewport2dDeferredPanningProxy(Viewport2D viewport) : base(viewport.HostElement, viewport.PlotterBase)
    {
      viewport.PropertyChanged += viewport_PropertyChanged;
      viewport.PanningStateChanged += viewport_PanningStateChanged;
      this.viewport = viewport;
    }

    DataRect prevVisible;

    private void viewport_PanningStateChanged(object sender, ValueChangedEventArgs<Viewport2DPanningState> e)
    {
      if (e.CurrentValue == Viewport2DPanningState.Panning && e.PreviousValue == Viewport2DPanningState.NotPanning)
      {
        prevVisible = Visible;
      }
      else if (e.CurrentValue == Viewport2DPanningState.NotPanning && e.PreviousValue == Viewport2DPanningState.Panning)
      {
        notMineEvent = true;
        try
        {
          Visible = viewport.Visible;
        }
        finally { notMineEvent = false; }
      }
    }

    protected override DataRect CoerceVisible(DataRect newVisible)
    {
      if (viewport == null)
      {
        return new DataRect(0, 0, 1, 1);
      }

      if (viewport.PanningState == Viewport2DPanningState.Panning)
      {
        return prevVisible;
      }
      return base.CoerceVisible(newVisible);
    }

    private bool notMineEvent = false;
    private void viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
    {
      notMineEvent = true;
      try
      {
        RaisePropertyChanged(e);
      }
      finally
      {
        notMineEvent = false;
      }
    }

    public override CoordinateTransform Transform
    {
      get => viewport.PanningState == Viewport2DPanningState.Panning
          ? viewport.Transform.WithRects(prevVisible, viewport.Output)
          : viewport.Transform;
      set => base.Transform = value;
    }

    protected override void RaisePropertyChanged(ExtendedPropertyChangedEventArgs args)
    {
      if (args.PropertyName == "Visible")
      {
        if (notMineEvent)
        {
          if (viewport.PanningState == Viewport2DPanningState.NotPanning)
          {
            base.RaisePropertyChanged(args);
          }
          else
          {
            // do nothing
          }
        }
      }
      else
      {
        base.RaisePropertyChanged(args);
      }
    }
  }
}