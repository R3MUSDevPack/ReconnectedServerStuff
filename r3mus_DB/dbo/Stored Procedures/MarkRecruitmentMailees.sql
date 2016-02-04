
CREATE PROCEDURE [dbo].[MarkRecruitmentMailees]
	@UserID NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @Result TABLE 
	(
		[Id] [int] NOT NULL,
		[Name] [nvarchar](max) NULL,
		[Submitted] [datetime2](7) NOT NULL,
		[Mailed] [datetime] NULL,
		[SubmitterId] [nvarchar](max) NULL,
		[MailerId] [nvarchar](max) NULL,
		[CorpId_AtLastCheck] [bigint] NOT NULL,
		[DateOfBirth] [datetime2](7) NULL,
		[LastUpdated] [datetime2](7) NULL
	);
	INSERT INTO @Result
	SELECT TOP (20) 
		[Id], [Name], [Submitted], [Mailed], [SubmitterId], [MailerId], [CorpId_AtLastCheck], [DateOfBirth], [LastUpdated]
	FROM [dbo].[RecruitmentMailees] WITH (NOLOCK)
	WHERE ([MailerId] IS NULL) 
	AND ([Name] NOT LIKE '%Miner%') 
	AND ([Name] NOT LIKE '%Citizen%') 
	AND ([Name] NOT LIKE '%Trader%') 
	AND (([CorpId_AtLastCheck] BETWEEN 1000000 AND 1000200) 
	AND
	[CorpId_AtLastCheck] > -1)
	AND (DATEDIFF(DAY, [DateOfBirth], GETDATE()) < 548)
	ORDER BY [Submitted] ASC;
	
	UPDATE [dbo].[RecruitmentMailees]
	SET [MailerId] = @UserID
	WHERE [Id] IN
	(
		SELECT [Id]
		FROM @Result
	);

    -- Insert statements for procedure here
	SELECT *
	FROM @Result;
END