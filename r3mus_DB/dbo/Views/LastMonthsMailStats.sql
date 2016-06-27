




CREATE VIEW [dbo].[LastMonthsMailStats]
AS
	SELECT DISTINCT
		RANK() OVER (ORDER BY COUNT ([Name]) ASC) AS [Id],
		[Mailer],
		(
			COUNT ([Name])
		) 
		AS [Mailed]
	FROM
	(		
		SELECT [MAILEES1].[Name], [MAIL1].[UserName] AS [Mailer]
		FROM [dbo].[RecruitmentMailees] AS [MAILEES1] WITH (NOLOCK)
		INNER JOIN [dbo].[AspNetUsers] AS [MAIL1] WITH (NOLOCK)
			ON [MAIL1].[Id] = [MAILEES1].[MailerId]
		WHERE [MAILEES1].[Mailed] >= DATEADD(DD, -30, GETDATE())
		UNION ALL
		SELECT [MAILEES2].[Name], [MAIL2].[UserName] AS [Mailer]
		FROM [$(r3mus_ArchiveDB)].[dbo].[RecruitmentMailees] AS [MAILEES2] WITH (NOLOCK)
		INNER JOIN [dbo].[AspNetUsers] AS [MAIL2] WITH (NOLOCK)
			ON [MAIL2].[Id] = [MAILEES2].[MailerId]
		WHERE [MAILEES2].[Mailed] >= DATEADD(DD, -30, GETDATE())
	) AS [a]
	GROUP BY [Mailer]