using System;

namespace MathMethods;

// finds minimum of the function
public static class NelderMead
{
	public static Point Solve(Func<double, double, double> fn, double eps, (double X, double Y) x, double h)
	{
		var alpha = 1.0;
		var beta = 0.5;
		var gamma = 2.0;
		var n = 2.0;

		var delta1 = ((Math.Sqrt(n + 1) + n - 1) / (n * Math.Sqrt(2))) * h;
		var delta2 = ((Math.Sqrt(n + 1) - 1) / (n * Math.Sqrt(2))) * h;

		var x1 = new Point(x.X, x.Y, fn);
		var x2 = new Point(x1.X + delta2, x1.Y + delta1, fn);
		var x3 = new Point(x1.X + delta1, x1.Y + delta2, fn);
		var (xh, xg, xl) = Order(x1, x2, x3);

		var counter = 0;
		do
		{
			counter++;
			Console.WriteLine($"Interation {counter}");

			(xh, xg, xl) = Order(xh, xg, xl);
			var x0 = new Point((xg.X + xl.X) / 2, (xg.Y + xl.Y) / 2, fn);
			var xr = new Point((1 + alpha) * x0.X - alpha * xh.X, (1 + alpha) * x0.Y - alpha * xh.Y, fn);

			if (xr.F < xl.F)
			{
				var xe = new Point(gamma * xr.X + (1 - gamma) * x0.X, gamma * xr.Y + (1 - gamma) * x0.Y, fn);
				if (xe.F < xl.F) xh = xe;
				else xh = xr;
			}
			else
			{
				if (xr.F > xg.F)
				{
					var xc = default(Point);
					if (xr.F > xh.F)
					{
						xc = new Point(beta * xh.X + (1 - beta) * x0.X, beta * xh.Y + (1 - beta) * x0.Y, fn);
					}
					else
					{
						xh = xr;
						xc = new Point(beta * xr.X + (1 - beta) * x0.X, beta * xr.Y + (1 - beta) * x0.Y, fn);
					}

					if (xc.F > xh.F)
					{
						xh.X = (xl.X + xh.X) / 2;
						xh.Y = (xl.Y + xh.Y) / 2;
						xh.F = fn(xh.X, xh.Y);

						xg.X = (xl.X + xg.X) / 2;
						xg.Y = (xl.Y + xg.Y) / 2;
						xg.F = fn(xg.X, xg.Y);
					}
					else xh = xc;
				}
				else xh = xr;
			}
		}
		while (!isFinished(eps, xh, xg, xl));
		return xl;
	}

	static bool isFinished(double eps, Point xh, Point xg, Point xl)
	{
		var n = 2.0;
		var fm = (xl.F + xg.F + xh.F) / (n + 1);
		var sigma = ( (xl.F - fm) * (xl.F - fm) + (xg.F - fm) * (xg.F - fm) + (xh.F - fm) * (xh.F - fm) ) / (n + 1);
		sigma = Math.Sqrt(sigma);
		Console.WriteLine($"sigma: {sigma}");
		if (sigma < eps)
		{
			Console.WriteLine($"sigma < eps {sigma} < {eps}");
			(xh, xg, xl) = Order(xh, xg, xl);
			Console.WriteLine($"fn min (x, y): ({xl.X}, {xl.Y}) fn val: {xl.F}");
			return true;
		}
		else return false;
	}

	static (Point, Point, Point) Order(Point a, Point b, Point c)
	{
		if (a.F > b.F) // _a_b_
		{
			if (c.F > a.F) return (c, a, b); // ca_b_
			else if (b.F > c.F) return (a, b, c); // _a_bc
			else return (a, c, b); // _acb_
		}
		else // b > c _b_a_
		{
			if (c.F > b.F) return (c, b, a); // cb_a_
			else if (a.F > c.F) return (b, a, c); // _b_ac 
			else return (b, c, a); // _bca_
		}
	}

	public class Point
	{
		public Point(double x, double y, Func<double, double, double> fn) => (X, Y, F) = (x, y, fn(x, y));
		public double X { get; set; }
		public double Y { get; set; }
		public double F { get; set; }

		public override string ToString() => $"X={X}; Y={Y}; F={F}";
	}
}
