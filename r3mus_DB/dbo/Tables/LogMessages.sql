CREATE TABLE [dbo].[LogMessages] (
    [LogDateTime] DATETIME      NOT NULL,
    [UserName]    VARCHAR (200) NOT NULL,
    [Message]     VARCHAR (500) NOT NULL,
    CONSTRAINT [PK_LogLines] PRIMARY KEY CLUSTERED ([LogDateTime] ASC, [UserName] ASC, [Message] ASC)
);

