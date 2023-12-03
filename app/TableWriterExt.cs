using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

static class TableWriterExt
{
	const char HrBar = '─';
	const char VrBar = '│';
	const char Cross = '┼';
	const char NL = '\n';

	public static void WriteTable(this TextWriter writer, Span2D<string> table)
	{
		var columnWidth = new int[table.Width];
		for (var rowIndex = 0; rowIndex < table.Height; rowIndex++)
		{
			var row = table.GetRowSpan(rowIndex);
			for (var i = 0; i < row.Length; i++)
			{
				if (row[i] == null) row[i] = string.Empty;
				else if (columnWidth[i] < row[i].Length) columnWidth[i] = row[i].Length;
			}
		}

		writer.WriteTableRow(columnWidth, table.GetRowSpan(0));
		writer.Write(NL);
		writer.WriteHrSeparator(columnWidth);
		writer.Write(NL); 
		for (var rowIndex = 1; rowIndex < table.Height; rowIndex++)
		{
			writer.WriteTableRow(columnWidth, table.GetRowSpan(rowIndex));
			writer.Write(NL);
		}
	}

	private static void WriteTableRow(this TextWriter writer, int[] columnWidth, ReadOnlySpan<string> row)
	{
		{
			var str = row[0];
			var d = columnWidth[0] - str.Length;
			writer.WriteSpace(d);
			writer.Write(str);
		}
		for (var i = 1; i < row.Length; i++)
		{
			writer.Write(VrBar);
			var str = row[i];
			var d = columnWidth[i] - str.Length;
			writer.WriteSpace(d);
			writer.Write(str);
		}
	}

	private static void WriteHrSeparator(this TextWriter writer, int[] columnWidth)
	{

		writer.WriteHrBar(columnWidth[0]);
		for (var i = 1; i < columnWidth.Length; i++)
		{
			writer.Write(Cross);
			writer.WriteHrBar(columnWidth[i]);
		}
	}

	private static void WriteSpace(this TextWriter writer, int length)
	{ 
		for (; length > 0; length--) writer.Write(' ');
	}

	private static void WriteHrBar(this TextWriter writer, int length)
	{
		for (; length > 0; length--) writer.Write(HrBar);
	}
}
