CREATE TABLE [dbo].[ApplicantApiInfoes] (
    [ApiKey]           INT            NOT NULL,
    [VerificationCode] NVARCHAR (MAX) NULL,
    [Applicant_Id]     INT            NOT NULL,
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_dbo.ApplicantApiInfoes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.ApplicantApiInfoes_dbo.Applicants_Id] FOREIGN KEY ([Applicant_Id]) REFERENCES [dbo].[Applicants] ([Id])
);

