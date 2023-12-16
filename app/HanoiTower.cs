using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathMethods;

public class HanoiTower
{
	public static void Solve(int ringCount)
	{
		if (ringCount == 0) return;
		Move(ringCount, "A", "B", "C");
	}

	static void Move(int ringCount, string source, string intermidiate, string destination)
	{
        if (ringCount == 1)
        {
			Console.WriteLine($"Move from {source} to {destination}");
			return;
		}
        ringCount--;
		Move(ringCount, source, destination, intermidiate);
		Console.WriteLine($"Move from {source} to {destination}");
		Move(ringCount, intermidiate, source, destination);
	}
}
