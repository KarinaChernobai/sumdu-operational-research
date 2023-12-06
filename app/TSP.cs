using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

public class TSP
{
	const string DoubleFormat = "0.######";
	struct Stat
	{
		public double Min;
		public int MinCount;
		public double Min2;
		public int MxIndex;
		public double Term
		{
			get { return MinCount > 1 ? 0 : Min2 - Min; } 
		}
		public override string ToString() => $"Min: {Min.ToStringPretty()}; MinCount: {MinCount}; Min2: {Min2.ToStringPretty()};";
	}

	struct StatItem
	{
		public Stat Row;
		public Stat Column;
	}

	int rowCount;
	int colCount;
	double[,] matrix;
	StatItem[] stats;
	List<(int Row, int Column)> zerosList = new List<(int, int)>();
	ILogWriter? _logWriter;

	public void Solve(double[,] initialMatrix, IConsumer<(int src, int dst)> consumer, ILogWriter? logWriter = null)
	{
		_logWriter = logWriter;
		var count = initialMatrix.GetLength(0);
		rowCount = count;
		colCount = count;
		CopyMatrix(initialMatrix);
		CreateStats(count);
		Log();
		for (; count > 1; count--)
		{
            for (var rowInx = 0; rowInx < rowCount; rowInx++)
            {
				ProcessRow(rowInx);
			}
			for (var colInx = 0; colInx < colCount; colInx++)
			{
				ProcessCol(colInx);
            }
			Log(true);
			Log(true, true);
			var coords = GetMaxSumCoords();
			consumer.Accept((stats[coords.Row].Row.MxIndex, stats[coords.Column].Column.MxIndex));
			DelRowColumn(coords);
			Reset();
			Log();
		}
		_logWriter = null;
	}

	private void CopyMatrix(double[,] initialMatrix)
	{
		var n = initialMatrix.GetLength(0);
		var m = initialMatrix.GetLength(1);
		matrix = new double[n, m];

		for (var i = 0; i < n; i++)
		{
			for (var j = 0; j < m; j++) matrix[i, j] = initialMatrix[i, j];
		}
	}

	private void CreateStats(int count)
	{
		stats = new StatItem[count];
        for (int i = 0; i < count; i++)
        {
			stats[i].Row.MxIndex = i;
			stats[i].Column.MxIndex = i;
        }
    }

	private void ProcessRow(int rowInx)
	{
		var min = Double.PositiveInfinity;
		var min2 = Double.PositiveInfinity;
		for (var colInx = 0; colInx < colCount; colInx++)
		{
			var value = matrix[stats[rowInx].Row.MxIndex, stats[colInx].Column.MxIndex];
			if (Double.IsNaN(value)) continue;
			if (min > value)
			{
				min2 = min;
				min = value;
			}
			else if (min2 > value) min2 = value;
		}
		stats[rowInx].Row.Min = min;
		stats[rowInx].Row.Min2 = min2;

		for (var colInx = 0; colInx < colCount; colInx++)
		{
			var value = matrix[stats[rowInx].Row.MxIndex, stats[colInx].Column.MxIndex];
			if (Double.IsNaN(value)) continue;
			value -= min;
            if (value == 0)
            {
				zerosList.Add((rowInx, colInx));
				stats[rowInx].Row.MinCount++;
				stats[colInx].Column.Min = 0;
				stats[colInx].Column.MinCount++;
			}
        }
    }

	private void ProcessCol(int colInx)
	{
		var minCount = stats[colInx].Column.MinCount;
		if (minCount > 1) return;
		if (minCount == 1)
		{
			var min2 = Double.PositiveInfinity;
			for (var rowInx = 0; rowInx < rowCount; rowInx++)
			{
				var value = matrix[stats[rowInx].Row.MxIndex, stats[colInx].Column.MxIndex] - stats[rowInx].Row.Min;
				if (Double.IsNaN(value) || value == 0) continue;
				if (value < min2) min2 = value;
			}
			stats[colInx].Column.Min2 = min2;
		}
		else 
		{
			var min = Double.PositiveInfinity;
			var min2 = Double.PositiveInfinity;
			for (var rowInx = 0; rowInx < rowCount; rowInx++)
			{
				var value = matrix[stats[rowInx].Row.MxIndex, stats[colInx].Column.MxIndex];
				if (Double.IsNaN(value)) continue;
				value -= stats[rowInx].Row.Min;
				if (value < min)
				{
					min2 = min;
					min = value;
				}
				else if (value < min2) min2 = value;
			}
			stats[colInx].Column.Min = min;
			stats[colInx].Column.Min2 = min2;

			for (var rowInx = 0; rowInx < rowCount; rowInx++)
			{
				var value = matrix[stats[rowInx].Row.MxIndex, stats[colInx].Column.MxIndex];
				if (Double.IsNaN(value)) continue;
				value -= stats[rowInx].Row.Min + stats[colInx].Column.Min;
				if (value == 0)
				{
					stats[colInx].Column.MinCount++;
					stats[rowInx].Row.MinCount++;
					zerosList.Add((colInx, rowInx));
				}
			}
		}
	}
	private (int Row, int Column) GetMaxSumCoords()
	{
		var maxSum = default(double);
		var coords = default((int RowInx, int ColInx));
        for (int i = 0; i < zerosList.Count; i++)
        {
			var inx = zerosList[i];
			var sum = stats[inx.Row].Row.Term + stats[inx.Column].Column.Term;
			if (maxSum < sum)
			{
				maxSum = sum;
				coords = inx;
			}
		}
		return coords;
    }

	private void DelRowColumn((int Row, int Column) coords)
	{
		var isRowDel = false;
		ref var rowMxIndex = ref stats[coords.Row].Row.MxIndex;
		ref var colMxIndex = ref stats[coords.Column].Column.MxIndex;

		for (int colInx = 0; colInx < colCount; colInx++)
        {
			var value = matrix[rowMxIndex, stats[colInx].Column.MxIndex];
			if (Double.IsNaN(value))
			{
				isRowDel = true;
				break;
			}
		}

		var isColDel = false;
        for (int rowInx = 0; rowInx < rowCount; rowInx++)
        {
			var value = matrix[stats[rowInx].Row.MxIndex, colMxIndex];
			if (Double.IsNaN(value))
			{
				isColDel = true;
				break;
			}
		}

		matrix[colMxIndex, rowMxIndex] = Double.NaN;

		if (isRowDel) 
		{
			rowMxIndex = stats[rowCount - 1].Row.MxIndex;
			rowCount--;
		}
        if (isColDel)
        {
			colMxIndex = stats[colCount - 1].Column.MxIndex;
			colCount--;
        }
		if (!isRowDel && !isColDel)
		{
			matrix[rowMxIndex, colMxIndex] = Double.NaN;
		}
    }

	private void Reset()
	{
		zerosList.Clear();
        for (int i = 0; i < stats.Length; i++)
        {
			stats[i].Column.Min = double.NaN;
			stats[i].Column.Min2 = double.NaN;
			stats[i].Column.MinCount = 0;

			stats[i].Row.Min = double.NaN;
			stats[i].Row.Min2 = double.NaN;
			stats[i].Row.MinCount = 0;
		}
    }

	void Log(bool withStats = false, bool calc = false)
	{
		if (_logWriter == null) return;
		var extra = withStats ? 5 : 1;
		var table = new string[rowCount + extra, colCount + extra];
		var map = (Span<StatIndex>)stackalloc StatIndex[rowCount > colCount ? rowCount : colCount];
		InitStatIndexMap(map);
		FillData(table, map, calc);
		if (withStats)
		{
			FillRowStats(table.AsSpan2D(0, colCount + 1, rowCount + 1, 4), map);
			FillColStats(table.AsSpan2D(rowCount + 1, 0, 4, colCount + 1));
		}
		_logWriter.GetWriter().Write('\n');
		var hrBorders = (Span<bool?>)stackalloc bool?[rowCount + 1];
		hrBorders[0] = false;
		hrBorders[rowCount] = true;
		var vrBorders = (Span<bool>)stackalloc bool[colCount + 1];
		vrBorders[colCount] = true;
		_logWriter.GetWriter().WriteTable(table, hrBorders, vrBorders);
		_logWriter.EndMessage();
	}

	void FillData(Span2D<string> table, ReadOnlySpan<StatIndex> map, bool calc)
	{
		{
			var tableRow = table.GetRowSpan(0).Slice(1);
			for (var columnIndex = 0; columnIndex < colCount; columnIndex++)
			{
				tableRow[map[columnIndex].Column] = stats[columnIndex].Column.MxIndex.ToString();
			}
		}
		table = table.Slice(1, 0, table.Height - 1, table.Width);
		for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{
			ref var rowStat = ref stats[rowIndex].Row;
			var matrixRow = matrix.GetRowSpan(rowStat.MxIndex);
			var tableRow = table.GetRowSpan(map[rowIndex].Row);
			table[map[rowIndex].Row, 0] = rowStat.MxIndex.ToString();
			tableRow = tableRow.Slice(1);
			if (calc)
			{
				var rowTerm = rowStat.Term; 
				for (var columnIndex = 0; columnIndex < colCount; columnIndex++)
				{
					var columnStat = stats[columnIndex].Column;
					var value = matrixRow[columnStat.MxIndex];
					if (double.IsNaN(value)) tableRow[map[columnIndex].Column] = " M ";
					else
					{
						value -= rowStat.Min + columnStat.Min;
						tableRow[map[columnIndex].Column] = value == 0
							? $">{(rowTerm + columnStat.Term).ToStringPretty()}<"
							: $" {value.ToStringPretty()} ";
					}
				}
			}
			else
			{
				for (var columnIndex = 0; columnIndex < colCount; columnIndex++)
				{
					var value = matrixRow[stats[columnIndex].Column.MxIndex];
					tableRow[map[columnIndex].Column] = double.IsNaN(value) ? "M" : value.ToString(DoubleFormat);
				}
			}
		}
	}

	void FillRowStats(Span2D<string> table, ReadOnlySpan<StatIndex> map)
	{
		var row = table.GetRowSpan(0);
		row[0] = "min";
		row[1] = "count";
		row[2] = "min2";
		row[3] = "term";
		table = table.Slice(1, 0, table.Height - 1, table.Width);
		for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{
			row = table.GetRowSpan(map[rowIndex].Row);
			ref var rowStat = ref stats[rowIndex].Row;
			row[0] = rowStat.Min.ToStringPretty();
			row[1] = rowStat.MinCount.ToString();
			row[2] = rowStat.Min2.ToStringPretty();
			row[3] = rowStat.Term.ToStringPretty();
		}

	}

	void FillColStats(Span2D<string> table)
	{
		var minRow = table.GetRowSpan(0);
		var countRow = table.GetRowSpan(1);
		var min2Row = table.GetRowSpan(2);
		var termRow = table.GetRowSpan(3);
		minRow[0] = "min";
		countRow[0] = "count";
		min2Row[0] = "min2";
		termRow[0] = "term";
		minRow = minRow.Slice(1);
		countRow = countRow.Slice(1);
		min2Row = min2Row.Slice(1);
		termRow = termRow.Slice(1);
		for (var columnIndex = 0; columnIndex < colCount; columnIndex++)
		{
			ref var columnStat = ref stats[columnIndex].Column;
			minRow[columnIndex] = columnStat.Min.ToStringPretty();
			countRow[columnIndex] = columnStat.MinCount.ToString();
			min2Row[columnIndex] = columnStat.Min2.ToStringPretty();
			termRow[columnIndex] = columnStat.Term.ToStringPretty();
		}
	}

	private struct StatIndex
	{
		public int Row;
		public int Column;
	}

	void InitStatIndexMap(scoped Span<StatIndex> map)
	{
		var array = (Span<(int statIndex, int mxIndex)>)stackalloc (int statIndex, int mxIndex)[map.Length];

		for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{
			ref var rowStat = ref stats[rowIndex].Row;
			array[rowIndex] = (rowIndex, rowStat.MxIndex);
		}
		{
			var span = array.Slice(0, rowCount);
			span.Sort(static (item1, item2) => item1.mxIndex.CompareTo(item2.mxIndex));
			for (var i = 0; i < span.Length; i++) map[span[i].statIndex].Row = i;
		}

		for (var columnIndex = 0; columnIndex < colCount; columnIndex++)
		{
			ref var columnStat = ref stats[columnIndex].Column;
			array[columnIndex] = (columnIndex, columnStat.MxIndex);
		}
		{
			var span = array.Slice(0, colCount);
			span.Sort(static (item1, item2) => item1.mxIndex.CompareTo(item2.mxIndex));
			for (var i = 0; i < span.Length; i++) map[span[i].statIndex].Column = i;
		}
	}
}
