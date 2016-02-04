CREATE TABLE [dbo].[RecruitmentMailees] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [Name]               NVARCHAR (MAX) NULL,
    [Submitted]          DATETIME2 (7)  NOT NULL,
    [Mailed]             DATETIME       NULL,
    [SubmitterId]        NVARCHAR (MAX) NULL,
    [MailerId]           NVARCHAR (MAX) NULL,
    [CorpId_AtLastCheck] BIGINT         DEFAULT ((0)) NOT NULL,
    [DateOfBirth]        DATETIME2 (7)  NULL,
    [LastUpdated]        DATETIME2 (7)  NULL,
    CONSTRAINT [PK_dbo.RecruitmentMailees] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE NONCLUSTERED INDEX [RM_1]
    ON [dbo].[RecruitmentMailees]([Submitted] ASC, [Mailed] ASC, [CorpId_AtLastCheck] ASC, [DateOfBirth] ASC, [LastUpdated] ASC)
    INCLUDE([Name], [SubmitterId], [MailerId]);

