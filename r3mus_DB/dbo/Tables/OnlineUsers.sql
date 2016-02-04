CREATE TABLE [dbo].[OnlineUsers] (
    [LoggerName]        VARCHAR (200) NOT NULL,
    [LastKnownDateTime] DATETIME      NULL,
    CONSTRAINT [PK_OnlineUsers] PRIMARY KEY CLUSTERED ([LoggerName] ASC)
);

