using System.Net.NetworkInformation;
using CommunityToolkit.HighPerformance;

namespace MathMethods;

public struct BellmanEq2
{
	const string DoubleFormat = "0.##############";

	readonly double[] _invOptions;
	readonly double[,] _samplesMx;
	readonly int[] _invPlan;
	readonly int[] _currInvPlan;
	double _maxValue;
	readonly ILogWriter? _logWriter;

	readonly int TargetCount => _invPlan.Length;
	readonly int SampleCount => _invOptions.Length;

	readonly (double Inv, double Reward) GetTargetInv(int targetInx)
	{
		var sampleInx = _invPlan[targetInx];
		return sampleInx < 0
			? (double.NaN, double.NaN)
			: (_invOptions[sampleInx], _samplesMx[sampleInx, targetInx]);
	}

	private BellmanEq2(double[] invOptions, double[,] samplesMx, int[] invPlan, ILogWriter? logWriter)
	{
		_invOptions = invOptions;
		_samplesMx = samplesMx;
		_invPlan = invPlan;
		_currInvPlan = new int[TargetCount];
		for (var i = 0; i < TargetCount; i++)
		{
			_invPlan[i] = -1;
			_currInvPlan[i] = -1;
		}
		_logWriter = logWriter;
	}

	public static void Solve(double[] invOptions, double[,] samplesMx, int[] invPlan, double invAmount, ILogWriter? logWriter = null)
	{
		if (invOptions.Length != samplesMx.GetLength(0)) throw new ArgumentException("The length of the investment options vector does not match samples matrix row count.");
		if (invPlan.Length != samplesMx.GetLength(1)) throw new ArgumentException("The length of the investment plan does not match samples matrix column count");

		var bellman = new BellmanEq2(invOptions, samplesMx, invPlan, logWriter);
		var availableInv = invAmount;
		bellman.ValueFn(0, availableInv, 0);
		bellman.Log(invAmount);
	}

	void ValueFn(int targetInx, double invAmount, double revenue)
	{
		if (targetInx == TargetCount - 1)
		{
			ValueFn1(targetInx, invAmount, revenue);
			return;
		}

		for (int sampleInx = 0; sampleInx < SampleCount; sampleInx++)
		{
			var remainder = invAmount - _invOptions[sampleInx];
			if (remainder < 0) break;
			_currInvPlan[targetInx] = sampleInx;
			ValueFn(targetInx + 1, remainder, revenue + _samplesMx[sampleInx, targetInx]);
		}
		_currInvPlan[targetInx] = -1;
	}

	void ValueFn1(int targetInx, double invAmount, double revenue)
	{
		for (int sampleInx = 0; sampleInx < _invOptions.Length; sampleInx++)
		{
			var remainder = invAmount - _invOptions[sampleInx];
			if (remainder < 0) break;
			_currInvPlan[targetInx] = sampleInx;
			var value = revenue + _samplesMx[sampleInx, targetInx] + remainder;
			if (value > _maxValue)
			{
				_maxValue = value;
				_currInvPlan.CopyTo(_invPlan, 0);
			}
		}
		_currInvPlan[targetInx] = -1;
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
