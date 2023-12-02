namespace MathMethods;

public class Program
{
	static void Main(string[] args)
	{
		TSPexample3();
	}

	static void NelderMeadExample()
	{
		var fn = (double x, double y) => -1 - 15 * x + 2 * x * x + x * y + 2 * y * y;
		var eps = 0.01;
		var alpha = 0.1;
		NelderMead.Solve(fn, eps, (3, 3), alpha);
		// new NedlerMead2().Solve(fn, eps, (3, 3), h);
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
	// Driver code
	static void TSPexample()
	{
		// Adjacency matrix for the given graph
		int[,] adj = { { 0, 10, 15, 20 },
				{ 10, 0, 35, 25 },
				{ 15, 35, 0, 30 },
				{ 20, 25, 30, 0 } };

		var (final_res, final_path) = TSP2.SolveTSP(adj);

		Console.WriteLine("Minimum cost : " + final_res);
		Console.Write("Path Taken : ");
		for (int i = 0; i < final_path.Length; i++)
		{
			Console.Write(final_path[i] + " ");
		}
		Console.WriteLine();
	}

	static void TSPexample2()
	{
		// Adjacency matrix for the given graph
		int[,] adj = { { 0, 5, 11, 9 },
				{ 10, 0, 8, 7 },
				{ 7, 14, 0, 8 },
				{ 12, 6, 15, 0 } };

		var (final_res, final_path) = TSP2.SolveTSP(adj);

		Console.WriteLine("Minimum cost : " + final_res);
		Console.Write("Path Taken : ");
		for (int i = 0; i < final_path.Length; i++)
		{
			Console.Write(final_path[i] + " ");
		}
		Console.WriteLine();
	}

	static void TSPexample3() 
	{
		const double M = Double.NaN;
		double[,] matrix = { 
			{ M, 5, 11, 9 },
			{ 10, M, 8, 7 },
			{ 7, 14, M, 8 },
			{ 12, 6, 15, M }
		};
		TSP.Solve(matrix);
	}
}