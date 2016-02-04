CREATE TABLE [dbo].[CRONJobs] (
    [JobName]       NVARCHAR (50) NOT NULL,
    [Schedule]      INT           NOT NULL,
    [ScheduleUnits] NVARCHAR (50) NOT NULL,
    [LastRun]       DATETIME      NULL,
    [Enabled]       BIT           CONSTRAINT [DF_CRONJobs_Enabled] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_CRONJobs] PRIMARY KEY CLUSTERED ([JobName] ASC)
);

