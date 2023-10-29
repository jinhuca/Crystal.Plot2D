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
    var time_ = timeSpan.TotalSeconds;

    var dTime_ = time_ - prevTime;

    acceleration = GetForces() / mass;

    velocity += acceleration * dTime_;
    var shift_ = velocity * dTime_;

    var viewportSize_ = Math.Sqrt(d: from.Width * from.Width + from.Height * from.Height);
    if (!(shift_.Length < viewportSize_ * 0.002 && time_ > 0.5))
    {
      position += shift_;
    }
    else
    {
      isFinished = true;
    }

    prevTime = time_;

    Point pos_ = new(x: position.X, y: position.Y);
    DataRect bounds_ = new(location: pos_, size: from.Size);

    return bounds_;
  }

  private Vector GetForces()
  {
    Vector springForce_ = new();
    if (useMouse)
    {
      var mousePos_ = GetMousePosition();
      if (!mousePos_.IsFinite()) { }

      var p1_ = initialMousePos.ScreenToData(transform: initialTransform);
      var p2_ = mousePos_.ScreenToData(transform: viewport.Transform);

      var transform_ = viewport.Transform;

      var diff_ = p2_ - p1_;
      springForce_ = -diff_ * springCoeff;
    }

    var frictionForce_ = GetFrictionForce(springForce: springForce_);

    var liquidFriction_ = -liquidFrictionCoeff * velocity - liquidFrictionQuadraticCoeff * velocity * velocity.Length;

    var result_ = springForce_ + frictionForce_ + liquidFriction_;
    return result_;
  }

  private Vector GetFrictionForce(Vector springForce)
  {
    var maxCalmFriction_ = frictionCalmCoeff * mass * G;
    if (maxCalmFriction_ >= springForce.Length)
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
