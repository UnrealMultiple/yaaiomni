﻿using System.Reflection;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace Chireiden.TShock.Omni.Misc;

public partial class Plugin
{
    [Command("Admin.GarbageCollect", "_gc", Permission = "chireiden.omni.admin.gc")]
    private void Command_GC(CommandArgs args)
    {
        if (args.Parameters.Contains("-f"))
        {
            System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
        }
        else
        {
            GC.Collect(3, GCCollectionMode.Optimized, false);
        }
        args.Player.SendSuccessMessage(GetString("GC Triggered."));
    }

    [Command("Admin.SqliteVacuum", "_sv", Permission = "chireiden.omni.admin.sv")]
    private void Command_SqliteVacuum(CommandArgs args)
    {
        var db = TShockAPI.TShock.DB;
        if (TShockAPI.DB.DbExt.GetSqlType(db) == TShockAPI.DB.SqlType.Sqlite)
        {
            TShockAPI.DB.DbExt.Query(db, "VACUUM");
            args.Player.SendSuccessMessage(GetString("SQLite Vacuum on TShock.DB triggered."));
        }
        else
        {
            args.Player.SendErrorMessage(GetString("TShock.DB is not SQLite."));
        }
    }

    [Command("Admin.RawBroadcast", "rbc", "rawbroadcast", Permission = "chireiden.omni.admin.rawbroadcast")]
    private void Command_RawBroadcast(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendErrorMessage(GetString("Invalid broadcast message."));
            return;
        }

        // No need to log because TShock log every command already.
        TSPlayer.All.SendMessage(args.Parameters[0], 0, 0, 0);
    }

    [Command("Admin.ListClients", "listclients", Permission = "chireiden.omni.admin.listclients")]
    private void Command_ListConnected(CommandArgs args)
    {
        foreach (var client in Terraria.Netplay.Clients)
        {
            if (client.IsConnected())
            {
                args.Player.SendInfoMessage(GetString($"Index: {client.Id} {client.Socket.GetRemoteAddress()} {client.Name} State: {client.State} Bytes: {Terraria.NetMessage.buffer[client.Id].totalData}"));
                args.Player.SendInfoMessage(GetString($"Status: {client.StatusText}"));
                args.Player.SendInfoMessage(GetString($"Status: {client.StatusText2}"));
            }
        }
    }

    [Command("Admin.DumpBuffer", "dumpbuffer", Permission = "chireiden.omni.admin.dumpbuffer")]
    private void Command_DumpBuffer(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendErrorMessage(GetString("Invalid player."));
            return;
        }

        if (!byte.TryParse(args.Parameters[0], out var index))
        {
            args.Player.SendErrorMessage(GetString("Invalid player."));
            return;
        }

        var path = args.Parameters.Count > 1 ? string.Join("_", args.Parameters[1].Split(Path.GetInvalidFileNameChars())) : "dump.bin";
        path = Path.Combine(TShockAPI.TShock.SavePath, path);

        File.WriteAllBytes(path, Terraria.NetMessage.buffer[index].readBuffer[..Terraria.NetMessage.buffer[index].totalData]);
    }

    [Command("Admin.FindCommand", "whereis", Permission = "chireiden.omni.admin.whereis")]
    private void Command_WhereIs(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendErrorMessage(GetString("Invalid command."));
            return;
        }

        var c = TShockAPI.Commands.ChatCommands.Where(command => command.HasAlias(args.Parameters[0])).ToList();

        args.Player.SendInfoMessage(GetString($"ChatCommands Found: {c.Count}"));

        var dict = ((Dictionary<string, Assembly>?) typeof(ServerApi).GetField("loadedAssemblies", _bfany)?.GetValue(null))?
            .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        foreach (var command in c)
        {
            if (command.Names.Count == 1)
            {
                args.Player.SendSuccessMessage($"{TShockAPI.Commands.Specifier}{command.Name} :");
            }
            else
            {
                var aliases = string.Join(", ", command.Names.Skip(1).Select(x => TShockAPI.Commands.Specifier + x));
                args.Player.SendSuccessMessage($"{TShockAPI.Commands.Specifier}{command.Name} ({aliases}) :");
            }
            var method = command.CommandDelegate.Method;
            var sig = $"{method.DeclaringType?.FullName}.{method.Name}";
            args.Player.SendInfoMessage(GetString($"    Signature: {sig}"));
            var asm = method.DeclaringType?.Assembly;
            if (asm is null)
            {
                args.Player.SendInfoMessage(GetString($"    No Assembly found"));
                continue;
            }
            if (!string.IsNullOrWhiteSpace(asm.Location))
            {
                args.Player.SendInfoMessage(GetString($"    Location: ({asm.Location})"));
            }
            if (dict?.TryGetValue(asm, out var fileNameWithoutExtension) == true)
            {
                args.Player.SendInfoMessage(GetString($"    File: {fileNameWithoutExtension}"));
            }
            var plugins = ServerApi.Plugins.Where(p => p.Plugin.GetType().Assembly == asm).ToList();
            if (plugins.Count == 0)
            {
                args.Player.SendInfoMessage(GetString($"    No Plugin found"));
                continue;
            }
            foreach (var plugin in plugins)
            {
                var p = plugin.Plugin;
                args.Player.SendInfoMessage(GetString($"    Plugin: {p.Name} v{p.Version} by {p.Author}"));
            }
        }
    }

    [Command("Admin.TerminateSocket", "kc", Permission = "chireiden.omni.admin.terminatesocket")]
    private void Command_TerminateSocket(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendErrorMessage(GetString("Invalid player."));
            return;
        }

        if (!byte.TryParse(args.Parameters[0], out var index))
        {
            args.Player.SendErrorMessage(GetString("Invalid player."));
            return;
        }

        Terraria.Netplay.Clients[index]?.Socket?.Close();
    }

    private (int Tick, DateTime Time) _tickCheck = (-1, DateTime.MinValue);
    [Command("Admin.UpsCheck", "_ups", Permission = "chireiden.omni.admin.upscheck")]
    private void Command_TicksPerSec(CommandArgs args)
    {
        var core = ServerApi.Plugins.Get<Omni.Plugin>();
        if (core is null)
        {
            args.Player.SendErrorMessage(GetString("Core Omni is null while tracking ticks per second."));
            return;
        }
        if (args.Parameters.Count > 0 && args.Parameters[0] == "bench")
        {
            var t = DateTime.Now;
            var c = 0;
            while (c < 1000000 && (DateTime.Now - t).TotalSeconds < 15)
            {
                var x = Random.Shared.Next(10, Terraria.Main.maxTilesX - 10);
                var y = Random.Shared.Next((int) Terraria.Main.worldSurface - 1, Terraria.Main.maxTilesY - 20);
                Terraria.WorldGen.growGrassUnderground = true;
                Terraria.WorldGen.UpdateWorld_UndergroundTile(x, y, false, 3);
                Terraria.WorldGen.UpdateWorld_OvergroundTile(x, y, false, 3);
                Terraria.WorldGen.growGrassUnderground = false;
                c += 1;
            }
            var dt = DateTime.Now - t;
            args.Player.SendInfoMessage(GetString($"UPS Bench: {c} in {dt} ({c / dt.TotalSeconds:F2} per second)"));
        }
        else if (this._tickCheck.Tick == -1)
        {
            this._tickCheck = (core.UpdateCounter, DateTime.Now);
            args.Player.SendInfoMessage(GetString("Started tracking ticks per second."));
        }
        else
        {
            var (Tick, Time) = this._tickCheck;
            this._tickCheck = (-1, DateTime.MinValue);
            var diff = core.UpdateCounter - Tick;
            var time = DateTime.Now - Time;
            args.Player.SendInfoMessage(GetString($"{diff} ticks / {time} seconds: {diff / time.TotalSeconds:F2}"));
        }
    }

    private void Detour_UpdateConnectedClients(On.Terraria.Netplay.orig_UpdateConnectedClients orig)
    {
        orig();
        if (!Terraria.Netplay.HasClients)
        {
            if (this._tickCheck.Tick != -1)
            {
                var (Tick, Time) = this._tickCheck;
                this._tickCheck = (-1, DateTime.MinValue);
                var diff = ServerApi.Plugins.Get<Omni.Plugin>()!.UpdateCounter - Tick;
                var time = DateTime.Now - Time;
                TShockAPI.TShock.Log.ConsoleInfo(
                    GetString($"[Omni] {diff} ticks in {time.TotalSeconds:F2} seconds ({diff / time.TotalSeconds:F2} tps)"));
            }
        }
    }

    [Command("PvPStatus", "_pvp", Permission = "chireiden.omni.setpvp")]
    [RelatedPermission("Admin.PvPStatus", "chireiden.omni.admin.setpvp")]
    private void Command_PvP(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendInfoMessage(GetString($"Your PvP status: {args.Player.TPlayer.hostile}"));
            return;
        }

        if (args.Parameters.Count > 1)
        {
            if (!args.Player.HasPermission(DefinedConsts.PermissionsList.Admin.PvPStatus))
            {
                args.Player.SendErrorMessage(GetString("You don't have permission to set other players' PvP status."));
                return;
            }

            var player = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (player.Count == 0)
            {
                args.Player.SendErrorMessage(GetString("Invalid player."));
                return;
            }
            else if (player.Count > 1)
            {
                args.Player.SendMultipleMatchError(player.Select(p => p.Name));
                return;
            }

            if (!bool.TryParse(args.Parameters[1], out var pvp))
            {
                args.Player.SendErrorMessage(GetString("Invalid PvP status."));
                return;
            }

            player[0].TPlayer.hostile = pvp;
            Terraria.NetMessage.TrySendData((int) PacketTypes.TogglePvp, -1, -1, NetworkText.Empty, player[0].Index);
        }
        else
        {
            if (!bool.TryParse(args.Parameters[0], out var pvp))
            {
                args.Player.SendErrorMessage(GetString("Invalid PvP status."));
                return;
            }

            args.Player.TPlayer.hostile = pvp;
            Terraria.NetMessage.TrySendData((int) PacketTypes.TogglePvp, -1, -1, NetworkText.Empty, args.Player.Index);
        }
    }

    [Command("TeamStatus", "_team", Permission = "chireiden.omni.setteam")]
    [RelatedPermission("Admin.TeamStatus", "chireiden.omni.admin.setteam")]
    private void Command_Team(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            var team = args.Player.TPlayer.team;
            if (team > Terraria.Main.teamColor.Length)
            {
                args.Player.SendErrorMessage(GetString("Invalid team."));
                return;
            }

            var color = Terraria.Main.teamColor[team];
            var cc = $"{color.R:X2}{color.G:X2}{color.B:X2}";
            args.Player.SendInfoMessage(GetString($"Your team: {team.Color(cc)}"));
            return;
        }

        if (args.Parameters.Count > 1)
        {
            if (!args.Player.HasPermission(DefinedConsts.PermissionsList.Admin.TeamStatus))
            {
                args.Player.SendErrorMessage(GetString("You don't have permission to set other players' team."));
                return;
            }

            var player = TSPlayer.FindByNameOrID(args.Parameters[0]);
            if (player.Count == 0)
            {
                args.Player.SendErrorMessage(GetString("Invalid player."));
                return;
            }
            else if (player.Count > 1)
            {
                args.Player.SendMultipleMatchError(player.Select(p => p.Name));
                return;
            }

            if (!byte.TryParse(args.Parameters[1], out var team))
            {
                args.Player.SendErrorMessage(GetString("Invalid team."));
                return;
            }

            player[0].TPlayer.team = team;
            Terraria.NetMessage.TrySendData((int) PacketTypes.PlayerTeam, -1, -1, NetworkText.Empty, player[0].Index);
        }
        else
        {
            if (!byte.TryParse(args.Parameters[0], out var team))
            {
                args.Player.SendErrorMessage(GetString("Invalid team."));
                return;
            }

            args.Player.TPlayer.team = team;
            Terraria.NetMessage.TrySendData((int) PacketTypes.PlayerTeam, -1, -1, NetworkText.Empty, args.Player.Index);
        }
    }

    [Command("Chat", "_chat", Permission = "chireiden.omni.chat")]
    private void Command_Chat(CommandArgs args)
    {
        var index = args.Player.Index;
        var scea = new ServerChatEventArgs();
        var command = Terraria.Chat.ChatCommandId.FromType<Terraria.Chat.Commands.SayChatCommand>();
        typeof(ServerChatEventArgs).GetProperty(nameof(ServerChatEventArgs.Buffer))!.SetValue(scea, Terraria.NetMessage.buffer[index]);
        typeof(ServerChatEventArgs).GetProperty(nameof(ServerChatEventArgs.Who))!.SetValue(scea, index);
        typeof(ServerChatEventArgs).GetProperty(nameof(ServerChatEventArgs.Text))!.SetValue(scea, string.Join(" ", args.Parameters));
        typeof(ServerChatEventArgs).GetProperty(nameof(ServerChatEventArgs.CommandId))!.SetValue(scea, command);
        TerrariaApi.Server.ServerApi.Hooks.ServerChat.Invoke(scea);
    }

    [Command("Admin.Caller", "_csf", Permission = "chireiden.omni.admin.callstackframe")]
    private void Command_Caller(CommandArgs args)
    {
        var st = new System.Diagnostics.StackTrace();
        args.Player.SendInfoMessage(GetString("Stack Trace:"));
        foreach (var frame in st.GetFrames())
        {
            args.Player.SendInfoMessage(frame.ToString());
        }
    }

    [Command("Admin.GenerateFullConfig", "genconfig", Permission = "chireiden.omni.admin.genconfig")]
    private void Command_GenerateFullConfig(CommandArgs args)
    {
        try
        {
            File.WriteAllText(this.ConfigPath, Json.JsonUtils.SerializeConfig(this.config, false));
        }
        catch (Exception ex)
        {
            args.Player.SendErrorMessage(GetString($"Failed to save config: {ex.Message}"));
        }
    }
}