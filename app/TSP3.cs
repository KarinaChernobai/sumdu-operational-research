using System;
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


public ref struct Tsp3
{
	const double Eps = 0.000001;

	struct RowStat
	{
		public double Term;
		public double Min { get => Term; set => Term = value; }
		public int MinCount;
		public double Min2;
		public int MxIndex;

		public void Reset()
		{
			Term = double.NaN;
			MinCount = 0;
			Min2 = double.NaN;
		}
	}

	struct ColumnStat
	{
		public double Term;
		public double Min { get => Term; set => Term = value; }
		public double Min2;
		public int MinCount;
		public int MxIndex;

		public void Reset()
		{
			Term = double.NaN;
			MinCount = 0;
			Min2 = double.NaN;
		}
	}

	struct StatItem
	{
		public RowStat Row;
		public ColumnStat Column;
	}

	struct Stat
	{
		public Stat(int size)
		{
			_size = size;
			_list = new StatItem[size];
			for (var i = 0; i < size; i++)
			{
				ref var item = ref _list[i];
				item.Row.MxIndex = i;
				item.Row.Reset();
				item.Column.MxIndex = i;
				item.Column.Reset();
			}
		}

		private readonly StatItem[] _list;
		private int _size;

		public readonly int Size => _size;

		public readonly ref RowStat GetRow(int rowIndex) => ref _list[rowIndex].Row;
		public readonly ref ColumnStat GetColumn(int columnIndex) => ref _list[columnIndex].Column;

		public readonly double GetSum(CellCoords coord) => _list[coord.Row].Row.Term + _list[coord.Column].Column.Term;

		public (int src, int dst) UsePath(CellCoords coord)
		{
			var lastIndex = _size - 1;
			var src = _list[coord.Row].Row.MxIndex;
			var dst = _list[coord.Column].Column.MxIndex;
			if (coord.Row < lastIndex)
			{
				_list[coord.Row].Row.MxIndex = _list[lastIndex].Row.MxIndex;
			}
			if (coord.Column < lastIndex)
			{
				_list[coord.Column].Column.MxIndex = _list[lastIndex].Column.MxIndex;
			}
			_size--;

			for (var i = 0; i < _size; i++)
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

		public int LastRowItemCount => _lastSetItemCount;
		public CellCoords this[int index] => _list[index];
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

	public static void Solve<T>(ReadOnlySpan2D<double> matrix, T pathConsumer) where T : IConsumer<(int src, int dst)>
	{
		if (matrix.Width < 2) throw new ArgumentException("The size of the distance matrix must be greater than 1.", nameof(matrix));
		if (matrix.Width != matrix.Height) throw new ArgumentException($"The distance matrix must be square. Width ({matrix.Width}) != Hieght ({matrix.Height})", nameof(matrix));
		var alg = new Tsp3(matrix);
		var size = alg._stat.Size;

		do
		{
			for (var i = 0; i < size; i++)
			{
				alg.ProcessRow(i);
			}

			for (var i = 0; i < size; i++)
			{
				alg.ProcessColumn(i);
			}

			alg.SetRowsTerm();

			var len = alg._minCellList.Count;
			var minCoord = alg._minCellList[0];
			var maxSum = alg._stat.GetSum(minCoord);
			for (var i = 1; i < len; i++)
			{
				var coord = alg._minCellList[i];
				var sum = alg._stat.GetSum(coord);
				if (sum > maxSum)
				{
					maxSum = sum;
					minCoord = coord;
				}
			}
			pathConsumer.Accept(alg._stat.UsePath(minCoord));
			alg._minCellList.Clear();
			size = alg._stat.Size;
		}
		while (size > 1);
	}

	private Tsp3(ReadOnlySpan2D<double> matrix)
	{
		_matrix = matrix;
		_stat = new Stat(matrix.Width);
		_minCellList = new ZeroCellList();
	}

	private readonly ReadOnlySpan2D<double> _matrix;
	private Stat _stat;
	private ZeroCellList _minCellList;

	void ProcessRow(int rowIndex)
	{
		var size = _stat.Size;
		var matrixRow = _matrix.GetRowSpan(rowIndex);
		_minCellList.StartColumnSet(rowIndex);
		var columnIndex = 0;
		var min = default(double);
		for (; columnIndex < size; columnIndex++)
		{
			ref var columnStat = ref _stat.GetColumn(columnIndex);
			var v = matrixRow[columnStat.MxIndex];
			if (!double.IsNaN(v))
			{
				min = v;
				_minCellList.AddColumn(columnIndex);
				columnIndex++;
				break;
			}
		}

		var min2 = double.NaN;

		for (; columnIndex < size; columnIndex++)
		{
			ref var columnStat = ref _stat.GetColumn(columnIndex);
			var v = matrixRow[columnStat.MxIndex];
			if (double.IsNaN(v)) continue;
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

		ref var rowStat = ref _stat.GetRow(rowIndex);
		rowStat.Min = min;
		rowStat.MinCount = _minCellList.LastRowItemCount;
		rowStat.Min2 = min2;

		for (columnIndex = 0; columnIndex < size; columnIndex++)
		{
			ref var columnStat = ref _stat.GetColumn(columnIndex);
			var v = matrixRow[columnStat.MxIndex];
			if (double.IsNaN(v)) continue;
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
		var size = _stat.Size;
		ref var columnStat = ref _stat.GetColumn(columnIndex);
		if (columnStat.Min == 0)
		{
			columnStat.Term = columnStat.MinCount > 1 ? 0 : columnStat.Min2;
			return;
		}
		_minCellList.StartRowSet(columnIndex);
		for (var rowIndex = 0; rowIndex < size; rowIndex++)
		{
			ref var rowStat = ref _stat.GetRow(rowIndex);
			var v = _matrix[rowStat.MxIndex, columnStat.MxIndex];
			if (double.IsNaN(v)) continue;
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
		var size = _stat.Size;
		for (var rowIndex = 0; rowIndex < size; rowIndex++)
		{
			ref var rowStat = ref _stat.GetRow(rowIndex);
			rowStat.Term = rowStat.MinCount > 1 ? 0 : (rowStat.Min2 - rowStat.Min);
		}
	}
}