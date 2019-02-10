--The application does not support USE statement

--GO
--USE [master];

--GO
--CREATE DATABASE [KIS];

--GO
--USE [KIS];

GO
PRINT N'Creating [Process_Vasilevsky_Alexander] table...';

GO
CREATE TABLE [Process_Vasilevsky_Alexander] (
    [ProcessId]			INT 			NOT NULL IDENTITY(1,1) 	PRIMARY KEY,
    [ProcessName]		VARCHAR (255)	NOT NULL UNIQUE
);

GO
PRINT N'Creating [User_Vasilevsky_Alexander] table...';

GO
CREATE TABLE [User_Vasilevsky_Alexander] (
    [UserId]			INT 			NOT NULL IDENTITY(1,1) 	PRIMARY KEY,
    [UserName]			VARCHAR (255)	NOT NULL UNIQUE
);

GO
PRINT N'Creating [Session_Vasilevsky_Alexander] table...';

GO
CREATE TABLE [Session_Vasilevsky_Alexander] (
    [SessionId]			INT 			NOT NULL IDENTITY(1,1) 	PRIMARY KEY,
    [Process]			INT				NOT NULL				FOREIGN KEY REFERENCES Process_Vasilevsky_Alexander([ProcessId]),
    [User]				INT				NOT NULL				FOREIGN KEY REFERENCES User_Vasilevsky_Alexander([UserId]),
	[PID]				INT				NOT NULL,
    [StartTime]			DATETIME		NOT NULL,
    [EndTime]			DATETIME		NULL,
	CONSTRAINT [CS_START_LT_END] CHECK ([StartTime] <= [EndTime])
);

--GO
--DROP TABLE [Event_Vasilevsky_Alexander];

--GO
--USE [master];

--GO
--DROP DATABASE [KIS];