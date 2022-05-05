
CREATE TABLE [dbo].[Genders] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[UserTypes] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[UnverifiedEmails] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [UserId]           INT            NOT NULL,
    [Email]            NVARCHAR (MAX) NOT NULL,
    [VerificationCode] INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[ResetPasswordCommands] (
    [Id]               INT IDENTITY (1, 1) NOT NULL,
    [UserId]           INT NOT NULL,
    [VerificationCode] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[Users] (
    [Id]           INT            IDENTITY (1, 1) NOT NULL,
    [UserTypeId]   INT            NOT NULL,
    [FirstName]    NVARCHAR (50)  NOT NULL,
    [LastName]     NVARCHAR (50)  NOT NULL,
    [Email]        NVARCHAR (MAX) NOT NULL,
    [Avatar]       NVARCHAR (MAX) NULL,
    [GenderId]     INT            NOT NULL,
    [Password]     NVARCHAR (50)  NOT NULL,
    [CreationDate] DATETIME       NOT NULL,
    [Verified]     BIT            NOT NULL,
    [Blocked]      BIT            NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([GenderId]) REFERENCES [dbo].[Genders] ([Id]),
    FOREIGN KEY ([UserTypeId]) REFERENCES [dbo].[UserTypes] ([Id])
);
CREATE TABLE [dbo].[Logins] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [UserId]     INT      NOT NULL,
    [LoginDate]  DATETIME NOT NULL,
    [LogoutDate] DATETIME NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id])
);
CREATE TABLE [dbo].[FriendShips] (
    [Id]           INT      IDENTITY (1, 1) NOT NULL,
    [UserId]       INT      NOT NULL,
    [TargetUserId] INT      NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    [Accepted]     BIT      NOT NULL,
    [Declined]     BIT      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);