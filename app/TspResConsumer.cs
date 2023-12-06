using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

class TspResConsumer : IConsumer<(int src, int dst)>
{
	public TspResConsumer(TextWriter output, double[,] mx)
	{
		(_output, _mx) = (output, mx);
		var count = _mx.GetLength(0);
		sumCol = (count * (count - 1)) / 2;
		sumRow = sumCol;
		pathArr = new int[count];
	}
	private int sumCol;
	private int sumRow;
	private int[] pathArr;

	private readonly TextWriter _output;
	private readonly double[,] _mx;

	public void Accept((int src, int dst) coord)
	{
		_output.WriteLine($"{coord.src} --[{_mx[coord.src, coord.dst]}]--> {coord.dst}");
		pathArr[coord.src] = coord.dst;
		sumCol -= coord.dst;
		sumRow -= coord.src;
	}

	public int[] GetFullPath()
	{
		pathArr[sumRow] = sumCol;
		return pathArr;
	}
}