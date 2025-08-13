# Chireiden.TShock.Omni & Misc

- 作者: SGKoishi
- 出处: [github](https://github.com/sgkoishi/yaaiomni)
- TShock的又一多功能插件集合，包含修复补丁、功能增强、实用工具、调试命令等。

### 常用功能
* `/whynot` 查看玩家最近的权限查询记录，终极解决"需要什么权限"类问题
* `/setlang`, `/maxplayers` 设置服务器语言和最大玩家数
* `/settimeout`, `/setinterval`, `/clearinterval`, `/showdelay` 基于定时器自动执行命令
* `/runas` 以其他玩家身份执行命令
* `/resetcharacter`, `/exportcharacter` 重置或导出角色数据
* 聊天防刷屏限制：3条/5秒，5条/20秒（配置项`.Mitigation.ChatSpamRestrict`）

### 更多特性

* `.PlayerWildcardFormat`: 支持`/g zenith *all*`式通配符
* `.HideCommands`和`.StartupCommands`可隐藏命令或设置启动时自动执行
* `.Enhancements.AlternativeCommandSyntax`支持`/命令1 ; 命令2 ; 命令3...`和`/命令1 && 命令2 && 命令3...`语法
* `.Mode.Vanilla.Enabled`会为玩家添加原版游戏体验所需权限
* `.CommandRenames`: 支持命令别名配置，如`{"Chireiden.TShock.Omni.Plugin.Command_PermissionCheck": ["whynot123", "whynot456"]}`

### 高级选项

执行`/genconfig`可生成完整配置文件。隐藏选项将显示（未修改的条目会在下次启动/重载时恢复隐藏状态）。

> [!CAUTION]
> **保持默认设置。除非您明确知道修改后果，否则请勿更改**

### 扩展功能

`Chireiden.TShock.Omni.Misc`插件包含多项随机功能：
* 基于权限限制特定Boss召唤、队伍状态和PVP状态
* `.LavaHandler`防止岩浆刷屏（不阻止岩浆生成，但会在可能生成后立即清除）
* 可在其他插件的小游戏中使用`/echo`、`/_pvp`、`/_team`等命令

（翻译说明：保留技术术语原文格式如命令名/config键名；调整了部分长句的语序使其符合中文表达习惯；将被动语态转换为主动表述；补充了必要的说明性文字）

## 指令

| 语法                                                                              |                             权限                             | 说明                                 |
|---------------------------------------------------------------------------------|:----------------------------------------------------------:|------------------------------------|
| `/_gc`<br>`/_gc -f`                                                             |                 `chireiden.omni.admin.gc`                  | 触发垃圾回收（`-f` 强制完整GC）                |
| `/_sv`                                                                          |                 `chireiden.omni.admin.sv`                  | 执行SQLite数据库压缩（VACUUM）              |
| `/rbc <消息>`<br>`/rawbroadcast <消息>`                                             |            `chireiden.omni.admin.rawbroadcast`             | 发送原始广播消息（无格式）                      |
| `/listclients`                                                                  |             `chireiden.omni.admin.listclients`             | 列出所有连接的客户端信息                       |
| `/dumpbuffer <玩家ID> [文件名]`                                                      |             `chireiden.omni.admin.dumpbuffer`              | 导出玩家网络缓冲区数据到文件                     |
| `/whereis <命令名>`                                                                |               `chireiden.omni.admin.whereis`               | 查找命令所属插件和程序集                       |
| `/kc <玩家ID>`                                                                    |           `chireiden.omni.admin.terminatesocket`           | 强制关闭玩家网络连接                         |
| `/_ups`<br>`/_ups bench`                                                        |              `chireiden.omni.admin.upscheck`               | 检查服务器每秒更新次数（`bench` 运行性能测试）        |
| `/_pvp [玩家名] <true/false>`                                                      |  `chireiden.omni.setpvp`<br>`chireiden.omni.admin.setpvp`  | 设置PvP状态（管理员可指定其他玩家）                |
| `/_team [玩家名] <队伍ID>`                                                           | `chireiden.omni.setteam`<br>`chireiden.omni.admin.setteam` | 设置队伍（0无队伍，1红，2绿，3蓝，4黄，5粉）          |
| `/_chat <消息>`                                                                   |                   `chireiden.omni.chat`                    | 模拟发送游戏内聊天消息                        |
| `/_csf`                                                                         |           `chireiden.omni.admin.callstackframe`            | 显示当前调用堆栈（调试用）                      |
| `/genconfig`                                                                    |              `chireiden.omni.admin.genconfig`              | 生成完整配置文件（<br/>显示隐藏选项）              |
| `/tileprovider <default\|heaptile\|constilation\|checkedtyped\|checkedgeneric>` |            `chireiden.omni.admin.tileprovider`             | 切换地图读写接口类型（内存优化）                   |
| `/ghost [-v\|-a\|-u]`                                                           |                   `chireiden.omni.ghost`                   | 切换幽灵状态（-v: 客户端幽灵 -a: 活动状态 -u: 取消）  |
| `/setlang [-g\|-t] [语言代码]`                                                      |                  `chireiden.omni.setlang`                  | 设置游戏/TShock语言（-g: 仅游戏 -t: 仅TShock） |
| `/maxplayers [数量]`                                                              |             `chireiden.omni.admin.maxplayers`              | 查看/设置最大玩家数                         |
| `/runas <玩家> <命令> [-f]`                                                         |                `chireiden.omni.admin.sudo`                 | 以其他玩家身份执行命令（-f: 跳过权限检查）            |
| `/resetcharacter [-f] [玩家]`                                                     |              `chireiden.omni.resetcharacter`               | 重置角色数据（需确认，支持通配符）                  |
| `/exportcharacter [玩家]`                                                         |           `chireiden.omni.admin.exportcharacter`           | 导出角色数据为.plr文件                      |
| `/echo <消息>`                                                                    |                   `chireiden.omni.echo`                    | 回显消息                               |
| `/_setperm`                                                                     |              `chireiden.omni.admin.setupperm`              | 应用默认权限设置                           |
| `/genconfig`                                                                    |              `chireiden.omni.admin.genconfig`              | 生成完整配置文件                           |
| `/_qbg <命令> [-t]`                                                               |            `chireiden.omni.admin.runbackground`            | 后台执行命令（-t: 使用Task运行）               |
| `/_locked <命令>`                                                                 |               `chireiden.omni.admin.locked`                | 锁定模式执行命令                           |
| `/whynot [-t\|-f\|-v]`                                                          |                  `chireiden.omni.whynot`                   | 查看权限检查历史（-t: 成功 -f: 失败 -v: 详细堆栈）   |
| `/_ping`                                                                        |                   `chireiden.omni.ping`                    | 测试玩家延迟                             |
| `/_debugstat`                                                                   |              `chireiden.omni.admin.debugstat`              | 输出调试统计信息                           |
| `/settimeout <命令> <间隔>`                                                         |                  `chireiden.omni.timeout`                  | 延迟执行命令（单位: 游戏帧）                    |
| `/setinterval <命令> <间隔>`                                                        |                 `chireiden.omni.interval`                  | 循环执行命令                             |
| `/clearinterval <ID>`                                                           |               `chireiden.omni.cleartimeout`                | 取消延迟/循环命令                          |
| `/showdelay`                                                                    |                `chireiden.omni.showtimeout`                | 查看待执行命令列表                          |
| `/trytileframe [x] [y]`                                                         |            `chireiden.omni.admin.trytileframe`             | 测试TileFrame计算（可能造成卡顿）              |
| `/inspecttileframe`                                                             |          `chireiden.omni.admin.inspecttileframe`           | 启用TileFrame检查（高级调试）                |


## 配置
> 配置文件位置：tshock/chireiden.omni.json
```json5
{
  // 是否在加载/重载时显示配置文件内容
  "ShowConfig": false,

  // 是否记录所有异常日志
  "LogFirstChance": false,

  // 日志时间格式（遵循.NET DateTime格式规范）
  "DateTimeFormat": "yyyy-MM-dd HH:mm:ss.fff",

  // 是否优先处理网络数据包（可能影响其他插件）
  "PrioritizedPacketHandle": true,

  // 匹配所有玩家的通配符格式（避免直接使用"*"可能与命令冲突）
  "PlayerWildcardFormat": [
    "*all*"
  ],

  // 匹配服务器控制台的通配符格式
  "ServerWildcardFormat": [
    "*server*",
    "*console*"
  ],

  // 隐藏的命令列表（不会显示在帮助菜单中）
  "HideCommands": [
    "whynot",
    "_debugstat",
    "resetcharacter", 
    "_ping",
    "echo",
    "_setperm",
    "inspecttileframe",
    "_qbg",
    "_locked"      
  ],

  // 服务器启动时自动执行的命令列表
  "StartupCommands": [],

  // 命令重命名映射表（键为原始命令全名，值为别名列表）
  "CommandRenames": {},

  // 功能增强设置
  "Enhancements": {
    // 是否定期清理未使用的客户端对象以节省内存
    "TrimMemory": true,

    // 是否启用替代命令语法（支持多命令分隔符）
    "AlternativeCommandSyntax": true,

    // 是否允许命令行参数覆盖配置文件
    "CLIoverConfig": true,

    // 是否修复默认语言检测问题
    "DefaultLanguageDetect": true,

    // TShock更新提示处理方式
    // 可选值: Silent(静默), Disabled(禁用), AsIs(保持原样)
    "SuppressUpdate": "Silent",

    // 网络套接字实现类型（影响内存使用）
    // 可选值: Vanilla(原版), TShock, AsIs, Unset, HackyBlocked, 
    //        HackyAsync, AnotherAsyncSocket, AnotherAsyncSocketAsFallback
    "Socket": "AnotherAsyncSocketAsFallback",

    // 玩家重名处理方式
    // 可选值: First(踢先登录者), Second(踢后登录者), Both(都踢), 
    //        None(都不踢), Known(踢未验证IP者), Unhandled(不处理)
    "NameCollision": "Unhandled",

    // 地图图格提供器
    // 可选值: AsIs(默认), CheckedTypedCollection, CheckedGenericCollection
    "TileProvider": "AsIs",

    // 是否支持超大型世界（可能导致原版客户端崩溃）
    "ExtraLargeWorld": true,

    // 帮助菜单中显示命令别名的级别（0=关闭）
    "ShowCommandAlias": 0,

    // 是否支持封禁规则中的正则表达式和IP掩码
    "BanPattern": true,

    // 是否尝试解析已加载程序集的引用
    "ResolveAssembly": true,

    // 是否启用IPv6双栈支持
    "IPv6DualStack": true
  },

  // 数据包调试设置
  "DebugPacket": {
    // 是否记录传入数据包
    "In": false,

    // 是否记录传出数据包
    "Out": false,

    // 是否记录原始字节流
    "BytesOut": false,

    // 异常显示级别
    // 可选值: None(不显示), Uncommon(非常见异常), All(所有异常)
    "ShowCatchedException": "Uncommon"
  },

  // 健壮性修复设置
  "Soundness": {
    // 是否限制弹幕类物品修改地形（如液体炸弹）
    "ProjectileKillMapEditRestriction": true,

    // 是否要求快速堆叠需要建筑权限
    "QuickStackRestriction": true,

    // 是否要求编辑告示牌需要建筑权限
    "SignEditRestriction": true,

    // 是否要求与图格实体交互需要建筑权限
    "ObjectInteractionRestriction": true,

    // 编码设置（-1=自动检测，0=UTF8）
    "UseDefaultEncoding": 0,

    // 是否强制使用英文命令（解决多语言环境问题）
    "UseEnglishCommand": true,

    // 是否允许原版本地化命令（需UseEnglishCommand启用）
    "AllowVanillaLocalizedCommand": true
  },

  // 权限系统设置
  "Permission": {
    "Log": {
      // 是否启用权限查询日志
      "Enabled": true,

      // 每个玩家保存的日志条数
      "LogCount": 50,

      // 是否记录重复权限检查
      "LogDuplicate": false,

      // 区分相同权限检查的时间间隔（秒）
      "LogDistinctTime": 1.0,

      // 是否记录堆栈轨迹
      "LogStackTrace": false
    },
    "Preset": {
      // 是否启用预设权限组
      "Enabled": true,

      // 是否始终应用预设权限
      "AlwaysApply": false,

      // 是否仅对管理员显示调试信息
      "DebugForAdminOnly": false
    }
  },

  // 游戏模式设置
  "Mode": {
    // 建筑模式设置
    "Building": {
      // 是否启用建筑模式
      "Enabled": false
    },

    // PvP模式设置
    "PvP": {
      // 是否强制开启PvP
      "Enabled": false
    },

    // 原版体验模式设置
    "Vanilla": {
      // 是否启用原版模式
      "Enabled": false,

      // 自动授予的权限列表
      "Permissions": [
        "tshock.account.register",  // 注册账号
        "tshock.account.login",     // 登录
        /* 其他权限省略... */
      ],

      // 是否允许旅途模式能力
      "AllowJourneyPowers": false,

      // 是否忽略反作弊检测
      "IgnoreAntiCheat": false,

      // 原版反作弊设置
      "AntiCheat": {
        // 是否启用原版反作弊
        "Enabled": false
      }
    }
  },

  // 问题缓解设置（警告：修改可能影响稳定性）
  "Mitigation": {
    // 是否禁用所有缓解措施
    "DisableAllMitigation": false,

    // 是否处理移动端物品栏同步问题（内存优化）
    "InventorySlotPE": true,

    // 是否修复移动端药水冷却绕过问题
    "PotionSicknessPE": true,

    // 是否阻止移动端使用物品时切换栏位
    "SwapWhileUsePE": true,

    // 是否回滚移动端物品切换操作（可能导致延迟）
    "SwapWhileUsePEHandleAttempt": false,

    // 聊天刷屏限制配置（格式：速率限制/时间窗口）
    "ChatSpamRestrict": [
      "1.6/5",  // 5秒内不超过1.6条消息
      "4/20"    // 20秒内不超过4条消息
    ],

    // 是否限制NPCBuff更新频率（防止网络风暴）
    "NpcUpdateBuffRateLimit": false,

    // 终端标题抑制模式
    // 可选值: Disabled(禁用), Smart(智能), Enabled(启用)
    "SuppressTitle": "Smart",

    // 连接频率限制配置
    "ConnectionLimit": [
      "3/5",   // 5秒内不超过3次连接
      "15/60"  // 60秒内不超过15次连接
    ],

    // 受限网络类型
    // 可选值: All(所有连接), Public(仅公网IP), None(不限制)
    "LimitedNetwork": "Public",

    // 连接状态超时设置（秒）
    "ConnectionStateTimeout": {
      "0": 1.0,  // 套接字创建后1秒
      "1": 4.0   // 收到连接请求后总共4秒
    },

    // 禁用玩家受伤处理方式
    // 可选值: AsIs(原样), Hurt(允许受伤), Ghost(幽灵模式)
    "DisabledDamageHandler": "Hurt",

    // 专家模式金币处理方式
    // 可选值: DisableValue(禁用金币值), ServerSide(服务端处理), AsIs(原样)
    "ExpertExtraCoin": "ServerSide",

    // 是否保持REST连接活跃
    "KeepRestAlive": true,

    // 部分更新配置处理方式
    // 可选值: Ignore(忽略), Replace(替换)
    "AcceptPartialUpdatedConfig": "Replace",

    // 是否检测物品ID溢出（防作弊）
    "OverflowWorldGenItemID": false,

    // 是否清除堆栈溢出时的图格数据（调试用）
    "ClearOverflowWorldGenStackTrace": false,

    // 堆栈溢出时是否保存地图快照
    "DumpMapOnStackOverflowWorldGen": true,

    // 是否使用非递归方式统计图格（防崩溃）
    "NonRecursiveWorldGenTileCount": true,

    // 是否允许旅途/非旅途玩家共存
    "AllowCrossJourney": false,

    // 是否在没有SSC时启用装备切换
    "LoadoutSwitchWithoutSSC": true,

    // 数据包频率限制配置（null=禁用）
    "PacketSpamLimit": null,

    // 是否严格限制套接字发送（防内存泄漏）
    "RestrictiveSocketSend": true,

    // 是否回显未变化的物品栏（防复制漏洞）
    "EchoUnchangedItem": true,

    // 是否允许重载IL钩子（可能不稳定）
    "ReloadILHook": false,

    // 是否检测递归图格破坏
    "RecursiveTileBreak": false,

    // 是否启用增量式箱子堆叠同步（实验性）
    "IncrementalChestStack": false,

    // 是否允许非原版名称更改（防作弊）
    "AllowNonVanillaNameChange": false,

    // 是否允许非标准连接状态包（兼容性选项）
    "AllowNonVanillaJoinState": false
  }
}
```
> 配置文件位置：tshock/chireiden.omni.misc.json
```json5
{
  // 功能增强设置
  "Enhancements": {
    // 是否同步客户端和服务端版本号（用于绕过版本验证）
    // 注意：启用可能导致兼容性问题
    "SyncVersion": false
  },

  // 岩浆处理设置 
  "LavaHandler": {
    // 是否启用岩浆处理系统（防止岩浆刷屏）
    "Enabled": false,

    // 是否允许地狱石生成岩浆
    "AllowHellstone": false,

    // 是否允许脆蜂蜜块生成岩浆
    "AllowCrispyHoneyBlock": false,

    // 是否允许地狱蝙蝠死亡生成岩浆
    "AllowHellbat": false,

    // 是否允许熔岩史莱姆死亡生成岩浆
    "AllowLavaSlime": false,

    // 是否允许熔岩蝙蝠死亡生成岩浆
    "AllowLavabat": false
  },

  // 权限控制系统  
  "Permission": {
    // 行为限制设置
    "Restrict": {
      // 是否启用权限限制系统
      "Enabled": false,

      // 是否限制队伍切换
      // 权限：
      // - chireiden.omni.toggleteam
      // - chireiden.omni.toggleteam.[队伍ID]
      "ToggleTeam": true,

      // 是否限制PvP状态切换
      // 权限：
      // - chireiden.omni.togglepvp
      // - chireiden.omni.togglepvp.[true/false]
      "TogglePvP": true,

      // 是否限制装备栏同步
      // 权限：chireiden.omni.syncloadout
      "SyncLoadout": true,

      // 是否限制Boss召唤
      // 权限：
      // - tshock.npc.summonboss
      // - chireiden.omni.summonboss.[BossID]
      "SummonBoss": true
    },

    // 预设权限组设置
    "Preset": {
      // 是否允许受限玩家使用预设权限
      // 影响群体：默认访客组(DefaultGuestGroup)
      "AllowRestricted": true
    }
  }
}
```


## 反馈
- 优先发issued -> 共同维护的插件库：https://github.com/UnrealMultiple/TShockPlugin
- 次优先：TShock官方群：816771079
- 大概率看不到但是也可以：国内社区trhub.cn ，bbstr.net , tr.monika.love