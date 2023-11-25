using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

class TSP3
{
	const double Eps = 0.000001;

	struct S
	{
		public double RowMin;

		public double ColumnMin;
	}

	public static void Solve(ReadOnlySpan2D<double> m)
	{
		var size = m.Width;
		if (size != m.Height) throw new ArgumentException($"Width ({m.Width}) != Hieght ({m.Height})", nameof(m));
		var stat = new S[size];
		for(var i = 0; i < size; i++) 
		{
			m.GetRow(0);
		}
	}

	static void RowStat(ReadOnlySpan<double> row)
	{
		var i = 0;
		var minCount = 1;
		var min = default(double);
		for (; i < row.Length; i++)
		{
			if (!double.IsNaN(row[i]))
			{ 
				min = row[i];
				i++;
				break;
			}
		}

		for (; i < row.Length; i++)
		{ 
			var item  = row[i];
			if (double.IsNaN(item)) continue;
			if (item > min) continue;
			if (item < min)
			{
				min = item;
				minCount = 1;
			}
			else
			{
			}
		}
	}
}
