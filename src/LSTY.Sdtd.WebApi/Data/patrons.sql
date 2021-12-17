--PRAGMA FOREIGN_KEYS = ON;			--启用外键

--配置项
CREATE TABLE IF NOT EXISTS T_Config(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	LastSaveDate TIMESTAMP,			--最后保存日期
	Name TEXT,						--配置名称
	SerializedContent TEXT			--序列化的内容
);

--Player
CREATE TABLE IF NOT EXISTS T_Player(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	PlatformUserId TEXT,				--平台用户Id
	PlatformType TEXT,					--平台类型
	EntityId INTEGER,					--实体Id
	Name TEXT,							--玩家名称
	IP TEXT,							--IP地址
	TotalPlayTime REAL,					--总游戏时长（单位：分钟）
	LastOnline TIMESTAMP,				--上次在线
	LastPositionX INTEGER,				--X坐标
	LastPositionY INTEGER,				--Y坐标
	LastPositionZ INTEGER,				--Z坐标
	[Level] REAL,						--等级
	Score INTEGER,						--评分
	ZombieKills INTEGER,				--击杀僵尸
	PlayerKills INTEGER,				--击杀玩家
	Deaths INTEGER						--死亡次数
);

--Inventory
CREATE TABLE IF NOT EXISTS T_Inventory(
	EntityId INTEGER PRIMARY KEY,		--EntityId
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	SerializedContent TEXT				--Inventory Content
);

--聊天日志
CREATE TABLE IF NOT EXISTS T_ChatLog(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	EntityId INTEGER,
	ChatType INTEGER,
	Message TEXT
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_SteamId ON T_ChatLog(EntityId);

CREATE VIEW IF NOT EXISTS V_ChatLog AS
SELECT _log.Id,
_log.CreatedDate,
_log.EntityId,
_log.ChatType,
_log.Message,
player.Name AS PlayerName,
player.PlatformUserId
FROM T_ChatLog AS _log LEFT JOIN T_Player AS player ON _log.EntityId=player.EntityId;
