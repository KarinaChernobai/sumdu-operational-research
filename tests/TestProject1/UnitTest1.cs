using CommunityToolkit.HighPerformance;
using MathMethods;
using Xunit.Abstractions;

namespace TestProject1;

public class Tsp3Test
{
	private readonly ITestOutputHelper _output;

	public Tsp3Test(ITestOutputHelper output) => _output = output;

	class TspResConsumer : IConsumer<(int row, int column)>
	{
		public TspResConsumer(ITestOutputHelper output, double[,] mx) => (_output, _mx) = (output, mx);

		private readonly ITestOutputHelper _output;
		private readonly double[,] _mx;

		public void Accept((int row, int column) coord)
		{
			_output.WriteLine($"{coord.row} --[{_mx[coord.row, coord.column],2}]--> {coord.column}");
		}
	}

	[Fact]
	public void Test1()
	{
		const double M = Double.NaN;
		var matrix = new double[,]
		{
			{ M, 5, 11, 9 },
			{ 10, M, 8, 7 },
			{ 7, 14, M, 8 },
			{ 12, 6, 15, M }
		};
		Tsp3.Solve(matrix, new TspResConsumer(_output, matrix));
	}
}