using System.Reflection.Metadata;
using System.Text;

namespace MathMethods;

public class Program
{
	static void Main(string[] args)
	{
		Console.OutputEncoding = Encoding.UTF8;
		TSPexample2();
	}

	static void NelderMeadExample()
	{
		var fn = (double x, double y) => -1 - 15 * x + 2 * x * x + x * y + 2 * y * y;
		var eps = 0.01;
		var alpha = 0.1;
		NelderMead.Solve(fn, eps, (3, 3), alpha);
	}
	static void GradientDescentExample()
	{
		var fn = (double[] x) => -1 - 15 * x[0] + 2 * x[0] * x[0] + x[0] * x[1] + 2 * x[1] * x[1];
		var grad = new Func<double[], double>[]
		{
			x => (4 * x[0] + x[1] - 15),
			x => (x[0] + 4 * x[1]),
		};
		var xNext = new double[2];
		var res = GradientDescent.Solve(fn, grad, [3, 3], 4.0, 0.001, xNext);
		Console.WriteLine($"The minimum is at\n x[0] = {xNext[0]},\n x[1] = {xNext[1]},\n fn(x) = {res}");
	}
	static void GradientDescentExample2()
	{
		var fn = (double[] x) => ( (x[0] - 1) * (x[0] - 1) + (x[1] - 3) * (x[1] - 3) + 4 * (x[2] + 5) * (x[2] + 5) );
		var grad = new Func<double[], double>[]
		{
			x => (2 * x[0] - 2),
			x => (2 * x[1] - 6),
			x => (8 * x[2] + 40),
		};
		var xNext = new double[3];
		var res = GradientDescent.Solve(fn, grad, [4, -1, 2], 4.0, 0.001, xNext);
		Console.WriteLine($"The minimum is at\n x[0] = {xNext[0]},\n x[1] = {xNext[1]},\n x[2] = {xNext[2]},\n fn(x) = {res}");
	}
	static void GradientDescentExample3()
	{
		var fn = (double[] x) => -1 - 15 * x[0] + 2 * x[0] * x[0] + x[0] * x[1] + 2 * x[1] * x[1];
		double eps = 0.1;
		double alpha = 4.0;
		double[] x = {3.0, 3.0};
		GradientDescent2.Solve(fn, x, alpha, eps);
		Console.WriteLine($"The minimum is at\n x[0] = {x[0]},\n x[1] = {x[1]},\n fn(x) = {fn(x)}");
	}
	static void TSPexample()
	{
		const double M = Double.NaN;
		double[,] matrix = {
			{ M, 5, 11, 9 },
			{ 10, M, 8, 7 },
			{ 7, 14, M, 8 },
			{ 12, 6, 15, M }
		};
		var tsp = new TSP();
		var consumer = new TspResConsumer(Console.Out, matrix);
		tsp.Solve(matrix, consumer, new LogWriter(Console.Out));
		var pathArr = consumer.GetFullPath();
		Console.Out.WriteTspPath(pathArr, matrix);
	}

	static void TSPexample2()
	{
		const double M = Double.NaN;
		var matrix = new double[,]
		{
			{ M,  25, 24, 26, 18, 21 },
			{ 17, M,  16, 18, 20, 15 },
			{ 18, 19, M,  19, 22, 20 },
			{ 28, 25, 20, M,  25, 21 },
			{ 17, 20, 22, 24, M,  22 },
			{ 25, 22, 18, 20, 21, M  }
		};
		var tsp = new TSP();
		var consumer = new TspResConsumer(Console.Out, matrix);
		tsp.Solve(matrix, consumer, new LogWriter(Console.Out));
		var pathArr = consumer.GetFullPath();
		Console.Out.WriteTspPath(pathArr, matrix);
	}
}