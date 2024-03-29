﻿using CommunityToolkit.HighPerformance;

namespace MathMethods;

public readonly struct BellmanEq
{
	const string DoubleFormat = "0.##############";

	readonly double[] _invOptions;
	readonly double[,] _samplesMx;
	readonly int[] _invPlan;
	readonly ILogWriter? _logWriter;

	readonly int TargetCount => _samplesMx.GetLength(1);
	readonly int SampleCount => _samplesMx.GetLength(0);

	readonly (double Inv, double Reward) GetTargetInv(int targetInx)
	{
		var sampleInx = _invPlan[targetInx];
		return sampleInx < 0 
			? (double.NaN, double.NaN)
			: (_invOptions[sampleInx], _samplesMx[sampleInx, targetInx]);
	}

	private BellmanEq(double[] inv, double[,] samplesMx, ILogWriter? logWriter)
	{
		_invOptions = inv;
		_samplesMx = samplesMx;
		_invPlan = new int[TargetCount];
		for (var i = 0; i < _invPlan.Length; i++) _invPlan[i] = -1;
		_logWriter = logWriter;
	}

	public static void Solve(double[] invOptions, double[,] samplesMx, double invAmount, ILogWriter? logWriter = null)
	{
		if (invOptions.Length != samplesMx.GetLength(0)) throw new ArgumentException("args wrong");
		if (invAmount < invOptions[0]) throw new ArgumentException("args wrong");
		
		var bellman = new BellmanEq(invOptions, samplesMx, logWriter);
		var availableInv = invAmount;
		for (var targetInx = 0; targetInx < bellman.TargetCount; targetInx++)
		{
			var (sampeInx, value) = bellman.ValueFn(targetInx, availableInv);
			if (sampeInx < 0) break;
			availableInv -= bellman._invOptions[sampeInx];
			bellman._invPlan[targetInx] = sampeInx;
		}
		bellman.Log(invAmount);
	}

	(int MaxSampleInx, double MaxValue) ValueFn(int targetInx, double invAmount)
	{
		if (targetInx == TargetCount - 1) return ValueFn1(targetInx, invAmount);

		var maxVal = invAmount;
		var maxSampleInx = -1;
		for (int sampleInx = 0; sampleInx < SampleCount; sampleInx++)
        {
			var remainder = invAmount - _invOptions[sampleInx];
			if (remainder < 0) break;
			var value = _samplesMx[sampleInx, targetInx] + ValueFn(targetInx + 1, remainder).MaxValue;
			if (value > maxVal)
			{ 
				maxVal = value;
				maxSampleInx = sampleInx;
			}
		}
		return (maxSampleInx, maxVal);
    }

	(int MaxSampleInx, double MaxValue) ValueFn1(int targetInx, double invAmount)
	{
		var maxValue = invAmount;
		var maxSampleInx = -1;
		for (int sampleInx = 0; sampleInx < _invOptions.Length; sampleInx++)
		{
			var remainder = invAmount - _invOptions[sampleInx];
			if (remainder < 0) break;
			var value = _samplesMx[sampleInx, targetInx] + remainder;
			if (value > maxValue)
			{
				maxValue = value;
				maxSampleInx = sampleInx;
			}
		}
		return (maxSampleInx, maxValue);
	}

	void Log(double invAmount)
	{
		if (_logWriter == null) return;

		const int ColRem = 0;
		const int ColInv = 1;
		const int ColProfit = 2;
		const int ColTotal = 3;

		var data = new string[2, TargetCount + 4];

		var row = data.GetRowSpan(0);
		for (var i = 0; i < TargetCount; i++) row[i] = $"g{i}";
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
		var remainder = invAmount - appliedInv;
		row = row.Slice(TargetCount);
		row[ColRem] = remainder.ToString(DoubleFormat);
		row[ColInv] = appliedInv.ToString(DoubleFormat);
		row[ColProfit] = (totalReward - appliedInv).ToString(DoubleFormat);
		row[ColTotal] = (totalReward + remainder).ToString(DoubleFormat);

		_logWriter.GetWriter().WriteTable(data, [false], []);
	}
}
