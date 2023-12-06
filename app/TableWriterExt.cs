using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

static class TableWriterExt
{
	const char NL = '\n';

	public static void WriteTable(this TextWriter writer, Span2D<string> data, ReadOnlySpan<bool?> hrBorders, ReadOnlySpan<bool> vrBorders)
	{
		var columnsWidth = (Span<int>)stackalloc int[data.Width];
		var context = new Context(writer, data, hrBorders, vrBorders, columnsWidth);

		context.WriteTableRow(0);
		for (var rowIndex = 1; rowIndex < data.Height; rowIndex++)
		{
			context.WriteHrSeparator(rowIndex-1);
			context.WriteTableRow(rowIndex);
		}
	}

	readonly ref struct Context
	{
		const char SHrBar = '─';
		const char SVrBar = '│';
		const char DHrBar = '═';
		const char DVrBar = '║';

		private static readonly char[] _cross = [ '┼', '╫', '╪', '╬' ];
		private static char Cross(bool hrBorder, bool vrBorder) => _cross[(hrBorder ? 2 : 0) + (vrBorder ? 1 : 0)];

		private readonly TextWriter _writer;
		private readonly Span2D<string> _data;
		private readonly ReadOnlySpan<bool?> _hrBorders;
		private readonly ReadOnlySpan<bool> _vrBorder;
		private readonly Span<int> _columnsWidth;

		public Context(TextWriter writer, Span2D<string> data, ReadOnlySpan<bool?> hrBorders, ReadOnlySpan<bool> vrBorder, Span<int> columnsWidth)
		{ 
			_writer = writer;
			_data = data;
			_hrBorders = hrBorders;
			_vrBorder = vrBorder;
			_columnsWidth = columnsWidth;
			CalcColumnWidth();
		}

		private void CalcColumnWidth()
		{
			for (var rowIndex = 0; rowIndex < _data.Height; rowIndex++)
			{
				var row = _data.GetRowSpan(rowIndex);
				for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
				{
					ref var cell = ref row[columnIndex];
					if (cell == null) cell = string.Empty;
					else
					{
						ref var width = ref _columnsWidth[columnIndex];
						if (width < cell.Length) width = cell.Length;
					}
				}
			}
		}

		public void WriteTableRow(int rowIndex)
		{
			var row = _data.GetRowSpan(rowIndex);
			{
				var str = row[0];
				var d = _columnsWidth[0] - str.Length;
				WriteSpace(d);
				_writer.Write(str);
			}
			
			for (var columnIndex = 1; columnIndex < row.Length; columnIndex++)
			{
				var i = columnIndex - 1;
				_writer.Write(i < _vrBorder.Length && _vrBorder[i] ? DVrBar : SVrBar);
				var str = row[columnIndex];
				var d = _columnsWidth[columnIndex] - str.Length;
				WriteSpace(d);
				_writer.Write(str);
			}
			_writer.Write(NL);
		}

		public void WriteHrSeparator(int rowIndex)
		{
			if (rowIndex >= _hrBorders.Length) return;
			if (_hrBorders[rowIndex] is not bool isDHr) return;
			var hrBorder = isDHr ? DHrBar : SHrBar;
			WriteHrBar(_columnsWidth[0], hrBorder);
			for (var columnIndex = 1; columnIndex < _columnsWidth.Length; columnIndex++)
			{
				var i = columnIndex - 1;
				_writer.Write(Cross(isDHr, i < _vrBorder.Length && _vrBorder[i]));
				WriteHrBar(_columnsWidth[columnIndex], hrBorder);
			}
			_writer.Write(NL);
		}


		private void WriteSpace(int length)
		{
			for (; length > 0; length--) _writer.Write(' ');
		}

		private void WriteHrBar(int length, char hrBar)
		{
			for (; length > 0; length--) _writer.Write(hrBar);
		}
	}
}
