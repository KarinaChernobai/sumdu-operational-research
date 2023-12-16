using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

public struct BellmanEq
{
	const string DoubleFormat = "0.##############";

	double[] _inv;
	double[,] _mtrx;
	int[] _invested;
	int TargetCount => _mtrx.GetLength(1);
	int SampleCount => _mtrx.GetLength(0);

	(double Inv, double Reward) GetTargetInv(int targetInx)
	{
		var sampleInx = _invested[targetInx];
		return sampleInx < 0 
			? (double.NaN, double.NaN)
			: (_inv[sampleInx], _mtrx[sampleInx, targetInx]);
	}
	readonly ILogWriter _logWriter;

	private BellmanEq(double[] inv, double[,] mtrx, ILogWriter logWriter)
	{
		_inv = inv;
		_mtrx = mtrx;
		_invested = new int[TargetCount];
		for (var i = 0; i < _invested.Length; i++) _invested[i] = -1;
		_logWriter = logWriter;
	}

	public static void Solve(double[] inv, double[,] mtrx, double r, ILogWriter logWriter)
	{
		if (inv.Length != mtrx.GetLength(0)) throw new ArgumentException("args wrong");
		if (r < inv[0]) throw new ArgumentException("args wrong");
		
		var bellman = new BellmanEq(inv, mtrx, logWriter);
		var availableInv = r;
		for (var targetInx = 0; targetInx < bellman.TargetCount; targetInx++)
		{
			var (sampeInx, value) = bellman.ValueFn(targetInx, availableInv); // 10.8
			if (sampeInx < 0) break;
			availableInv -= bellman._inv[sampeInx];
			bellman._invested[targetInx] = sampeInx;
		}
		bellman.Log(r);
	}

	(int MaxSampleInx, double MaxValue) ValueFn(int targetInx, double r)
	{
		if (targetInx == TargetCount - 1) return ValueFn1(targetInx, r);

		var maxVal = r;
		var maxSampleInx = -1;
		for (int sampleInx = 0; sampleInx < SampleCount; sampleInx++)
        {
			var remainder = r - _inv[sampleInx];
			if (remainder < 0) break;
			var value = _mtrx[sampleInx, targetInx] + ValueFn(targetInx + 1, remainder).MaxValue;
			if (value > maxVal)
			{
				maxVal = value;
				maxSampleInx = sampleInx;
			}
		}
		return (maxSampleInx, maxVal);
    }

	(int MaxSampleInx, double MaxValue) ValueFn1(int targetInx, double r)
	{
		var maxValue = r;
		var maxSampleInx = -1;
		for (int sampleInx = 0; sampleInx < _inv.Length; sampleInx++)
		{
			var remainder = r - _inv[sampleInx];
			if (remainder < 0) break;
			var value = _mtrx[sampleInx, targetInx] + remainder;
			if (value > maxValue)
			{
				maxValue = value;
				maxSampleInx = sampleInx;
			}
		}
		return (maxSampleInx, maxValue);
	}

	void Log(double r)
	{
		const int ColRem = 0;
		const int ColInv = 1;
		const int ColProfit = 2;
		const int ColTotal = 3;

		var data = new string[2, TargetCount + 4];

		var row = data.GetRowSpan(0);
		for (var i = 0; i < TargetCount; i++) row[i] = $"t{i}";
		row = row.Slice(TargetCount);
		row[ColRem] = "rem";
		row[ColInv] = "inv";
		row[ColProfit] = "profit";
		row[ColTotal] = "total";

		row = data.GetRowSpan(1);
		var totalReward = 0d;
		var appliedInv = 0d;
		for (var i = 0; i < TargetCount; i++)
		{
			var (inv, reward) = GetTargetInv(i);
			if (!double.IsNaN(inv))
			{
				appliedInv += inv;
				totalReward += reward;
				row[i] = reward.ToString(DoubleFormat);
			}
		}
		var remainder = r - appliedInv;
		row = row.Slice(TargetCount);
		row[ColRem] = remainder.ToString(DoubleFormat);
		row[ColInv] = appliedInv.ToString(DoubleFormat);
		row[ColProfit] = (totalReward - appliedInv).ToString(DoubleFormat);
		row[ColTotal] = (totalReward + remainder).ToString(DoubleFormat);

		_logWriter.GetWriter().WriteTable(data, [false], []);
	}
}
