using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

class LogWriter(TextWriter output) : ILogWriter
{
	private readonly TextWriter _output = output;

	public TextWriter GetWriter() => _output;

	public void EndMessage() => _output.WriteLine();
}
