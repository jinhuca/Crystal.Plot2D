﻿using System;
using System.Windows;
using System.Windows.Input;

namespace Crystal.Plot2D.Charts;

internal sealed class PhysicalRectAnimation
{
  Vector position;
  Vector velocity;
  public Vector Velocity
  {
    get => velocity;
    set => velocity = value;
  }

  Vector acceleration;
  private double mass = 1; // kilogramms
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

  double frictionMovementCoeff = 0.1;
  public double FrictionMovementCoeff
  {
    get => frictionMovementCoeff;
    set => frictionMovementCoeff = value;
  }

  double springCoeff = 50;
  public double SpringCoeff
  {
    get => springCoeff;
    set => springCoeff = value;
  }

  double liquidFrictionCoeff = 1;
  public double LiquidFrictionCoeff
  {
    get => liquidFrictionCoeff;
    set => liquidFrictionCoeff = value;
  }

  double liquidFrictionQuadraticCoeff = 10;
  public double LiquidFrictionQuadraticCoeff
  {
    get => liquidFrictionQuadraticCoeff;
    set => liquidFrictionQuadraticCoeff = value;
  }

  const double G = 9.81;

  DataRect from;
  readonly Viewport2D viewport;
  readonly Point initialMousePos;
  readonly CoordinateTransform initialTransform;

  public PhysicalRectAnimation(Viewport2D viewport, Point initialMousePos)
  {
    from = viewport.Visible;
    this.viewport = viewport;
    this.initialMousePos = initialMousePos;

    initialTransform = viewport.Transform;

    position = from.Location.ToVector();
  }

  double prevTime;

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
    double time = timeSpan.TotalSeconds;

    double dtime = time - prevTime;

    acceleration = GetForces() / mass;

    velocity += acceleration * dtime;
    var shift = velocity * dtime;

    double viewportSize = Math.Sqrt(d: from.Width * from.Width + from.Height * from.Height);
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
      Point mousePos = GetMousePosition();
      if (!mousePos.IsFinite()) { }

      Point p1 = initialMousePos.ScreenToData(transform: initialTransform);
      Point p2 = mousePos.ScreenToData(transform: viewport.Transform);

      var transform = viewport.Transform;

      Vector diff = p2 - p1;
      springForce = -diff * springCoeff;
    }

    Vector frictionForce = GetFrictionForce(springForce: springForce);

    Vector liquidFriction = -liquidFrictionCoeff * velocity - liquidFrictionQuadraticCoeff * velocity * velocity.Length;

    Vector result = springForce + frictionForce + liquidFriction;
    return result;
  }

  private Vector GetFrictionForce(Vector springForce)
  {
    double maxCalmFriction = frictionCalmCoeff * mass * G;
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
