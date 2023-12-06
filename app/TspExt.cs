using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

static class TspExt
{
	public static void WriteTspPath(this TextWriter output, int[] pathArr, double[,] matrix)
	{
		var distSum = default(double);
		var inxRow = 0;
		var inxCol = pathArr[inxRow];
		var path = new StringBuilder();
		var distance = matrix[inxRow, inxCol];
		path.Append($"{inxRow} --[{distance}]--> {inxCol}");
		distSum += distance;
		inxRow = inxCol;
		do
		{
			inxCol = pathArr[inxRow];
			distance = matrix[inxRow, inxCol];
			path.Append($" --[{distance}]--> {inxCol}");
			distSum += distance;
			inxRow = inxCol;
		}
		while (inxRow != 0);
		output.WriteLine(path.ToString());
		output.WriteLine(distSum.ToString());
	}

	public static string ToStringPretty(this double value)
	{
		if (double.IsNaN(value)) return "?";
		if (double.IsPositiveInfinity(value)) return "∞";
		if (double.IsNegativeInfinity(value)) return "-∞";
		return value.ToString("0.######");
	}
}
