





CREATE VIEW [dbo].[LastWeeksSubmissionStats]
AS
	SELECT DISTINCT
		RANK() OVER (ORDER BY COUNT ([Name]) ASC) AS [Id],
		[Submitter],
		(
			COUNT ([Name])
		) 
		AS [Submitted]
	FROM
	(
		SELECT [MAILEES1].[Name], [SUB1].[UserName] AS [Submitter]
		FROM [dbo].[RecruitmentMailees] AS [MAILEES1] WITH (NOLOCK)
		INNER JOIN [dbo].[AspNetUsers] AS [SUB1] WITH (NOLOCK)
			ON [SUB1].[Id] = [MAILEES1].[SubmitterId]
		WHERE [MAILEES1].[Submitted] >= DATEADD(DD, -7, GETDATE())
		UNION ALL
		SELECT [MAILEES2].[Name], [SUB2].[UserName] AS [Submitter]
		FROM [$(r3mus_ArchiveDB)].[dbo].[RecruitmentMailees] AS [MAILEES2] WITH (NOLOCK)
		INNER JOIN [dbo].[AspNetUsers] AS [SUB2] WITH (NOLOCK)
			ON [SUB2].[Id] = [MAILEES2].[SubmitterId]
		WHERE [MAILEES2].[Submitted] >= DATEADD(DD, -7, GETDATE())
	) AS [a]
	GROUP BY [Submitter]