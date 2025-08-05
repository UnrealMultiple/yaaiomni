﻿using Chireiden.TShock.Omni.DefinedConsts;

namespace Chireiden.TShock.Omni;

public partial class Plugin
{
    private void VanillaSetup()
    {
        var vanillaMode = this.config.Mode.Value.Vanilla.Value;
        if (!vanillaMode.Enabled)
        {
            return;
        }

        if (!TShockAPI.TShock.Groups.GroupExists(Misc.VanillaGroup))
        {
            TShockAPI.TShock.Groups.AddGroup(Misc.VanillaGroup, null, "", "255,255,255");
        }

        var addperm = TShockAPI.TShock.Groups.AddPermissions(Misc.VanillaGroup, vanillaMode.Permissions);
        if (vanillaMode.AllowJourneyPowers)
        {
            addperm += TShockAPI.TShock.Groups.AddPermissions(Misc.VanillaGroup, new List<string> { "tshock.journey.*" });
        }
        if (vanillaMode.IgnoreAntiCheat)
        {
            addperm += TShockAPI.TShock.Groups.AddPermissions(Misc.VanillaGroup, new List<string> { "tshock.ignore.*", "!tshock.ignore.ssc" });
        }
        if (addperm.Length == 0)
        {
            TShockAPI.TSPlayer.Server.SendInfoMessage(GetString($"Failed to add permissions to group {Misc.VanillaGroup}."));
        }

        var vg = TShockAPI.TShock.Groups.GetGroupByName(Misc.VanillaGroup);
        TShockAPI.TShock.Groups.UpdateGroup(Misc.VanillaGroup, null, vg.Permissions, vg.ChatColor, vg.Suffix, vg.Prefix);
        var group = TShockAPI.TShock.Groups.GetGroupByName(TShockAPI.TShock.Config.Settings.DefaultRegistrationGroupName);
        group = Utils.ParentGroup(group, _ => true);
        if (group == null)
        {
            TShockAPI.TSPlayer.Server.SendErrorMessage(GetString($"Failed to find group {TShockAPI.TShock.Config.Settings.DefaultRegistrationGroupName}."));
            return;
        }
        if (group.Name != Misc.VanillaGroup)
        {
            try
            {
                TShockAPI.TShock.Groups.UpdateGroup(group.Name, Misc.VanillaGroup, group.Permissions, group.ChatColor, group.Suffix, group.Prefix);
            }
            catch (Exception e)
            {
                TShockAPI.TSPlayer.Server.SendInfoMessage(GetString($"Failed to set parent group to {Misc.VanillaGroup}: {e}"));
            }
        }
    }

    private void DefaultPermissionSetup()
    {
        var preset = this.config.Permission.Value.Preset.Value;
        var vanillaMode = this.config.Mode.Value.Vanilla.Value.Enabled;
        if (!preset.Enabled && !vanillaMode && !preset.AlwaysApply)
        {
            return;
        }

        if (preset.AlwaysApply)
        {
            this.PermissionSetup();
        }
        else
        {
            Utils.OnceFlag(Misc.PresetLock, this.PermissionSetup);
        }
    }

    public event Action<Plugin>? OnPermissionSetup;
    private void PermissionSetup()
    {
        OnPermissionSetup?.Invoke(this);

        var preset = this.config.Permission.Value.Preset.Value;
        var guest = TShockAPI.TShock.Groups.GetGroupByName(TShockAPI.TShock.Config.Settings.DefaultGuestGroupName);
        Utils.AddPermission(guest,
            Permission.Ping,
            Permission.Echo);

        if (preset.DebugForAdminOnly)
        {
            Utils.AliasPermission(TShockAPI.Permissions.kick, Permission.Whynot);
        }
        else
        {
            Utils.AddPermission(guest, Permission.Whynot);
        }

        Utils.AliasPermission(TShockAPI.Permissions.kick,
            Permission.Admin.Ghost,
            Permission.Admin.ManageLanguage,
            Permission.Admin.DebugStat,
            Permission.SetTimeout,
            Permission.SetInterval,
            Permission.ClearInterval,
            Permission.ShowTimeout,
            Permission.ResetCharacter,
            Permission.Admin.RunBackground);

        Utils.AliasPermission(TShockAPI.Permissions.maintenance,
            Permission.Admin.MaxPlayers,
            Permission.Admin.TileProvider,
            Permission.Admin.ResetCharacterOther,
            Permission.Admin.ExportCharacter,
            Permission.Admin.ApplyDefaultPermission,
            Permission.Admin.GenerateFullConfig);

        Utils.AliasPermission(TShockAPI.Permissions.su,
            Permission.Admin.Sudo,
            Permission.Admin.ResetCharacterAll);
    }
}