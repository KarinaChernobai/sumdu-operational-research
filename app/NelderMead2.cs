using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

public class NedlerMead2
{
	public NedlerMead2()
	{
		_x1 = new Point(this);
		_x2 = new Point(this);
		_x3 = new Point(this);
		_xe = new Point(this);
		_xc = new Point(this);
		_xr = new Point(this);
		_x0 = new Point(this);
	}

	Func<double, double, double> _fn = null!;

	readonly Point _x1;
	readonly Point _x2;
	readonly Point _x3;
	readonly Point _xe;
	readonly Point _xc;
	readonly Point _xr;
	readonly Point _x0;



	public void Solve(Func<double, double, double> fn, double eps, (double X, double Y) x, double h)
	{
		_fn = fn;
		var alpha = 1.0;
		var beta = 0.5;
		var gamma = 2.0;
		var n = 2.0;

		var delta1 = ((Math.Sqrt(n + 1) + n - 1) / (n * Math.Sqrt(2))) * h;
		var delta2 = ((Math.Sqrt(n + 1) - 1) / (n * Math.Sqrt(2))) * h;

		var xh = _x1.SetCoords(x.X, x.Y);
		var xg = _x2.SetCoords(x.X + delta2, x.Y + delta1);
		var xl = _x3.SetCoords(x.X + delta1, x.Y + delta2);
		var (x0, xr, xc, xe) = (_x0, _xr, _xc, _xe);

		var counter = 0;
		do
		{
			counter++;
			Console.WriteLine($"Interation {counter}");

			(xh, xg, xl) = Order(xh, xg, xl);
			x0.SetCoords((xg.X + xl.X) / 2, (xg.Y + xl.Y) / 2);
			xr.SetCoords((1 + alpha) * x0.X - alpha * xh.X, (1 + alpha) * x0.Y - alpha * xh.Y);

			if (xr.F < xl.F)
			{
				xe.SetCoords(gamma * xr.X + (1 - gamma) * x0.X, gamma * xr.Y + (1 - gamma) * x0.Y);
				xh.Assign(xe.F < xl.F ? xe : xr);
			}
			else
			{
				if (xr.F > xg.F)
				{
					if (xr.F > xh.F)
					{
						xc.SetCoords(beta * xh.X + (1 - beta) * x0.X, beta * xh.Y + (1 - beta) * x0.Y);
					}
					else
					{
						xh.Assign(xr);
						xc.SetCoords(beta * xr.X + (1 - beta) * x0.X, beta * xr.Y + (1 - beta) * x0.Y);
					}

					if (xc.F > xh.F)
					{
						xh.SetCoords((xl.X + xh.X) / 2, (xl.Y + xh.Y) / 2);
						xh.SetCoords((xl.X + xg.X) / 2, (xl.Y + xg.Y) / 2);
					}
					else
					{
						xh.Assign(xc);
					}
				}
				else
				{
					xh.Assign(xr);
				}
			}
		}
		while (!isFinished(eps, xh, xg, xl));
	}

	static bool isFinished(double eps, Point xh, Point xg, Point xl)
	{
		var n = 2.0;
		var fm = (xl.F + xg.F + xh.F) / (n + 1);
		var sigma = ((xl.F - fm) * (xl.F - fm) + (xg.F - fm) * (xg.F - fm) + (xh.F - fm) * (xh.F - fm)) / (n + 1);
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

	private sealed class Point
	{
		public Point(NedlerMead2 parent) => _parent = parent;

		private readonly NedlerMead2 _parent;
		public double X { get; private set; }
		public double Y { get; private set; }
		public double F { get; private set; }

		public Point SetCoords(double x, double y)
		{
			X = x; 
			Y = y;
			F = _parent._fn(X, Y);
			return this;
		}

		public void Assign(Point p) => (X, Y, F) = (p.X, p.Y, p.F);

		public override string ToString() => $"X={X}; Y={Y}; F={F}";
	}
}

