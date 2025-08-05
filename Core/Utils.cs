﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria.Localization;
using TShockAPI;
using static Terraria.Utils;
using TileCollection = ModFramework.ICollection<Terraria.ITile>;

namespace Chireiden.TShock.Omni;

public static partial class Utils
{
    internal static TileActionAttempt WithPermissionCheck(TileActionAttempt action, TSPlayer? player)
    {
        return (x, y) => (player?.HasBuildPermission(x, y) ?? false) && action(x, y);
    }

    public static bool SingleOfEither<T>(this IEnumerable<T> collection, bool skipMultiple, out T? value, params Func<T, bool>[] predicate)
    {
        foreach (var p in predicate)
        {
            var col = collection.Where(p);
            if (col.Any())
            {
                var t2 = col.Take(2);
                if (t2.Count() == 1)
                {
                    value = t2.First();
                    return true;
                }
                else if (!skipMultiple)
                {
                    break;
                }
            }
        }
        value = default;
        return false;
    }

    public static bool FirstOfEither<T>(this IEnumerable<T> collection, out T? value, params Func<T, bool>[] predicate)
    {
        foreach (var p in predicate)
        {
            var col = collection.Where(p);
            if (col.Any())
            {
                value = col.First();
                return true;
            }
        }
        value = default;
        return false;
    }

    public static bool TryParseGameCulture(string s, [NotNullWhen(returnValue: true)] out GameCulture? culture, bool nearby = true)
    {
        if (int.TryParse(s, out var number) && GameCulture._legacyCultures.TryGetValue(number, out culture))
        {
            return true;
        }

        static bool ToGameCulture(CultureInfo culture, [NotNullWhen(returnValue: true)] out GameCulture? gameCulture)
        {
            var (distance, nc) = GameCulture._legacyCultures.Values
               .Select(v => (Distance: Distance(v.CultureInfo, culture), Culture: v))
               .OrderBy(c => c.Distance)
               .First();

            if (distance != 1)
            {
                gameCulture = nc;
                return true;
            }

            gameCulture = null;
            return false;
        }

        static bool MatchCulture(IEnumerable<CultureInfo> culture, string criteria, out CultureInfo result)
        {
            return culture.SingleOfEither(skipMultiple: true, out result!,
                c => c.Name == criteria,
                c => c.TwoLetterISOLanguageName == criteria,
                c => c.DisplayName == criteria,
                c => c.NativeName == criteria,
                c => c.NativeName.Contains(criteria));
        }

        if (MatchCulture(GameCulture._legacyCultures.Values.Select(v => v.CultureInfo), s, out var targetCulture)
            && ToGameCulture(targetCulture, out culture))
        {
            return true;
        }

        if (!nearby)
        {
            culture = null;
            return false;
        }

        try
        {
            // Sometimes the CultureInfo.GetCultures(CultureTypes.AllCultures) does not contains the default culture
            // So we try to create it and check if that works
            if (ToGameCulture(CultureInfo.GetCultureInfo(s), out culture))
            {
                return true;
            }
        }
        catch
        {
        }

        if (MatchCulture(CultureInfo.GetCultures(CultureTypes.AllCultures), s, out targetCulture)
            && ToGameCulture(targetCulture, out culture))
        {
            return true;
        }

        culture = null;
        return false;
    }

    public static double Distance(CultureInfo a, CultureInfo b)
    {
        static CultureInfo[] Parents(CultureInfo i)
        {
            var result = new List<CultureInfo>();
            while (i != CultureInfo.InvariantCulture)
            {
                result.Add(i);
                i = i.Parent;
            }
            return result.Reverse<CultureInfo>().ToArray();
        }

        var ap = Parents(a);
        var bp = Parents(b);
        var weight = 1.0;
        for (var i = 0; i < Math.Min(ap.Length, bp.Length); i++)
        {
            if (!ap[i].Equals(bp[i])) // Equals, not ==
            {
                return weight;
            }
            weight /= 2;
        }
        return ap.Length == bp.Length ? 0 : weight;
    }

    public static CultureInfo CultureRedirect(CultureInfo cultureInfo)
    {
        return cultureInfo.Name == "zh-Hans" ? new CultureInfo("zh-CN") : cultureInfo;
    }

    public static Group? ParentGroup(Group? group, Func<Group, bool> predicate)
    {
        var hashset = new HashSet<string>();
        if (group == null || !predicate(group))
        {
            return null;
        }
        while (true)
        {
            if (!hashset.Add(group.Name))
            {
                return null;
            }

            var parent = group.Parent;
            if (parent == null || !predicate(parent))
            {
                return group;
            }
            group = parent;
        }
    }

    public static TileCollection CloneTileCollection(TileCollection existing, TileCollection newstorage)
    {
        for (var x = 0; x < existing.Width; x++)
        {
            for (var y = 0; y < existing.Height; y++)
            {
                newstorage[x, y] = existing[x, y];
            }
        }
        return newstorage;
    }

    internal static void TryRenameCommand(Command command, Dictionary<string, List<string>> newnames)
    {
        var method = command.CommandDelegate.Method;
        var sig = $"{method.DeclaringType?.FullName}.{method.Name}";
        if (newnames.TryGetValue(sig, out var names))
        {
            command.Names.Clear();
            command.Names.AddRange(names);
        }
    }

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved

    /// <summary>
    /// Converts a list of arguments back to a command.
    /// This is a rough inverse of <seealso cref="TShockAPI.Commands.ParseParameters(string)" />.
    /// Will not add specifier for you.
    /// </summary>
    public static string ToCommand(string command, List<string> args)
    {
        // Theoretically there should be no space to trim
        // but REST's rawcmd assume everything as command
        // and the alt syntax parse and keep the first character
        return string.Join(" ", new List<string> { $"{command}" }.Concat(args.Select(arg =>
        {
            var parg = arg.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return string.IsNullOrWhiteSpace(parg) || parg.Contains(' ') || parg.Contains('\\') ? $"\"{parg}\"" : parg;
        }))).Trim();
    }

    /// <summary>
    /// Parse a string into a list of commands.
    /// Supports <c>command [args] [&amp;&amp; command [args]] ..</c> and
    /// <c>command [args] [; command [args]] ..</c>.
    /// Similar but not fully compatible with the syntax of <seealso cref="TShockAPI.Commands.ParseParameters(string)"/>.
    /// </summary>
    public static List<Plugin.ParsedCommand> ParseCommands(string input)
    {
        var result = new List<Plugin.ParsedCommand>();
        var currentIndex = 0;
        var inQuote = false;
        var currentCommand = new Plugin.ParsedCommand();
        while (currentIndex < input.Length)
        {
            switch (input[currentIndex])
            {
                case '\"':
                    inQuote = !inQuote;
                    if (!inQuote && currentIndex + 1 < input.Length && char.IsWhiteSpace(input[currentIndex + 1]))
                    {
                        currentCommand.EndSegment(true);
                        currentIndex++;
                    }
                    break;
                case '\\':
                    if (currentIndex + 1 < input.Length)
                    {
                        currentIndex++;
                    }
                    currentCommand.AppendChar(input[currentIndex]);
                    break;
                case '&' when !inQuote && currentIndex + 1 < input.Length && input[currentIndex + 1] == '&':
                case ';' when !inQuote:
                    currentCommand.EndSegment(false);
                    currentCommand.ContinueOnError = input[currentIndex] == ';';
                    result.Add(currentCommand);
                    currentCommand = new Plugin.ParsedCommand();
                    currentIndex += currentCommand.ContinueOnError ? 0 : 1;
                    while (currentIndex + 1 < input.Length && char.IsWhiteSpace(input[currentIndex + 1]))
                    {
                        currentIndex++;
                    }
                    break;
                case char c when !inQuote && char.IsWhiteSpace(c):
                    currentCommand.EndSegment(false);
                    while (currentIndex + 1 < input.Length && char.IsWhiteSpace(input[currentIndex + 1]))
                    {
                        currentIndex++;
                    }
                    break;
                case char c:
                    currentCommand.AppendChar(c);
                    break;
            }
            currentIndex++;
        }
        currentCommand.EndSegment();
        if (!currentCommand.IsEmpty)
        {
            result.Add(currentCommand);
        }
        return result;
    }

#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved

    public static IEnumerable<TSPlayer> ActivePlayers => TShockAPI.TShock.Players.Where(p => p?.Active == true);

    public static IEnumerable<TShockAPI.DB.UserAccount> SearchUserAccounts(string? pat)
    {
        if (pat == null)
        {
            yield break;
        }
        if (pat == "*")
        {
            foreach (var acc in Utils.ActivePlayers.Select(p => p.Account).Where(a => a != null))
            {
                yield return acc;
            }
        }
        else if (pat.StartsWith("tsi:"))
        {
            if (int.TryParse(pat[4..], out var id))
            {
                var exact = TShockAPI.TShock.Players.SingleOrDefault(p => p?.Index == id)?.Account;
                if (exact != null)
                {
                    yield return exact;
                    yield break;
                }
            }
        }
        else if (pat.StartsWith("usr:"))
        {
            pat = pat[4..];
            var exact = TShockAPI.TShock.UserAccounts.GetUserAccountByName(pat);
            if (exact != null)
            {
                yield return exact;
                yield break;
            }

            foreach (var acc in TShockAPI.TShock.UserAccounts.GetUserAccounts()
                .Where(p => p?.Name.Contains(pat) == true)
                .OrderBy(p => p.Name.StartsWith(pat) ? 0 : 1))
            {
                yield return acc;
            }
        }
        else if (pat.StartsWith("usi:"))
        {
            if (int.TryParse(pat[4..], out var id))
            {
                var exact = TShockAPI.TShock.UserAccounts.GetUserAccountByID(id);
                if (exact != null)
                {
                    yield return exact;
                }
            }
        }
        else
        {
            if (pat.StartsWith("tsp:"))
            {
                pat = pat[4..];
            }
            var exact = TShockAPI.TShock.Players.SingleOrDefault(p => p?.Name == pat)?.Account;
            if (exact != null)
            {
                yield return exact;
                yield break;
            }

            foreach (var acc in TShockAPI.TShock.Players
                .Where(p => p?.Active == true && p?.Account?.Name?.Contains(pat) == true)
                .OrderBy(p => p.Account.Name.StartsWith(pat) ? 0 : 1)
                .Select(p => p.Account))
            {
                yield return acc;
            }
        }
    }

    internal static bool PrivateIPAddress(System.Net.IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            return PrivateIPAddress(address.MapToIPv4());
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // IPv6 local addr
            return address.GetAddressBytes() switch
            {
                [0xfd, _, _, _, _, _, _, _, _, _, _, _, _, _, _, _] => true,
                [0xfe, >= 0x80 and <= 0xbf, _, _, _, _, _, _, _, _, _, _, _, _, _, _] => true,
                _ => false
            };
        }

        if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            return false;
        }

        // IPv4 local addr
        return address.GetAddressBytes() switch
        {
            [10, _, _, _] => true,
            [127, _, _, _] => true,
            [192, 168, _, _] => true,
            [172, >= 16 and < 32, _, _] => true,
            [169, 254, _, _] => true,
            _ => false
        };
    }

    internal static Type TShockType(string name)
    {
        foreach (var type in typeof(TShockAPI.TShock).Module.GetTypes())
        {
            if (type.Name == name)
            {
                return type;
            }
        }
        throw new TypeLoadException(GetString($"Could not find type {name} in TShock"));
    }

    public static Terraria.Item GetInventory(this Terraria.Player p, short slot)
    {
        return slot switch
        {
            short when Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0 + 10 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0
                => p.Loadouts[2].Dye[slot - Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0],
            short when Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0
                => p.Loadouts[2].Armor[slot - Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0],
            short when Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0
                => p.Loadouts[1].Dye[slot - Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0],
            short when Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0
                => p.Loadouts[1].Armor[slot - Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0],
            short when Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0
                => p.Loadouts[0].Dye[slot - Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0],
            short when Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0
                => p.Loadouts[0].Armor[slot - Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0],
            short when Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank4_0
                => p.bank4.item[slot - Terraria.ID.PlayerItemSlotID.Bank4_0],
            short when Terraria.ID.PlayerItemSlotID.Bank4_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank3_0
                => p.bank3.item[slot - Terraria.ID.PlayerItemSlotID.Bank3_0],
            short when Terraria.ID.PlayerItemSlotID.Bank3_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.TrashItem
                => p.trashItem,
            short when Terraria.ID.PlayerItemSlotID.TrashItem > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank2_0
                => p.bank2.item[slot - Terraria.ID.PlayerItemSlotID.Bank2_0],
            short when Terraria.ID.PlayerItemSlotID.Bank2_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Bank1_0
                => p.bank.item[slot - Terraria.ID.PlayerItemSlotID.Bank1_0],
            short when Terraria.ID.PlayerItemSlotID.Bank1_0 > slot && slot >= Terraria.ID.PlayerItemSlotID.MiscDye0
                => p.miscDyes[slot - Terraria.ID.PlayerItemSlotID.MiscDye0],
            short when Terraria.ID.PlayerItemSlotID.MiscDye0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Misc0
                => p.miscEquips[slot - Terraria.ID.PlayerItemSlotID.Misc0],
            short when Terraria.ID.PlayerItemSlotID.Misc0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Dye0
                => p.dye[slot - Terraria.ID.PlayerItemSlotID.Dye0],
            short when Terraria.ID.PlayerItemSlotID.Dye0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Armor0
                => p.armor[slot - Terraria.ID.PlayerItemSlotID.Armor0],
            short when Terraria.ID.PlayerItemSlotID.Armor0 > slot && slot >= Terraria.ID.PlayerItemSlotID.Inventory0
                => p.inventory[slot - Terraria.ID.PlayerItemSlotID.Inventory0],
            _ => throw new System.Runtime.CompilerServices.SwitchExpressionException($"Unexpected slot: {slot}")
        };
    }

    public static System.Reflection.MethodInfo? Method(Expression<Action> expression)
    {
        return (expression.Body as MethodCallExpression)?.Method;
    }

    public static void CancelPacket(this OTAPI.Hooks.MessageBuffer.GetDataEventArgs args)
    {
        // FIXME: TSAPI is not respecting args.Result, so we have to craft invalid packet.
        args.Result = OTAPI.HookResult.Cancel;
        args.PacketId = byte.MaxValue;
    }

    public static bool HasPermission(this TShockAPI.TSPlayer player, List<string> p)
    {
        return p.Any(player.HasPermission);
    }

    public static void ShowInfo(string value)
    {
        if (TShockAPI.TShock.Log != null)
        {
            TShockAPI.TShock.Log.ConsoleInfo(value);
        }
        else
        {
            Console.WriteLine(value);
        }
    }

    public static void ShowError(string value)
    {
        if (TShockAPI.TShock.Log != null)
        {
            TShockAPI.TShock.Log.ConsoleError(value);
        }
        else
        {
            Console.WriteLine(value);
        }
    }

    public static void AliasPermission(string orig, params string[] equiv)
    {
        foreach (var group in TShockAPI.TShock.Groups.groups)
        {
            if (group.HasPermission(orig) && (group.Parent?.HasPermission(orig) != true))
            {
                AddPermission(group, equiv);
            }
        }
    }

    public static void AddPermission(TShockAPI.Group? group, params string[] perm)
    {
        if (group == null)
        {
            return;
        }
        TShockAPI.TShock.Groups.AddPermissions(group!.Name, [.. perm]);
    }

    public static void OnceFlag(string key, Action action)
    {
        if (!File.Exists(Path.Combine(TShockAPI.TShock.SavePath, key)))
        {
            action();
            File.WriteAllText(Path.Combine(TShockAPI.TShock.SavePath, key), string.Empty);
        }
    }

    public static void AssemblyMutex(TerrariaApi.Server.TerrariaPlugin plugin)
    {
        var asm = plugin.GetType().Assembly;
        var lds = ((List<TerrariaApi.Server.PluginContainer>) typeof(TerrariaApi.Server.ServerApi)
            .GetField("plugins", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!)!;

        var dict = ((Dictionary<string, System.Reflection.Assembly>) typeof(TerrariaApi.Server.ServerApi)
            .GetField("loadedAssemblies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!)
            .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)!;

        foreach (var other in lds)
        {
            var ld = other.Plugin.GetType().Assembly;
            if (ld.GetName().Name == asm.GetName().Name && other.Plugin != plugin)
            {
                var em = GetString($"Duplicate {plugin.Name} loaded:");
                if (dict.TryGetValue(ld, out var fileNameWithoutExtension))
                {
                    em += Environment.NewLine + GetString($"  --> Loaded:  {fileNameWithoutExtension} (v{ld.GetName().Version})");
                }
                if (dict.TryGetValue(asm, out fileNameWithoutExtension))
                {
                    em += Environment.NewLine + GetString($"  --> Current: {fileNameWithoutExtension} (v{asm.GetName().Version})");
                }
                TerrariaApi.Server.ServerApi.LogWriter.PluginWriteLine(plugin, em, System.Diagnostics.TraceLevel.Error);
                throw new OperationCanceledException(GetString($"{asm.GetName().Name} already loaded."));
            }
        }
    }

    public static void RegisterFirst<T>(Type def, string name, object? instnace, params T?[] eventHandlers) where T : Delegate
    {
        var t = def.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance);
        t?.SetValue(instnace, Delegate.Combine([.. eventHandlers, (T?) t.GetValue(instnace)]));
    }

    public static T Read<T>(this Terraria.MessageBuffer buffer, int offset) where T : struct
    {
        return MemoryMarshal.Read<T>(buffer.readBuffer.AsSpan(offset));
    }

    public static T Read<T>(this OTAPI.Hooks.MessageBuffer.GetDataEventArgs args, int offset) where T : struct
    {
        return args.Instance.Read<T>(args.ReadOffset + offset);
    }

    public static List<Terraria.ITile> CheckInvalidTiles(TileCollection tiles, int x, int y, int r)
    {
        var result = new List<Terraria.ITile>();
        for (var i = x - r; i <= x + r; i++)
        {
            if (i < 0 || i >= Terraria.Main.maxTilesX)
            {
                continue;
            }
            for (var j = y - r; j <= y + r; j++)
            {
                if (j < 0 || j >= Terraria.Main.maxTilesY)
                {
                    continue;
                }
                if (!TileValid(tiles[i, j]))
                {
                    result.Add(tiles[i, j]);
                }
            }
        }
        return result;
    }

    public static bool TileValid(Terraria.ITile tile)
    {
        return tile.type switch
        {
            Terraria.ID.TileID.OpenDoor => TShockAPI.GetDataHandlers.MaxPlaceStyles[tile.type] < (tile.frameY / 54) + (tile.frameX / 72 * 36),
            Terraria.ID.TileID.ClosedDoor => TShockAPI.GetDataHandlers.MaxPlaceStyles[tile.type] < (tile.frameY / 54) + (tile.frameX / 54 * 36),
            _ => true
        };
    }

    public class ConsolePlayer(string name) : TSPlayer(name)
    {
        public static readonly ConsolePlayer Instance = new ConsolePlayer("Console");
        private static readonly Dictionary<ConsoleColor, int> _consoleColorMap = new Dictionary<ConsoleColor, int>
        {
            [ConsoleColor.Red] = 0xFF0000,
            [ConsoleColor.Green] = 0x00FF00,
            [ConsoleColor.Blue] = 0x0000FF,
            [ConsoleColor.Yellow] = 0xFFFF00,
            [ConsoleColor.Cyan] = 0x00FFFF,
            [ConsoleColor.Magenta] = 0xFF00FF,
            [ConsoleColor.White] = 0xFFFFFF,
            [ConsoleColor.Gray] = 0x808080,
            [ConsoleColor.DarkRed] = 0x800000,
            [ConsoleColor.DarkGreen] = 0x008000,
            [ConsoleColor.DarkBlue] = 0x000080,
            [ConsoleColor.DarkYellow] = 0x808000,
            [ConsoleColor.DarkCyan] = 0x008080,
            [ConsoleColor.DarkMagenta] = 0x800080,
            [ConsoleColor.DarkGray] = 0x808080,
            [ConsoleColor.Black] = 0x000000,
        };

        public override void SendMessage(string msg, byte red, byte green, byte blue)
        {
            Console.ForegroundColor = _consoleColorMap
                .MinBy(kvp =>
                    Math.Pow((kvp.Value >> 16) - red, 2)
                    + Math.Pow(((kvp.Value >> 8) & 0xFF) - green, 2)
                    + Math.Pow((kvp.Value & 0xFF) - blue, 2))
                .Key;
            Console.Write(msg);
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}

public class Ring<T>(int length) : IEnumerable<T>
{
    internal T[] _data = new T[length];
    internal int _start = 0;
    internal int _length = 0;

    public T this[int index] => this._data[(index + this._start) % this._data.Length];

    public void Add(T item)
    {
        if (this._length < this._data.Length)
        {
            this._data[this._length++] = item;
        }
        else
        {
            this._data[this._start] = item;
            this._start = (this._start + 1) % this._data.Length;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new RingEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return new RingEnumerator(this);
    }

    private class RingEnumerator(Ring<T> ring) : IEnumerator<T>
    {
        private readonly Ring<T> _ring = ring;
        private int _index = -1;

        public T Current => this._ring[this._index];

        object? System.Collections.IEnumerator.Current => this._ring[this._index];

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return ++this._index < this._ring._length;
        }

        public void Reset()
        {
            this._index = -1;
        }
    }
}