using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

public interface IConsumer<T>
{
	void Accept(T value);
}

public readonly struct BoolMatrix(int rowCount, int columnCount)
{
	private readonly BitArray _array = new BitArray(rowCount * columnCount);
	public readonly int RowCount = rowCount;
	public readonly int ColumnCount = columnCount;

	public bool this[int row, int column]
	{
		get => _array[row * ColumnCount + column];
		set => _array[row * ColumnCount + column] = value;
	}

	public Segment GetRow(int row) => new Segment(_array, row * ColumnCount);

	public readonly struct Segment(BitArray array, int offset)
	{
		private readonly BitArray _array = array;
		private readonly int _offset = offset;

		public bool this[int index]
		{
			get => _array[_offset + index];
			set => _array[_offset + index] = value;
		}
	}

	public Cursor GetColumn(int column) => new Cursor(_array, ColumnCount, column);

	public struct Cursor(BitArray array, int columnCount, int offset)
	{
		private readonly BitArray _array = array;
		private int _columnCount = columnCount;
		private int _offset = offset;

		public bool Value
		{
			get => _array[_offset];
			set => _array[_offset] = value;	
		}
		public void MoveToNextRow() => _offset += _columnCount;
	}
}


public ref struct TSP2
{
	const double Eps = 0.000001;
	const string DoubleFormat = "0.######";

	struct Stat
	{
		public double Term;
		public double Min { get => Term; set => Term = value; }
		public int MinCount;
		public double Min2;
		public int MxIndex;
		public int NoPathCellCount;

		public void Reset()
		{
			Term = double.NaN;
			MinCount = 0;
			Min2 = double.NaN;
			NoPathCellCount = 0;
		}
	}

	struct StatItem
	{
		public Stat Row;
		public Stat Column;
	}

	struct StatList
	{
		public StatList(int size)
		{
			_rowCount = size;
			_columnCount = size;
			_list = new StatItem[size];
			NoPathMx = new BoolMatrix(size, size);
			for (var i = 0; i < size; i++)
			{
				ref var item = ref _list[i];
				item.Row.MxIndex = i;
				item.Row.Reset();
				item.Column.MxIndex = i;
				item.Column.Reset();
				NoPathMx[i, i] = true;
			}
		}

		private readonly StatItem[] _list;
		public readonly BoolMatrix NoPathMx;

		private int _rowCount;
		public readonly int RowCount => _rowCount;
		private int _columnCount;
		public readonly int ColumnCount => _columnCount;

		public readonly ref Stat GetRow(int rowIndex) => ref _list[rowIndex].Row;
		public readonly ref Stat GetColumn(int columnIndex) => ref _list[columnIndex].Column;

		public readonly double GetSum(CellCoords coord) => _list[coord.Row].Row.Term + _list[coord.Column].Column.Term;

		public (int src, int dst) UsePath(CellCoords coord)
		{
			ref var row = ref GetRow(coord.Row);
			var src = row.MxIndex;
			ref var column = ref GetColumn(coord.Column);
			var dst = column.MxIndex;
			NoPathMx[src, dst] = true;
			NoPathMx[dst, src] = true;
			if (row.NoPathCellCount > 0)
			{
				var lastRowIndex = _rowCount - 1;
				if (coord.Row < lastRowIndex)
				{
					_list[coord.Row].Row.MxIndex = _list[lastRowIndex].Row.MxIndex;
				}
				_rowCount--;
			}
			if (column.NoPathCellCount > 0)
			{
				var lastColumnIndex = _columnCount - 1;
				if (coord.Column < lastColumnIndex)
				{
					_list[coord.Column].Column.MxIndex = _list[lastColumnIndex].Column.MxIndex;
				}
				_columnCount--;
			}

			var len = _columnCount > _rowCount ? _columnCount : _rowCount;
			for (var i = 0; i < len; i++)
			{
				ref var c = ref _list[i];
				c.Row.Reset();
				c.Column.Reset();
			}

			return (src, dst);
		}
	}

	readonly struct CellCoords
	{
		public CellCoords(int row, int column) => (Row, Column) = (row, column);
		public readonly int Row;
		public readonly int Column;

		public override string ToString() => $"{Row}; {Column}";
	}

	struct ZeroCellList
	{
		private readonly List<CellCoords> _list = new List<CellCoords>();
		private int _setIndex = -1;
		private int _lastSetItemCount;
		private bool _isColumnSet;
		public ZeroCellList() { }

		public readonly int LastRowItemCount => _lastSetItemCount;
		public readonly CellCoords this[int index] => _list[index];
		public readonly int Count => _list.Count;

		public void ClearLastSet()
		{
			_list.RemoveRange(_setIndex, _lastSetItemCount);
			_lastSetItemCount = 0;
		}

		public void Clear()
		{
			_list.Clear();
			_setIndex = -1;
			_lastSetItemCount = 0;
		}

		public void StartColumnSet(int row)
		{
			_isColumnSet = true;
			_setIndex = row;
			_lastSetItemCount = 0;
		}

		public void AddColumn(int column)
		{
			if (_setIndex < 0) throw new InvalidOperationException("A column set is not started.");
			if (!_isColumnSet) throw new InvalidOperationException("Column may not be added to the row set.");
			_list.Add(new CellCoords(_setIndex, column));
			_lastSetItemCount++;
		}

		public void StartRowSet(int column)
		{
			_isColumnSet = false;
			_setIndex = column;
			_lastSetItemCount = 0;
		}

		public void AddRow(int row)
		{
			if (_setIndex < 0) throw new InvalidOperationException("A row set is not started.");
			if (_isColumnSet) throw new InvalidOperationException("A row may not be added to the column set.");
			_list.Add(new CellCoords(row, _setIndex));
			_lastSetItemCount++;
		}
	}

	public static void Solve<TConsumer>(ReadOnlySpan2D<double> matrix, TConsumer pathConsumer, ILogWriter? logWriter = null) where TConsumer : IConsumer<(int src, int dst)>
	{
		if (matrix.Width < 2) throw new ArgumentException("The size of the distance matrix must be greater than 1.", nameof(matrix));
		if (matrix.Width != matrix.Height) throw new ArgumentException($"The distance matrix must be square. Width ({matrix.Width}) != Hieght ({matrix.Height})", nameof(matrix));
		var alg = new TSP2(matrix, logWriter);

		alg.Log();

		var rowCount = alg._statList.RowCount;
		var columnCount = alg._statList.ColumnCount;

		for (var iteration = matrix.Width-1; iteration > 0; iteration--)
		{
			for (var i = 0; i < rowCount; i++)
			{
				alg.ProcessRow(i);
			}

			for (var i = 0; i < columnCount; i++)
			{
				alg.ProcessColumn(i);
			}

			alg.SetRowsTerm();

			var len = alg._minCellList.Count;
			var minCoord = alg._minCellList[0];
			var maxSum = alg._statList.GetSum(minCoord);
			for (var i = 1; i < len; i++)
			{
				var coord = alg._minCellList[i];
				var sum = alg._statList.GetSum(coord);
				if (sum > maxSum)
				{
					maxSum = sum;
					minCoord = coord;
				}
			}
			pathConsumer.Accept(alg._statList.UsePath(minCoord));
			alg._minCellList.Clear();
			rowCount = alg._statList.RowCount;
			columnCount = alg._statList.ColumnCount;
			alg.Log();
		}
	}

	private TSP2(ReadOnlySpan2D<double> matrix, ILogWriter? logWriter)
	{
		_logWriter = logWriter;
		_mx = matrix;
		var size = matrix.Width;
		_statList = new StatList(size);
		_minCellList = new ZeroCellList();
	}

	private readonly ReadOnlySpan2D<double> _mx;
	private StatList _statList;
	private ZeroCellList _minCellList;
	private ILogWriter? _logWriter;

	void ProcessRow(int rowIndex)
	{
		var columnCount = _statList.ColumnCount;
		ref var rowStat = ref _statList.GetRow(rowIndex);
		var matrixRow = _mx.GetRowSpan(rowStat.MxIndex);
		var noPathRow = _statList.NoPathMx.GetRow(rowStat.MxIndex);
		_minCellList.StartColumnSet(rowIndex);

		var columnIndex = 0;
		var min = default(double);
		for (; columnIndex < columnCount; columnIndex++)
		{
			ref var columnStat = ref _statList.GetColumn(columnIndex);
			if (!noPathRow[columnStat.MxIndex])
			{
				min = matrixRow[columnStat.MxIndex];
				_minCellList.AddColumn(columnIndex);
				columnIndex++;
				break;
			}
			rowStat.NoPathCellCount++;
			columnStat.NoPathCellCount++;
		}

		var min2 = double.NaN;

		for (; columnIndex < columnCount; columnIndex++)
		{
			ref var columnStat = ref _statList.GetColumn(columnIndex);
			if (noPathRow[columnStat.MxIndex])
			{
				rowStat.NoPathCellCount++;
				columnStat.NoPathCellCount++;
				continue;
			}
			var v = matrixRow[columnStat.MxIndex];
			// v ≈ min
			if (Math.Abs(v - min) < Eps)
			{
				_minCellList.AddColumn(columnIndex);
			}
			else if (v < min)
			{
				min2 = min;
				min = v;
				_minCellList.ClearLastSet();
				_minCellList.AddColumn(columnIndex);
			}
			else if (v < min2 || double.IsNaN(min2)) min2 = v;
		}

		rowStat.Min = min;
		rowStat.MinCount = _minCellList.LastRowItemCount;
		rowStat.Min2 = min2;

		for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
		{
			ref var columnStat = ref _statList.GetColumn(columnIndex);
			if (noPathRow[columnStat.MxIndex]) continue;
			var v = matrixRow[columnStat.MxIndex];
			v -= min;
			if (double.IsNaN(columnStat.Min))
			{
				columnStat.Min = v < Eps ? 0 : v;
				columnStat.MinCount = 1;
			}
			else if (Math.Abs(v - columnStat.Min) < Eps)
			{
				columnStat.MinCount++;
			}
			else if (v < columnStat.Min)
			{
				columnStat.Min2 = columnStat.Min;
				columnStat.Min = v;
				columnStat.MinCount = 1;
			}
			else if (v < columnStat.Min2 || double.IsNaN(columnStat.Min2)) columnStat.Min2 = v;
		}
	}

	void ProcessColumn(int columnIndex)
	{
		ref var columnStat = ref _statList.GetColumn(columnIndex);
		if (columnStat.Min == 0)
		{
			columnStat.Term = columnStat.MinCount > 1 ? 0 : columnStat.Min2;
			return;
		}
		var noPathColumn = _statList.NoPathMx.GetColumn(columnStat.MxIndex);
		_minCellList.StartRowSet(columnIndex);
		var rowCount = _statList.RowCount;
		for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{
			if (noPathColumn.Value)
			{
				noPathColumn.MoveToNextRow();
				continue;
			}
			noPathColumn.MoveToNextRow();
			ref var rowStat = ref _statList.GetRow(rowIndex);
			var v = _mx[rowStat.MxIndex, columnStat.MxIndex];
			v -= rowStat.Min + columnStat.Min;
			// v ≈ 0
			if (v < Eps)
			{
				rowStat.MinCount++;
				_minCellList.AddRow(rowIndex);
			}
		}
		columnStat.Term = columnStat.MinCount > 1 ? 0 : (columnStat.Min2 - columnStat.Min);
	}

	void SetRowsTerm()
	{
		var rowCount = _statList.RowCount;
		for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
		{
			ref var rowStat = ref _statList.GetRow(rowIndex);
			rowStat.Term = rowStat.MinCount > 1 ? 0 : (rowStat.Min2 - rowStat.Min);
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

		for (var rowIndex = 0; rowIndex < _statList.RowCount; rowIndex++)
		{
			ref var rowStat = ref _statList.GetRow(rowIndex);
			array[rowIndex] = (rowIndex, rowStat.MxIndex);
		}
		{
			var span = array.Slice(0, _statList.RowCount);
			span.Sort(static (item1, item2) => item1.mxIndex.CompareTo(item2.mxIndex));
			for (var i = 0; i < span.Length; i++) map[span[i].statIndex].Row = i;
		}

		for (var columnIndex = 0; columnIndex < _statList.ColumnCount; columnIndex++)
		{
			ref var columnStat = ref _statList.GetColumn(columnIndex);
			array[columnIndex] = (columnIndex, columnStat.MxIndex);
		}
		{
			var span = array.Slice(0, _statList.ColumnCount);
			span.Sort(static (item1, item2) => item1.mxIndex.CompareTo(item2.mxIndex));
			for (var i = 0; i < span.Length; i++) map[span[i].statIndex].Column = i;
		}
	}

	void Log()
	{
		if (_logWriter == null) return;
		var table = new string[_statList.RowCount+1, _statList.ColumnCount+1];
		var map = (Span<StatIndex>)stackalloc StatIndex[_statList.RowCount > _statList.ColumnCount ? _statList.RowCount : _statList.ColumnCount];
		InitStatIndexMap(map);
		FillData(table, map);
		_logWriter.GetWriter().Write('\n');
		_logWriter.GetWriter().WriteTable(table, [false], []);
		_logWriter.EndMessage();
	}

	void FillData(Span2D<string> table, scoped ReadOnlySpan<StatIndex> map)
	{
		{
			var tableRow = table.GetRowSpan(0).Slice(1);
			for (var columnIndex = 0; columnIndex < _statList.ColumnCount; columnIndex++)
			{
				tableRow[map[columnIndex].Column] = _statList.GetColumn(columnIndex).MxIndex.ToString();
			}
		}
		table = table.Slice(1, 0, table.Height - 1, table.Width);
		for (var rowIndex = 0; rowIndex < _statList.RowCount; rowIndex++)
		{
			ref var rowStat = ref _statList.GetRow(rowIndex);
			var matrixRow = _mx.GetRowSpan(rowStat.MxIndex);
			var noPathRow = _statList.NoPathMx.GetRow(rowStat.MxIndex);
			var tableRow = table.GetRowSpan(map[rowIndex].Row);
			table[map[rowIndex].Row, 0] = rowStat.MxIndex.ToString();
			tableRow = tableRow.Slice(1);
			for (var columnIndex = 0; columnIndex < _statList.ColumnCount; columnIndex++)
			{
				ref var columnStat = ref _statList.GetColumn(columnIndex);
				tableRow[map[columnIndex].Column] = noPathRow[columnStat.MxIndex] ? "M" : matrixRow[columnStat.MxIndex].ToString(DoubleFormat);
			}
		}
	}
}