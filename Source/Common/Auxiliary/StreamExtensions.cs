using System.IO;

namespace Crystal.Plot2D.Common;

public static class StreamExtensions
{
  public static void CopyTo(this Stream input, Stream output)
  {
    byte[] buffer = new byte[32768];
    while (true)
    {
      int read = input.Read(buffer: buffer, offset: 0, count: buffer.Length);
      if (read <= 0)
      {
        return;
      }

      output.Write(buffer: buffer, offset: 0, count: read);
    }
  }
}
