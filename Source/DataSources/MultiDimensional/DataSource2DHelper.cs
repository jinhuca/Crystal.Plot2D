﻿using System;
using System.Windows;

namespace Crystal.Plot2D.DataSources;

public static class DataSource2DHelper
{
  public static Point[,] CreateUniformGrid(int width, int height, double gridWidth, double gridHeight)
  {
    return CreateUniformGrid(width: width, height: height, xStart: 0, yStart: 0, xStep: gridWidth / width, yStep: gridHeight / height);
  }

  public static Point[,] CreateUniformGrid(int width, int height, double xStart, double yStart, double xStep, double yStep)
  {
    Point[,] result = new Point[width, height];

    double x = xStart;
    for (int ix = 0; ix < width; ix++)
    {
      double y = yStart;
      for (int iy = 0; iy < height; iy++)
      {
        result[ix, iy] = new Point(x: x, y: y);
        y += yStep;
      }
      x += xStep;
    }

    return result;
  }

  public static Vector[,] CreateVectorData(int width, int height, Func<int, int, Vector> generator)
  {
    Vector[,] result = new Vector[width, height];

    for (int ix = 0; ix < width; ix++)
    {
      for (int iy = 0; iy < height; iy++)
      {
        result[ix, iy] = generator(arg1: ix, arg2: iy);
      }
    }

    return result;
  }
}