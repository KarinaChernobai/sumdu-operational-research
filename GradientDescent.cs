using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

public class GradientDescent
{
	const double DoubleEpsilon = 1e-15;
	const string DoubleFormat = "0.###############";

	class Point
	{
		public double X;
		public double Phi;
		public Point(double x, double phi)
			=> (X, Phi) = (x, phi);

		public override string ToString() => $"X: {X.ToString(DoubleFormat)}; Phi: {Phi.ToString(DoubleFormat)}";
	}
	struct Alg
	{ 
		Func<double[], double> fn;
		Func<double[], double>[] grad;
		double[] x;
		double[] xNext;
		Point a;
		Point b;
		Point c;

		public Alg(Func<double[], double> fn, Func<double[], double>[] grad)
		{
			this.fn = fn;
			this.grad = grad;
		}

		public bool QuadraticInterpolation()
		{
			var sqrA = a.X * a.X;
			var sqrB = b.X * b.X;
			var sqrC = c.X * c.X;
			var denominator = (b.X - c.X) * a.Phi + (c.X - a.X) * b.Phi + (a.X - b.X) * c.Phi;
			if (denominator == 0) return false;
			var delta = ((sqrB - sqrC) * a.Phi + (sqrC - sqrA) * b.Phi + (sqrA - sqrB) * c.Phi) / (denominator * 2);
			if (Double.IsInfinity(delta)) return false;
			CalcPhiPoint(delta); // xNext is calculated to be x - lambda * grad(x)
			var phiVal = fn(xNext);
			var max = GetMaxPhiPoint(); // a.Phi, b.Phi, c.Phi are compared and the biggest is saved to var max
			if (Math.Abs(max.X - delta) < DoubleEpsilon) return false;
			if (Math.Abs(max.Phi - phiVal) < DoubleEpsilon) return false;
			if (phiVal > max.Phi) throw new Exception("Error Quadratic interpolation");
			max.X = delta; // update with new value
			max.Phi = phiVal;
			Order();
			return true;
		}

		public double Iteration(double lambda, double[] x, double[] xNext)
		{
			this.x = x;
			this.xNext = xNext;
			CalcPhiPoint(0); // updates xNext
			a = new Point(0, fn(xNext)); // a is updated for future use in QuadraticInterpolation
			CalcPhiPoint(lambda / 2);
			b = new Point(lambda / 2, fn(xNext));
			CalcPhiPoint(lambda);
			c = new Point(lambda, fn(xNext));
			Order();

			var r = QuadraticInterpolation() && QuadraticInterpolation() && QuadraticInterpolation();
			var minLambda = GetMinLambda(); // compare a.Phi, b.Phi, c.Phi
			CalcPhiPoint(minLambda); // updates xNext
			return fn(xNext);
		}

		Point GetMaxPhiPoint() 
		{
			if (a.Phi > b.Phi && a.Phi > c.Phi) return a;
			if (b.Phi > c.Phi) return b;
			return c;
		}

		public double GetMinLambda () 
		{
			if (a.Phi < b.Phi && a.Phi < c.Phi) return a.X;
			if (b.Phi < c.Phi) return b.X;
			return c.X;
		}

		void Order()
		{
			if (a.X > b.X)
			{
				if (a.X > c.X)
				{
					// a is max
					if (b.X > c.X) (a, b, c) = (c, b, a);
					else (a, b, c) = (b, c, a);
				}
				// c is max
				(a, b) = (b, a);
			}
			else // a.X < b.X
			{
				if (b.X > c.X)
				{
					// b is max
					if (a.X > c.X) (a, b, c) = (c, a, b);
					else (b, c) = (c, b);
				}
				// else everything is already ordered
			}
		}


		void CalcPhiPoint(double lambda)
		{
			for (var i = 0; i < x.Length; i++) xNext[i] = x[i] - lambda * grad[i](x);
		}

		public double Norm()
		{
			var d = default(double);
			for (var i = 0; i < grad.Length; i++)
			{
				var tmp = grad[i](x);
				d += tmp * tmp;
			}
			return Math.Sqrt(d);
		}
	}
	public static double Solve(Func<double[], double> fn, Func<double[], double> [] grad, double[] x, double lambda, double eps, double[] xNext)
	{
		var alg = new Alg(fn, grad);
		var fnValPrev = alg.Iteration(lambda, x, xNext);
		var counter = 0;
		Console.Write($"{counter++}) ");
		for (int i = 0; i < x.Length; i++) Console.Write($"x{i} = {x[i]}; ");
        Console.WriteLine($"fn(x) = {fnValPrev}");
		while (true) 
		{
			lambda /= 2;
			(x, xNext) = (xNext, x);
			var fnValNext = alg.Iteration(lambda, x, xNext);
			Console.Write($"{counter++}) ");
			for (int i = 0; i < x.Length; i++) Console.Write($"x{i} = {x[i]}; ");
			Console.WriteLine($"fn(x) = {fnValPrev}");
			if (Math.Abs(fnValPrev - fnValNext) < eps) break;
			fnValPrev = fnValNext;
		}
		return fnValPrev;
	}
}
