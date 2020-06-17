using System;
using System.Collections.Generic;
using System.Text;

namespace OakPosTool
{
    class ColoredConsole
    {
		public static void WriteLine(string str)
		{
			Write(str + Environment.NewLine);
		}

		public static void Write(string str)
		{
			string s = str.
			Replace("^0", "[30m"). // Black
			Replace("^1", "[31m"). // Dark Red
			Replace("^2", "[32m"). // Dark Green
			Replace("^3", "[33m"). // Dark Yellow (Gold)
			Replace("^4", "[34m"). // Dark Blue
			Replace("^5", "[35m"). // Magenta
			Replace("^6", "[36m"). // Cyan
			Replace("^7", "[37m"). // Gray
			Replace("^8", "[90m"). // Light Gray
			Replace("^9", "[91m"). // Red
			Replace("^A", "[92m"). // Green
			Replace("^B", "[93m"). // Yellow
			Replace("^C", "[94m"). // Blue
			Replace("^D", "[95m"). // Pink
			Replace("^E", "[96m"). // Aqua
			Replace("^F", "[97m"). // White
			Replace("^R", "[0m"). // Reset
			Replace("^L", "[1m"). // Bold
			Replace("^I", "[3m"). // Italic
			Replace("^U", "[4m"). // Underline

			Replace("^a", "[92m").
			Replace("^b", "[93m").
			Replace("^c", "[94m").
			Replace("^d", "[95m").
			Replace("^e", "[96m").
			Replace("^f", "[97m").
			Replace("^r", "[0m").
			Replace("^l", "[1m").
			Replace("^i", "[3m").
			Replace("^u", "[4m");

			Console.Write(s);
		}
	}
}
