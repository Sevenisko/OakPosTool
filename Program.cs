using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using Sevenisko.SharpWood;
using Console = OakPosTool.ColoredConsole;
using SysCon = System.Console;

namespace OakPosTool
{
    public class OakPlayerData
    {
        public bool canUseKeys = false;
    }

    class Program
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        static OakVec3 spawnPosition;

        static void Main(string[] args)
        {
            try
            {
                File.OpenText(Environment.CurrentDirectory + "\\spawn.txt");
            }
            catch
            {
                Console.WriteLine("File doesn't exist");
                Console.WriteLine("^F[^1ERROR^F]: ^CSpawn position is not set!^R");
                return;
            }

            try
            {
                string[] buf = File.ReadAllText(Environment.CurrentDirectory + "\\spawn.txt").Split(';');
                spawnPosition = new OakVec3(float.Parse(buf[0]), float.Parse(buf[1]), float.Parse(buf[2]));
            }
            catch
            {
                Console.WriteLine("Something fucked up");
                Console.WriteLine("^F[^1ERROR^F]: ^CSpawn position is not set!^R");
                return;
            }

            var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                Console.WriteLine("Failed to get output console mode");
                SysCon.ReadKey();
                return;
            }

            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            if (!SetConsoleMode(iStdOut, outConsoleMode))
            {
                Console.WriteLine($"Failed to set output console mode, error code: {GetLastError()}");
                SysCon.ReadKey();
                return;
            }

            OakwoodEvents.OnLog += OnLog;
            OakwoodEvents.OnPlayerConnect += OnPlayerConnect;
            OakwoodEvents.OnPlayerDisconnect += OnPlayerDisconnect;
            OakwoodEvents.OnPlayerDeath += OnPlayerDeath;
            OakwoodEvents.OnPlayerChat += OnPlayerChat;
            OakwoodEvents.OnPlayerKeyDown += OnPlayerKeyDown;

            Console.WriteLine($"^9Oakwood ^Fposition tool^R");
            Console.WriteLine($"^FMade by ^ESevenisko^R");
            SysCon.WriteLine();

            Thread clThread = new Thread(() => Oakwood.CreateClient("ipc://oakwood-inbound", "ipc://oakwood-outbound"));
            clThread.Start();
        }

        private static void OnPlayerChat(OakwoodPlayer player, string text)
        {
            Oakwood.SendChatMessage($"[CHAT] {player.Name}: {text}");
        }

        private static void OnPlayerDeath(OakwoodPlayer player, OakwoodPlayer killer)
        {
            foreach(var p in Oakwood.Players)
            {
                p.HUD.Message($"{player.Name} died.", OakColor.White);
            }

            player.Spawn(spawnPosition, 0);
        }

        private static void OnPlayerDisconnect(OakwoodPlayer player)
        {
            foreach (var p in Oakwood.Players)
            {
                p.HUD.Message($"{player.Name} left the session.", OakColor.White);
            }
            player.PlayerData = null;
        }

        private static void OnPlayerConnect(OakwoodPlayer player)
        {
            foreach (var p in Oakwood.Players)
            {
                p.HUD.Message($"{player.Name} joined the session.", OakColor.White);
            }

            player.PlayerData = new OakPlayerData();
            player.Spawn(spawnPosition, 0);
        }

        private static void OnPlayerKeyDown(OakwoodPlayer player, VirtualKey key)
        {
            OakVec3 pos = player.Position;

            switch (key)
            {
                case VirtualKey.Numpad8: // X+
                    {
                        if (!(player.PlayerData as OakPlayerData).canUseKeys) break;
                        OakVec3 newPos = new OakVec3(pos.x + 1.5f, pos.y, pos.z);
                        player.Position = newPos;
                    }
                    break;
                case VirtualKey.Numpad2: // X-
                    {
                        if (!(player.PlayerData as OakPlayerData).canUseKeys) break;
                        OakVec3 newPos = new OakVec3(pos.x - 1.5f, pos.y, pos.z);
                        player.Position = newPos;
                    }
                    break;
                case VirtualKey.Numpad4: // Z-
                    {
                        if (!(player.PlayerData as OakPlayerData).canUseKeys) break;
                        OakVec3 newPos = new OakVec3(pos.x, pos.y, pos.z - 1.5f);
                        player.Position = newPos;
                    }
                    break;
                case VirtualKey.Numpad6: // Z+
                    {
                        if (!(player.PlayerData as OakPlayerData).canUseKeys) break;
                        OakVec3 newPos = new OakVec3(pos.x, pos.y, pos.z + 1.5f);
                        player.Position = newPos;
                    }
                    break;
                case VirtualKey.Numpad3: // Y-
                    {
                        if (!(player.PlayerData as OakPlayerData).canUseKeys) break;
                        OakVec3 newPos = new OakVec3(pos.x, pos.y - 1.5f, pos.z);
                        player.Position = newPos;
                    }
                    break;
                case VirtualKey.Numpad9: // Y+
                    {
                        if (!(player.PlayerData as OakPlayerData).canUseKeys) break;
                        OakVec3 newPos = new OakVec3(pos.x, pos.y + 1.5f, pos.z);
                        player.Position = newPos;
                    }
                    break;
                case VirtualKey.F5:
                    {
                        (player.PlayerData as OakPlayerData).canUseKeys = !(player.PlayerData as OakPlayerData).canUseKeys;
                        string msg = (player.PlayerData as OakPlayerData).canUseKeys ? "enabled" : "disabled";
                        player.HUD.Message($"Key movement {msg}!", OakColor.White);
                    }
                    break;
            }
        }

        private static void OnLog(DateTime time, string source, string message)
        {
            Console.WriteLine($"^F[^9{time.ToString("HH:mm:ss")} ^F- ^A{source}^F]: ^5{message}^R");
        }

        [Command("getpos", "Prints your current position")]
        static void GetPos(OakwoodPlayer player)
        {
            OakVec3 pos = player.Position, dir = player.Direction;
            player.HUD.Message($"Position: ({pos.x}, {pos.y}, {pos.z})", OakColor.White);
            player.HUD.Message($"Direction: ({dir.x}, {dir.y}, {dir.z})", OakColor.White);
        }

        [Command("distance", "Gets distance between two positions")]
        static void Distance(OakwoodPlayer player, string pos1Name, string pos2Name)
        {
            string file1Name = Environment.CurrentDirectory + $"\\SavedPos\\{pos1Name}.txt";
            string file2Name = Environment.CurrentDirectory + $"\\SavedPos\\{pos2Name}.txt";

            if (!File.Exists(file1Name))
            {
                player.HUD.Message($"Position '{pos1Name}' doesn't exist!", OakColor.Red);
                return;
            }
            
            if (!File.Exists(file2Name))
            {
                player.HUD.Message($"Position '{pos2Name}' doesn't exist!", OakColor.Red);
                return;
            }

            OakVec3 p1 = new OakVec3(), p2 = new OakVec3();

            string[] lines = File.ReadAllLines(file1Name);

            foreach (string line in lines)
            {
                if (!line.StartsWith("#"))
                {
                    string[] buf = line.Split(';');
                    p1 = new OakVec3(float.Parse(buf[0]), float.Parse(buf[1]), float.Parse(buf[2]));

                    break;
                }
            }

            lines = File.ReadAllLines(file2Name);

            foreach (string line in lines)
            {
                if (!line.StartsWith("#"))
                {
                    string[] buf = line.Split(';');
                    p2 = new OakVec3(float.Parse(buf[0]), float.Parse(buf[1]), float.Parse(buf[2]));

                    break;
                }
            }

            float dist = OakVec3.Distance(p1, p2);

            player.HUD.Message($"Distance: {dist.ToString("0.00")}", OakColor.White);
        }

        [Command("writepos", "Writes a position into file")]
        static void WritePos(OakwoodPlayer player, string posName)
        {
            string fileName = Environment.CurrentDirectory + $"\\SavedPos\\{posName}.txt";
            OakVec3 pos = player.Position, dir = player.Direction;

            if (File.Exists(fileName))
            {
                player.HUD.Message($"Position '{posName}' updated!", OakColor.White);   
            }
            else
            {
                player.HUD.Message($"Position '{posName}' saved!", OakColor.White);
            }

            File.WriteAllText(fileName, $"# The format is following: <posX>;<posY>;<posZ>;<dirX>;<dirY>;<dirZ>\n{pos.x};{pos.y};{pos.z};{dir.x};{dir.y};{dir.z}");
        }

        [Command("readpos", "Reads a position from file")]
        static void ReadPos(OakwoodPlayer player, string posName)
        {
            string fileName = Environment.CurrentDirectory + $"\\SavedPos\\{posName}.txt";

            if (!File.Exists(fileName))
            {
                player.HUD.Message($"Position '{posName}' doesn't exist!", OakColor.Red);
                return;
            }

            string[] lines = File.ReadAllLines(fileName);

            foreach(string line in lines)
            {
                if(!line.StartsWith("#"))
                {
                    string[] buf = line.Split(';');
                    player.Position = new OakVec3(float.Parse(buf[0]), float.Parse(buf[1]), float.Parse(buf[2]));
                    player.Direction = new OakVec3(float.Parse(buf[3]), float.Parse(buf[4]), float.Parse(buf[5]));

                    player.HUD.Message($"Position '{posName}' loaded!", OakColor.White);

                    break;
                }
            }
        }
    }
}
