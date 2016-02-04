



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
		SELECT [MAILEES].[Name], [MAIL].[UserName] AS [Mailer]
		FROM [dbo].[RecruitmentMailees] AS [MAILEES] WITH (NOLOCK)
		INNER JOIN [dbo].[AspNetUsers] AS [MAIL] WITH (NOLOCK)
			ON [MAIL].[Id] = [MAILEES].[MailerId]
		WHERE [MAILEES].[Mailed] >= DATEADD(MM, -30, GETDATE())
	) AS [a]
	GROUP BY [Mailer]