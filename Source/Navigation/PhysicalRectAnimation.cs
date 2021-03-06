using System;
using System.Windows;
using System.Windows.Input;

namespace Crystal.Plot2D.Charts
{
  internal sealed class PhysicalRectAnimation
  {
    Vector position = new();
    Vector velocity = new();
    public Vector Velocity
    {
      get { return velocity; }
      set { velocity = value; }
    }

    Vector acceleration = new();
    private double mass = 1; // kilogramms
    public double Mass
    {
      get { return mass; }
      set { mass = value; }
    }

    private double frictionCalmCoeff = 0.0;
    public double FrictionCalmCoeff
    {
      get { return frictionCalmCoeff; }
      set { frictionCalmCoeff = value; }
    }

    double frictionMovementCoeff = 0.1;
    public double FrictionMovementCoeff
    {
      get { return frictionMovementCoeff; }
      set { frictionMovementCoeff = value; }
    }

    double springCoeff = 50;
    public double SpringCoeff
    {
      get { return springCoeff; }
      set { springCoeff = value; }
    }

    double liquidFrictionCoeff = 1;
    public double LiquidFrictionCoeff
    {
      get { return liquidFrictionCoeff; }
      set { liquidFrictionCoeff = value; }
    }

    double liquidFrictionQuadraticCoeff = 10;
    public double LiquidFrictionQuadraticCoeff
    {
      get { return liquidFrictionQuadraticCoeff; }
      set { liquidFrictionQuadraticCoeff = value; }
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

    private bool isFinished = false;
    public bool IsFinished
    {
      get { return isFinished; }
    }

    private bool useMouse = true;
    public bool UseMouse
    {
      get { return useMouse; }
      set { useMouse = value; }
    }

    public DataRect GetValue(TimeSpan timeSpan)
    {
      double time = timeSpan.TotalSeconds;

      double dtime = time - prevTime;

      acceleration = GetForces() / mass;

      velocity += acceleration * dtime;
      var shift = velocity * dtime;

      double viewportSize = Math.Sqrt(from.Width * from.Width + from.Height * from.Height);
      if (!(shift.Length < viewportSize * 0.002 && time > 0.5))
      {
        position += shift;
      }
      else
      {
        isFinished = true;
      }

      prevTime = time;

      Point pos = new(position.X, position.Y);
      DataRect bounds = new(pos, from.Size);

      return bounds;
    }

    private Vector GetForces()
    {
      Vector springForce = new();
      if (useMouse)
      {
        Point mousePos = GetMousePosition();
        if (!mousePos.IsFinite()) { }

        Point p1 = initialMousePos.ScreenToData(initialTransform);
        Point p2 = mousePos.ScreenToData(viewport.Transform);

        var transform = viewport.Transform;

        Vector diff = p2 - p1;
        springForce = -diff * springCoeff;
      }

      Vector frictionForce = GetFrictionForce(springForce);

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
      return Mouse.GetPosition(viewport.Plotter.ViewportPanel);
    }
  }
}
