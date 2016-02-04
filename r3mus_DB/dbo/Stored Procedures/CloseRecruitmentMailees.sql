

CREATE PROCEDURE [dbo].[CloseRecruitmentMailees]
	@Names NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @Delimiter CHAR(1) = ',';

	DECLARE @val NVARCHAR(4000);
	DECLARE @table TABLE ( [val] NVARCHAR(4000) );

	WHILE LEN(@Names) > 0
	BEGIN
		SET @val = LEFT(@Names, ISNULL(NULLIF(CHARINDEX(@Delimiter, @Names) - 1, -1), LEN(@Names)));
		SET @Names = LTRIM(RTRIM(SUBSTRING(@Names, ISNULL(NULLIF(CHARINDEX(@Delimiter, @Names), 0), LEN(@Names)) + 1, LEN(@Names))));
		INSERT INTO @table
		VALUES (@val)
	END
	
	UPDATE [dbo].[RecruitmentMailees] 
	SET [Mailed] = GETDATE()
	WHERE [Name] IN
	(
		SELECT *
		FROM @table
	);	
		
	SET IDENTITY_INSERT [$(r3mus_ArchiveDB)].[dbo].[RecruitmentMailees] ON;
	
	INSERT INTO [$(r3mus_ArchiveDB)].[dbo].[RecruitmentMailees]
	([Id]
		  ,[Name]
		  ,[Submitted]
		  ,[Mailed]
		  ,[SubmitterId]
		  ,[MailerId]
		  ,[CorpId_AtLastCheck]
		  ,[DateOfBirth]
		  ,[LastUpdated]
	)
	SELECT [Id]
		  ,[Name]
		  ,[Submitted]
		  ,[Mailed]
		  ,[SubmitterId]
		  ,[MailerId]
		  ,[CorpId_AtLastCheck]
		  ,[DateOfBirth]
		  ,[LastUpdated]
	  FROM [dbo].[RecruitmentMailees] WITH (NOLOCK)
	  WHERE [MailerId] IS NOT NULL
	  OR  NOT ([CorpId_AtLastCheck] BETWEEN 
						  1000000 AND 1000200)
	OR [CorpId_AtLastCheck] = -1;
	
	SET IDENTITY_INSERT [$(r3mus_ArchiveDB)].[dbo].[RecruitmentMailees] OFF;

	DELETE FROM [dbo].[RecruitmentMailees]
	WHERE [Id] IN
	(
		SELECT [Id]
		FROM [$(r3mus_ArchiveDB)].[dbo].[RecruitmentMailees] WITH (NOLOCK)
	);

END