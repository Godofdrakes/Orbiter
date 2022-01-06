namespace LibOrbiter.Exceptions;

public class OrbiterException : Exception
{
	public OrbiterException(string? message) : base(message) { }
	public OrbiterException(string? message, Exception? innerException) : base(message, innerException) { }
}