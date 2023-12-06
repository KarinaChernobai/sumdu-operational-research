using System.Drawing;
using System.Text;
using CommunityToolkit.HighPerformance;
using MathMethods;
using Xunit.Abstractions;

namespace TestProject1;

public class Tsp3Test
{
	private readonly ITestOutputHelper _output;
	const double M = Double.NaN;

	public Tsp3Test(ITestOutputHelper output) => _output = output;
	
	void WritePath(int[] pathArr, double[,] matrix)
	{
		var distSum = default(double);
		var inxRow = 0;
		var inxCol = pathArr[inxRow];
		var path = new StringBuilder();
		var distance = matrix[inxRow, inxCol];
		path.Append($"{inxRow} --[{distance}]--> {inxCol}");
		distSum += distance;
		inxRow = inxCol;
		do
		{
			inxCol = pathArr[inxRow];
			distance = matrix[inxRow, inxCol];
			path.Append($" --[{distance}]--> {inxCol}");
			distSum += distance;
			inxRow = inxCol;
		}
		while (inxRow != 0);
		_output.WriteLine(path.ToString());
		_output.WriteLine(distSum.ToString());
	}

	class TspResConsumer : IConsumer<(int src, int dst)>
	{
		public TspResConsumer(ITestOutputHelper output, double[,] mx)
		{
			(_output, _mx) = (output, mx);
			var count = _mx.GetLength(0);
			sumCol = (count * (count - 1)) / 2;
			sumRow = sumCol;
			pathArr = new int[count];
		}
		private int sumCol;
		private int sumRow;
		private int[] pathArr;

		private readonly ITestOutputHelper _output;
		private readonly double[,] _mx;

		public void Accept((int src, int dst) coord)
		{
			_output.WriteLine($"{coord.src} --[{_mx[coord.src, coord.dst]}]--> {coord.dst}");
			pathArr[coord.src] = coord.dst;
			sumCol -= coord.dst; 
			sumRow -= coord.src;
		}

		public int[] GetFullPath()
		{
			pathArr[sumRow] = sumCol;
			return pathArr;
		}
	}

	class LogWriter(ITestOutputHelper output) : ILogWriter
	{
		private readonly ITestOutputHelper _output = output;
		private readonly StringWriter _writer = new StringWriter();

		public TextWriter GetWriter() => _writer;

		public void EndMessage()
		{
			_output.WriteLine(_writer.ToString());
			_writer.GetStringBuilder().Clear();
		}
	}

	[Fact]
	public void Test1()
	{
		var matrix = new double[,]
		{
			{ M, 5, 11, 9 },
			{ 10, M, 8, 7 },
			{ 7, 14, M, 8 },
			{ 12, 6, 15, M }
		};
		var consumer = new TspResConsumer(_output, matrix);
		TSP2.Solve(matrix, consumer, new LogWriter(_output));
		_output.WriteLine(string.Empty);
		var pathArr = consumer.GetFullPath();
		WritePath(pathArr, matrix);
    }
	[Fact]
	public void Test2()
	{
		var matrix = new double[,]
		{
			{ M,  25, 24, 26, 18, 21 },
			{ 17, M,  16, 18, 20, 15 },
			{ 18, 19, M,  19, 22, 20 },
			{ 28, 25, 20, M,  25, 21 },
			{ 17, 20, 22, 24, M,  22 },
			{ 25, 22, 18, 20, 21, M  }
		};
		var consumer = new TspResConsumer(_output, matrix);
		TSP2.Solve(matrix, consumer, new LogWriter(_output));
		_output.WriteLine(string.Empty);
		var pathArr = consumer.GetFullPath();
		WritePath(pathArr, matrix);
	}
	[Fact]
	public void Test3()
	{
		var matrix = new double[,]
		{
			{ M, 5, 11, 9 },
			{ 10, M, 8, 7 },
			{ 7, 14, M, 8 },
			{ 12, 6, 15, M }
		};
		var consumer = new TspResConsumer(_output, matrix);
		var tsp = new TSP();
		tsp.Solve(matrix, consumer, new LogWriter(_output));
		var pathArr = consumer.GetFullPath();
		WritePath(pathArr, matrix);
	}
	[Fact]
	public void Test4()
	{
		var matrix = new double[,]
		{
			{ M,  25, 24, 26, 18, 21 },
			{ 17, M,  16, 18, 20, 15 },
			{ 18, 19, M,  19, 22, 20 },
			{ 28, 25, 20, M,  25, 21 },
			{ 17, 20, 22, 24, M,  22 },
			{ 25, 22, 18, 20, 21, M  }
		};
		var consumer = new TspResConsumer(_output, matrix);
		var tsp = new TSP();
		tsp.Solve(matrix, consumer, new LogWriter(_output));
		var pathArr = consumer.GetFullPath();
		WritePath(pathArr, matrix);
	}
}