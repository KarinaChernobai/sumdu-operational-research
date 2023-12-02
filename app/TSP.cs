using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

public class TSP
{
	const string DoubleFormat = "0.###############";
	class Point
	{
		public double Value;
		public double Sum;
		public Point(double val) => Value = val;
		public override string ToString() => $"X: {Value.ToString(DoubleFormat)}; Phi: {Sum.ToString(DoubleFormat)}";
	}
	public static void Solve(double[,] initialMatrix)
	{
		var n = initialMatrix.GetLength(0);
		var m = initialMatrix.GetLength(1);
		Point[,] matrix = new Point[n, m];
		var solution = new double[n];
		var solInx = 0;
		var distance = default(double);
		var horizontal = new double[n];
		var vertical = new double[m];
		for (int i = 0; i < n; i++)
		{
			horizontal[i] = i;
			vertical[i] = i;
		}

		while (n != 0 && m != 0) 
		{
			double[] rowMin = new double[n];
			// fill the matrix and find the smallest element in the row
			Console.WriteLine("fill the matrix");
			for (int i = 0; i < n; i++)
			{
				var min = Double.MaxValue;
				for (int j = 0; j < m; j++)
				{
					var value = initialMatrix[i, j];
					matrix[i, j] = new Point(value);
					if (Double.IsNaN(value)) continue;
					if (value < min) min = value;
					Console.Write(initialMatrix[i, j] + " ");
				}
				Console.WriteLine();
				rowMin[i] = min;
			}
			// to be deleted
			Console.WriteLine("find the smallest element in the row");
			for (int i = 0; i < rowMin.Length; i++)
			{
				Console.Write(rowMin[i] + " ");
			}
			Console.WriteLine();
			Console.WriteLine("reduce every element in the row by the rowMin");
			// reduce every element in the row by the rowMin
			double[] columnMin = new double[m];
			for (int i = 0; i < columnMin.Length; i++)
			{
				columnMin[i] = Double.MaxValue;
			}
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					if (Double.IsNaN(value)) continue;
					value -= rowMin[i];
					matrix[i, j].Value = value;
					if (columnMin[j] > value)
					{
						columnMin[j] = value;
					}
					Console.Write(matrix[i, j].Value + " ");
				}
				Console.WriteLine();
			}
			// to be deleted
			Console.WriteLine("find the smallest element in the column");
			for (int i = 0; i < columnMin.Length; i++)
			{
				Console.Write(columnMin[i] + " ");
			}
			// reduce every element in the row by the columnMin
			Console.WriteLine("\nreduce every element in the row by the columnMin");
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					if (Double.IsNaN(value)) continue;
					value -= columnMin[j];
					matrix[i, j].Value = value;
					Console.Write(matrix[i, j].Value + " ");
				}
				Console.WriteLine();
			}
			// calculate sum in the null sqaure
			Console.WriteLine("\ncount zeros and min non zero value ");
			for (int i = 0; i < n; i++)
			{
				// count zeros and min non zero value 
				var counterZero = 0;
				var minVal = double.MaxValue;
				var counterZero2 = 0;
				var minVal2 = double.MaxValue;
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					if (!Double.IsNaN(value))
					{
						if (value == 0)
						{
							if (++counterZero > 1) minVal = 0;
						}
						else if (minVal > value) minVal = value;
					}

					var value2 = matrix[j, i].Value;
					if (!Double.IsNaN(value))
					{
						if (value2 == 0)
						{
							if (++counterZero2 > 1) minVal2 = 0;
						}
						else if (minVal2 > value2) minVal2 = value2;
					}
				}

				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					if (!Double.IsNaN(value))
					{
						if (value == 0 && counterZero == 1) matrix[i, j].Sum += minVal;
					}

					var value2 = matrix[j, i].Value;
					if (!Double.IsNaN(value2))
					{
						if (value2 == 0 && counterZero2 == 1) matrix[j, i].Sum += minVal2;
					}
				}
			}

			// this cycle is ought to be deleted
			Console.WriteLine("\nPrint sums");
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					var sum = matrix[i, j].Sum;
					if (Double.IsNaN(value)) Console.Write("M ");
					else if (value == 0) Console.Write(value + "(" + sum + ") ");
					else Console.Write(value + " ");
					// else Console.Write(value + "(" + sum + ")");
				}
				Console.WriteLine();
			}

			Console.WriteLine("\nFind the biggest element");
			var maxVal = double.MinValue;
			var p = 0;
			var q = 0;
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					if (Double.IsNaN(value)) continue;
					var sum = matrix[i, j].Sum;
					if (value == 0 && maxVal < sum)
					{
						maxVal = sum;
						p = i;
						q = j;
					}
				}
			}
			Console.WriteLine("biggest sum = " + maxVal);

			matrix[q, p].Value = Double.NaN;
			Console.WriteLine("\nRewrite the matrix");
			if (p != n - 1)
			{
				var i = p;
				// for (int j = 0; j < n; j++) matrix[i, j] = matrix[n - 1, j];
				for (int j = 0; j < n; j++)
				{
					matrix[i, j].Value = initialMatrix[n - 1, j];
				}

				for (int j = 0;j < m; j++)
				{

				}
			}
			if (q != m - 1)
			{
				var j = q;
				// for (int i = 0; i < n; i++) matrix[i, j] = matrix[i, m - 1];
				for (int i = 0; i < n; i++)
				{
					matrix[i, j].Value = initialMatrix[i, m - 1];
				}
			}
			n--;
			m--;

			// this cycle is ought to be deleted
			// print new matrix
			Console.WriteLine("\nprint new matrix");
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					if (Double.IsNaN(value)) Console.Write("M ");
					else Console.Write(value + " ");
				}
				Console.WriteLine();
			}

			solution[solInx] = q;
			solution[solInx + 1] = p;
			distance += initialMatrix[p, q];

			for (int i = 0; i < solution.Length; i++)
			{
				Console.Write(solution[i] + "->");
			}
			Console.WriteLine();
			Console.WriteLine("distance " + distance);


			// renew matrix
			Console.WriteLine("\nrenew matrix");
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < m; j++)
				{
					var value = matrix[i, j].Value;
					var sum = matrix[i, j].Sum;
					if (Double.IsNaN(value)) Console.Write("M ");
					else if (value == 0) Console.Write(value + "(" + sum + ") ");
					else Console.Write(value + " ");
					// else Console.Write(value + "(" + sum + ")");
				}
				Console.WriteLine();
			}

			Console.WriteLine();
		}
	}
}
