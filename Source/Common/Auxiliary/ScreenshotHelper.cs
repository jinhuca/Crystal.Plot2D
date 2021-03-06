using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Crystal.Plot2D.Common
{
  internal static class ScreenshotHelper
  {
    /// <summary>
    ///   Gets the encoder by extension.
    /// </summary>
    /// <param name="extension">The extension</param>
    /// <returns>BitmapEncoder object</returns>
    internal static BitmapEncoder GetEncoderByExtension(string extension)
    {
      switch (extension)
      {
        case "bmp":
          return new BmpBitmapEncoder();
        case "jpg":
          return new JpegBitmapEncoder();
        case "gif":
          return new GifBitmapEncoder();
        case "png":
          return new PngBitmapEncoder();
        case "tiff":
          return new TiffBitmapEncoder();
        case "wmp":
          return new WmpBitmapEncoder();
        default:
          throw new ArgumentException(Strings.Exceptions.CannotDetermineImageTypeByExtension, "extension");
      }
    }

    /// <summary>
    /// Creates the screenshot of entire plotter element
    /// </summary>
    /// <returns></returns>
    internal static BitmapSource CreateScreenshot(UIElement uiElement, Int32Rect screenshotSource)
    {
      Window window = Window.GetWindow(uiElement);
      if (window == null)
      {
        return CreateElementScreenshot(uiElement);
      }
      Size size = window.RenderSize;

      //double dpiCoeff = 32 / SystemParameters.CursorWidth;
      //int dpi = (int)(dpiCoeff * 96);
      double dpiCoeff = 1;
      int dpi = 96;

      RenderTargetBitmap bmp = new((int)(size.Width * dpiCoeff), (int)(size.Height * dpiCoeff), dpi, dpi, PixelFormats.Default);

      // white background
      Rectangle whiteRect = new() { Width = size.Width, Height = size.Height, Fill = Brushes.White };
      whiteRect.Measure(size);
      whiteRect.Arrange(new Rect(size));
      bmp.Render(whiteRect);
      // the very element
      bmp.Render(uiElement);

      CroppedBitmap croppedBmp = new(bmp, screenshotSource);
      return croppedBmp;
    }

    private static BitmapSource CreateElementScreenshot(UIElement uiElement)
    {
      bool measureValid = uiElement.IsMeasureValid;

      if (!measureValid)
      {
        double width = 300;
        double height = 300;

        if (uiElement is FrameworkElement frElement)
        {
          if (!double.IsNaN(frElement.Width))
          {
            width = frElement.Width;
          }
          if (!double.IsNaN(frElement.Height))
          {
            height = frElement.Height;
          }
        }

        Size size = new(width, height);
        uiElement.Measure(size);
        uiElement.Arrange(new Rect(size));
      }

      RenderTargetBitmap bmp = new(
        (int)uiElement.RenderSize.Width,
        (int)uiElement.RenderSize.Height,
        96,
        96,
        PixelFormats.Default);

      // this is waiting for dispatcher to perform measure, arrange and render passes
      uiElement.Dispatcher.Invoke(((Action)(() => { })), DispatcherPriority.Background);

      Size elementSize = uiElement.DesiredSize;
      // white background
      Rectangle whiteRect = new() { Width = elementSize.Width, Height = elementSize.Height, Fill = Brushes.White };
      whiteRect.Measure(elementSize);
      whiteRect.Arrange(new Rect(elementSize));
      bmp.Render(whiteRect);
      bmp.Render(uiElement);
      return bmp;
    }

    private static readonly Dictionary<BitmapSource, string> pendingBitmaps = new();

    internal static void SaveBitmapToStream(BitmapSource bitmap, Stream stream, string fileExtension)
    {
      if (bitmap == null)
      {
        throw new ArgumentNullException("bitmap");
      }
      if (stream == null)
      {
        throw new ArgumentNullException("stream");
      }
      if (string.IsNullOrEmpty(fileExtension))
      {
        throw new ArgumentException(Strings.Exceptions.ExtensionCannotBeNullOrEmpty, fileExtension);
      }
      BitmapEncoder encoder = GetEncoderByExtension(fileExtension);
      encoder.Frames.Add(BitmapFrame.Create(bitmap, null, new BitmapMetadata(fileExtension.Trim('.')), null));
      encoder.Save(stream);
    }

    internal static void SaveBitmapToFile(BitmapSource bitmap, string filePath)
    {
      if (string.IsNullOrEmpty(filePath))
      {
        throw new ArgumentException(Strings.Exceptions.FilePathCannotbeNullOrEmpty, "filePath");
      }

      if (bitmap.IsDownloading)
      {
        pendingBitmaps[bitmap] = filePath;
        bitmap.DownloadCompleted += OnBitmapDownloadCompleted;
        return;
      }

      string dirPath = System.IO.Path.GetDirectoryName(filePath);
      if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
      {
        Directory.CreateDirectory(dirPath);
      }

      bool fileExistedBefore = File.Exists(filePath);
      try
      {
        using (FileStream fs = new(filePath, FileMode.Create, FileAccess.Write))
        {
          string extension = System.IO.Path.GetExtension(filePath).TrimStart('.');
          SaveBitmapToStream(bitmap, fs, extension);
        }
      }
      catch (ArgumentException)
      {
        if (!fileExistedBefore && File.Exists(filePath))
        {
          try
          {
            File.Delete(filePath);
          }
          catch { }
        }
      }
      catch (IOException exc)
      {
        Debug.WriteLine("Exception while saving bitmap to file: " + exc.Message);
      }
    }

    public static void SaveStreamToFile(Stream stream, string filePath)
    {
      string dirPath = System.IO.Path.GetDirectoryName(filePath);
      if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
      {
        Directory.CreateDirectory(dirPath);
      }

      using (FileStream fs = new(filePath, FileMode.Create, FileAccess.Write))
      {
        string extension = System.IO.Path.GetExtension(filePath).TrimStart('.');
        if (stream.CanSeek)
        {
          stream.Seek(0, SeekOrigin.Begin);
        }
        stream.CopyTo(fs);
      }

      stream.Dispose();
    }

    private static void OnBitmapDownloadCompleted(object sender, EventArgs e)
    {
      BitmapSource bmp = (BitmapSource)sender;
      bmp.DownloadCompleted -= OnBitmapDownloadCompleted;
      string filePath = pendingBitmaps[bmp];
      pendingBitmaps.Remove(bmp);
      SaveBitmapToFile(bmp, filePath);
    }
  }
}
