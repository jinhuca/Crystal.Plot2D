using System;
using System.Runtime.Serialization;

namespace Crystal.Plot2D.Isolines;

/// <summary>
///   Exception that is thrown when error occurs while building isolines.
/// </summary>
[Serializable]
public sealed class IsolineGenerationException : Exception
{
  internal IsolineGenerationException() { }
  internal IsolineGenerationException(string message) : base(message: message) { }
  internal IsolineGenerationException(string message, Exception inner) : base(message: message, innerException: inner) { }
  internal IsolineGenerationException(SerializationInfo info, StreamingContext context) : base(info: info, context: context) { }
}
