using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

public class GradientDescent2
{
	public static void Solve(Func<double[], double> fn, double[] x, double alpha, double eps)
	{
		var n = x.Length;
		var delta = 1e-6;
		var fnVal0 = fn(x);
		var grad = new double[n];
		Grad(fn, x, delta, grad);

		// initial norm
		var gradNorm = Norm(grad);
		var lambda = alpha / gradNorm;

		var counter = 0;
		while (gradNorm > eps)
		{
			// next value
			for (int i = 0; i < n; ++i) x[i] -= lambda * grad[i];
			// next gradient
			Grad(fn, x, delta, grad); 
			// next norm
			gradNorm = Norm(grad);
			lambda = alpha / gradNorm;
			var fnVal = fn(x);
			Console.WriteLine($"{counter}) x0 = {x[0]}; x1 = {x[1]}; fn(x) = {fnVal}");
			if (fnVal > fnVal0) alpha /= 2;
			else fnVal0 = fnVal;
			counter++;
		}
	}

	private static double Norm(double[] v)
	{
		var norm = default(double);
		for (int i = 0; i < v.Length; ++i) norm += v[i] * v[i];
		norm = Math.Sqrt(norm);
		return norm;
	}
	// calculates the gradient fn(x)
	private static void Grad(Func<double[], double> fn, double[] x, double delta, double[] grad)
	{
		var val = fn(x);
		var tmp = x[0];
		x[0] += delta;
		grad[0] = (fn(x) - val) / delta;
		x[0] = tmp;
		for (int i = 1; i < x.Length; ++i)
		{
			tmp = x[i];
			x[i] += delta;
			grad[i] = (fn(x) - val) / delta;
			x[i] = tmp;
		}
	}
}
