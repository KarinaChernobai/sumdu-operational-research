namespace MathMethods;

public class Program
{
	static void Main(string[] args)
	{
		var fn = (double x, double y) => -1 - 15*x + 2*x*x + x*y + 2*y*y;
		double eps = 0.01;
		var h = 2.0;
		NedlerMead.Solve(fn, eps, (3, 3), h);
		// new NedlerMead2().Solve(fn, eps, (3, 3), h);
	}
}