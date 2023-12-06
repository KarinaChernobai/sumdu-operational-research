namespace MathMethods;

public interface ILogWriter
{
	TextWriter GetWriter();
	void EndMessage();
}
