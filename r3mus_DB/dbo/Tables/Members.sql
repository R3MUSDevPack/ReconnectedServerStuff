CREATE TABLE [dbo].[Members] (
    [ID]                BIGINT         NOT NULL,
    [Name]              NVARCHAR (MAX) NOT NULL,
    [Title]             NVARCHAR (MAX) NULL,
    [MemberSince]       DATETIME       NULL,
    [LastLogonDateTime] NVARCHAR (MAX) NULL,
    [Location]          NVARCHAR (MAX) NULL,
    [ShipType]          NVARCHAR (MAX) NULL,
    [Roles]             NVARCHAR (MAX) NULL,
    [GrantableRoles]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_CorpMembers] PRIMARY KEY CLUSTERED ([ID] ASC)
);

