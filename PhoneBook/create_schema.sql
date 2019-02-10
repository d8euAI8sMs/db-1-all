--The application does not support USE statement

--GO
--USE [master];

--GO
--CREATE DATABASE [KIS];

--GO
--USE [KIS];

GO
PRINT N'Creating [City_Vasilevsky_Alexander] table...';

GO
CREATE TABLE [City_Vasilevsky_Alexander] (
    [CityId]	INT 			NOT NULL IDENTITY(1,1) 	PRIMARY KEY,
    [CityName]	VARCHAR (255)	NOT NULL,
    [CityCode]	INT				NOT NULL,
    [StateCode]	INT				NOT NULL
);

GO
PRINT N'Creating [Person_Vasilevsky_Alexander] table...';

GO
CREATE TABLE [Person_Vasilevsky_Alexander] (
    [PersonId]		INT 			NOT NULL IDENTITY(1,1) 	PRIMARY KEY,
    [PersonName]	VARCHAR (255)	NOT NULL,
    [BirthDate]		DATETIME		NOT NULL,
    [PersonAddress]	VARCHAR (255)	NULL,
    [PersonComment]	VARCHAR (255)	NULL
);

GO
PRINT N'Creating [Contact_Vasilevsky_Alexander] table...';

GO
CREATE TABLE [Contact_Vasilevsky_Alexander] (
    [ContactId]		INT 			NOT NULL IDENTITY (1,1)	PRIMARY KEY,
    [Person]		INT				NOT NULL				FOREIGN KEY REFERENCES [Person_Vasilevsky_Alexander] ([PersonId]),
    [ContactType]	CHARACTER(1)	NOT NULL, -- 'm' stands for mobile, 'c' for corporative, 'h' for home
    [Phone]			VARCHAR(31)		NOT NULL,
    [City]			INT				NULL 					FOREIGN KEY REFERENCES [City_Vasilevsky_Alexander] ([CityId]),
    CONSTRAINT CS_PHONE_VALIDATION CHECK ([ContactType] = 'm' AND [City] IS NULL OR [ContactType] != 'm' AND [City] IS NOT NULL)
);

--GO
--DROP TABLE [Contact_Vasilevsky_Alexander];
--DROP TABLE [City_Vasilevsky_Alexander];
--DROP TABLE [Person_Vasilevsky_Alexander];

--GO
--USE [master];

--GO
--DROP DATABASE [KIS];