--PRAGMA FOREIGN_KEYS = ON;			--启用外键

--Player
CREATE TABLE IF NOT EXISTS T_Player(
	EntityId INTEGER PRIMARY KEY,		--实体Id
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	PlatformUserId TEXT,				--平台用户Id
	PlatformType TEXT,					--平台类型
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
	Deaths INTEGER,						--死亡次数
	EOS TEXT							--跨平台Id
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_PlatformUserId ON T_Player(PlatformUserId);
CREATE INDEX IF NOT EXISTS Index_Name ON T_Player(Name);
CREATE UNIQUE INDEX IF NOT EXISTS Index_EOS ON T_Player(EOS);


--Inventory
CREATE TABLE IF NOT EXISTS T_Inventory(
	EntityId INTEGER PRIMARY KEY,		--EntityId
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	SerializedContent TEXT				--Inventory Content
);

--聊天日志
CREATE TABLE IF NOT EXISTS T_ChatRecord(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	CreatedDate TIMESTAMP DEFAULT (DATETIME(CURRENT_TIMESTAMP,'LOCALTIME')),
	EntityId INTEGER,
	SenderName TEXT,
	ChatType INTEGER,
	Message TEXT
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_EntityId ON T_ChatRecord(EntityId);
CREATE INDEX IF NOT EXISTS Index_SenderName ON T_ChatRecord(SenderName);

CREATE VIEW IF NOT EXISTS V_ChatRecord AS
SELECT _log.Id,
_log.CreatedDate,
_log.EntityId,
_log.SenderName,
_log.ChatType,
_log.Message,
player.PlatformUserId,
player.EOS
FROM T_ChatRecord AS _log LEFT JOIN T_Player AS player ON _log.EntityId=player.EntityId;
