using System;
using System.Windows;
using System.Windows.Input;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;

namespace Crystal.Plot2D.Navigation;

internal sealed class PhysicalRectAnimation
{
  private Vector position;
  private Vector velocity;
  public Vector Velocity
  {
    get => velocity;
    set => velocity = value;
  }

  private Vector acceleration;
  private double mass = 1; // kilograms
  public double Mass
  {
    get => mass;
    set => mass = value;
  }

  private double frictionCalmCoeff;
  public double FrictionCalmCoeff
  {
    get => frictionCalmCoeff;
    set => frictionCalmCoeff = value;
  }

  private double frictionMovementCoeff = 0.1;
  public double FrictionMovementCoeff
  {
    get => frictionMovementCoeff;
    set => frictionMovementCoeff = value;
  }

  private double springCoeff = 50;
  public double SpringCoeff
  {
    get => springCoeff;
    set => springCoeff = value;
  }

  private double liquidFrictionCoeff = 1;
  public double LiquidFrictionCoeff
  {
    get => liquidFrictionCoeff;
    set => liquidFrictionCoeff = value;
  }

  private double liquidFrictionQuadraticCoeff = 10;
  public double LiquidFrictionQuadraticCoeff
  {
    get => liquidFrictionQuadraticCoeff;
    set => liquidFrictionQuadraticCoeff = value;
  }

  private const double G = 9.81;

  private DataRect from;
  private readonly Viewport2D viewport;
  private readonly Point initialMousePos;
  private readonly CoordinateTransform initialTransform;

  public PhysicalRectAnimation(Viewport2D viewport, Point initialMousePos)
  {
    from = viewport.Visible;
    this.viewport = viewport;
    this.initialMousePos = initialMousePos;

    initialTransform = viewport.Transform;

    position = from.Location.ToVector();
  }

  private double prevTime;

  private bool isFinished;
  public bool IsFinished => isFinished;

  private bool useMouse = true;
  public bool UseMouse
  {
    get => useMouse;
    set => useMouse = value;
  }

  public DataRect GetValue(TimeSpan timeSpan)
  {
    var time = timeSpan.TotalSeconds;

    var dtime = time - prevTime;

    acceleration = GetForces() / mass;

    velocity += acceleration * dtime;
    var shift = velocity * dtime;

    var viewportSize = Math.Sqrt(d: from.Width * from.Width + from.Height * from.Height);
    if (!(shift.Length < viewportSize * 0.002 && time > 0.5))
    {
      position += shift;
    }
    else
    {
      isFinished = true;
    }

    prevTime = time;

    Point pos = new(x: position.X, y: position.Y);
    DataRect bounds = new(location: pos, size: from.Size);

    return bounds;
  }

  private Vector GetForces()
  {
    Vector springForce = new();
    if (useMouse)
    {
      var mousePos = GetMousePosition();
      if (!mousePos.IsFinite()) { }

      var p1 = initialMousePos.ScreenToData(transform: initialTransform);
      var p2 = mousePos.ScreenToData(transform: viewport.Transform);

      var transform = viewport.Transform;

      var diff = p2 - p1;
      springForce = -diff * springCoeff;
    }

    var frictionForce = GetFrictionForce(springForce: springForce);

    var liquidFriction = -liquidFrictionCoeff * velocity - liquidFrictionQuadraticCoeff * velocity * velocity.Length;

    var result = springForce + frictionForce + liquidFriction;
    return result;
  }

  private Vector GetFrictionForce(Vector springForce)
  {
    var maxCalmFriction = frictionCalmCoeff * mass * G;
    if (maxCalmFriction >= springForce.Length)
    {
      return -springForce;
    }

    if (velocity.Length == 0)
    {
      return new Vector();
    }

    return -velocity / velocity.Length * frictionMovementCoeff * mass * G;
  }

  private Point GetMousePosition()
  {
    return Mouse.GetPosition(relativeTo: viewport.Plotter.ViewportPanel);
  }
}
