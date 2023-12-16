using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

public struct Simplex
{
	const string DoubleFormat = "0.######";
	double[,] _tableau;
	int TableauHeight => _tableau.GetLength(0);
	int TableauWidth => _tableau.GetLength(1);

	int _xCount;
	int _slackCount;
	double[] _objFunc;
	double[,] _constrMx;
	ILogWriter? _logWriter;

	private Simplex(double[] func, double[,] constrMx, ILogWriter? logWriter)
    {
		_logWriter = logWriter;
		_objFunc = func;
		_constrMx = constrMx;
		_slackCount = constrMx.GetLength(0);
		_xCount = func.Length;

		var m = constrMx.GetLength(1);
		if (_xCount != m - 1) throw new ArgumentException($"The number of variables specified by '{nameof(func)}' does not match the matrix '{nameof(constrMx)}'", nameof(constrMx));
		
		_tableau = new double[_slackCount + 1, m + _slackCount];
		FillObjectiveRow();
		FillConstraints();
		FillSlackVars();
	}

	void FillObjectiveRow()
	{
		var row = _tableau.GetRowSpan(0);
		for (var i = 0; i < _objFunc.Length; i++)
		{
			row[i] = _objFunc[i];
		}
	}

	void FillConstraints()
	{
		var solIndex = TableauWidth - 1;
		var span = new Span2D<double>(_tableau, 1, 0, _slackCount, TableauWidth); 
		for (var rowInx = 0; rowInx < _slackCount; rowInx++)
		{
			var row = span.GetRowSpan(rowInx);
			for (var colInx = 0; colInx < _xCount; colInx++)
			{
				row[colInx] = _constrMx[rowInx, colInx];
			}
			row[solIndex] = _constrMx[rowInx, _xCount];
		}
	}

	void FillSlackVars()
	{
		var span = new Span2D<double>(_tableau, 1, _xCount, _slackCount, _slackCount);
		for (var rowInx = 0; rowInx < _slackCount; rowInx++)
		{
			for (var colInx = 0; colInx <= _slackCount; colInx++)
			{
				if (rowInx == colInx) span[rowInx, colInx] = 1;
			}
		}
	}

	// maximize, minimize
	public static double[] Solve(double[] func, double[,] constraints, ILogWriter? logWriter = null)
	{
		
		var simplex = new Simplex(func, constraints, logWriter);
		simplex.Log();
		while(true)
		{
			var pivot = simplex.ChoosePivot();
			if (pivot.rowInx == -1) break;
			Console.WriteLine($"Pivot: {pivot.rowInx} {pivot.colInx}");
			simplex.ChangeMx(pivot);
			simplex.Log();
		}
		var solution = simplex.GetSolution();
		return solution;
    }

	(int rowInx, int colInx) ChoosePivot()
	{
		var objRow = _tableau.GetRowSpan(0);
		var min = Double.PositiveInfinity;
		var pivotColInx = 0;
		for (int colInx = 0; colInx < objRow.Length; colInx++)
		{
			var value = objRow[colInx];
			if (min > value)
			{
				min = value;
				pivotColInx = colInx;
			}
		}
		if (min >= 0) return (-1, -1);
		min = Double.PositiveInfinity;
		var pivotRowInx = 0;
		for (var rowIndex = 1; rowIndex < TableauHeight; rowIndex++)
		{
			var value = _tableau[rowIndex, pivotColInx];
			if (value <= 0) continue;
            var rhs = _tableau[rowIndex, TableauWidth - 1]; // right hand side, solution
			rhs /= value;
			if (rhs < 0) continue;
			if (min > rhs)
			{
				min = rhs;
				pivotRowInx = rowIndex;
			}
		}
		return (pivotRowInx, pivotColInx);
	}

	void ChangeMx((int rowInx, int colInx) pivot)
	{
		var pivotVal = _tableau[pivot.rowInx, pivot.colInx];
        for (var colInx = 0; colInx < _tableau.GetLength(1); colInx++)
        {
			_tableau[pivot.rowInx, colInx] /= pivotVal;
        }

        for (var rowInx = 0; rowInx < _tableau.GetLength(0); rowInx++)
        {
			if (rowInx == pivot.rowInx) continue;
			var value = _tableau[rowInx, pivot.colInx] * (-1);
			for (var colInx = 0; colInx < _tableau.GetLength(1); colInx++)
            {
				var exmpl = _tableau[rowInx, colInx];
				_tableau[rowInx, colInx] += value * _tableau[pivot.rowInx, colInx];
			}
        }
    }

	double[] GetSolution()
	{
		double[] solution = new double[_xCount];
		for (var rowInx = 1; rowInx < _tableau.GetLength(0); rowInx++)
		{
			for (var colInx = 0; colInx < _xCount; colInx++)
			{
				if (_tableau[rowInx, colInx] == 1)
				{
					solution[colInx] = _tableau[rowInx, _tableau.GetLength(1) - 1];
					break;
				}
			}
		}
		return solution;
	}

    void Log()
    {
		if (_logWriter == null) return;
        var data = new string[TableauHeight + 1, TableauWidth + 1];
		DataTableColHeaders(data);
		DataTableRowHeaders(data);
		DataTableTableau(data);
		_logWriter.GetWriter().WriteTable(data, [true, false], [true]);
		_logWriter.EndMessage();
	}

	void DataTableColHeaders(Span2D<string> data)
	{
		var row = data.GetRowSpan(0).Slice(1);
		for (int i = 0; i < _xCount; i++)
		{
			row[i] = "x" + i.ToString();
		}
		row = row.Slice(_xCount);
		for (int i = 0; i < _slackCount; i++)
		{
			row[i] = "s" + i.ToString();
		}
		row[row.Length - 1] = "RHS";
	}

	void DataTableRowHeaders(Span2D<string> data)
	{
		data[1, 0] = "z";
		var dataSpan = data.Slice(2, 0, _slackCount, 1);
		var slackInx = 0;
		foreach (ref var col in dataSpan.GetColumn(0))
		{
			col = "s" + slackInx;
			slackInx++;
		}
	}

	void DataTableTableau(Span2D<string> data)
	{
		data = data.Slice(1, 1, TableauHeight, TableauWidth);
		for (var rowIndex = 0; rowIndex < TableauHeight; rowIndex++)
		{
			var dataRow = data.GetRow(rowIndex);
			var tableauRow = _tableau.GetRow(rowIndex);
			for (var colIndex = 0; colIndex < TableauWidth; colIndex++)
			{
				dataRow[colIndex] = tableauRow[colIndex].ToString(DoubleFormat);
			}
		}
	}
}
