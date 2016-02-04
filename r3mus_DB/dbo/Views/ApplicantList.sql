






CREATE VIEW [dbo].[ApplicantList]
AS
	SELECT 
		[Applicants].[Id],
		[Applicants].[Name],
		[Applicants].[EmailAddress],
		[Applicants].[ApiKey],
		[Applicants].[VerificationCode],
		[Applicants].[Information],
		[Applicants].[Age],
		[Applicants].[ToonAge],
		[Applicants].[Source],
		[Applied].[DateTimeCreated] AS [Applied],
		[Applications].[DateTimeCreated] AS [LastStatusUpdate],
		[Applications].[Status],
		[Applications].[DateTimeCreated],
		[Applications].[Notes],
		ISNULL([Reviewer].[UserName], '') AS [UserName]
	FROM [dbo].[Applicants] AS [Applicants] WITH (NOLOCK)	
	INNER JOIN [dbo].[Applications] AS [Applications] WITH (NOLOCK)
		ON [Applicants].[Id] = [Applications].[ApplicantId]
		AND [Applications].[DateTimeCreated] = 
		(
			SELECT MAX([DateTimeCreated])
			FROM [dbo].[Applications]
			WHERE [Applicants].[Id] = [Applications].[ApplicantId]
		)		
	INNER JOIN [dbo].[Applications] AS [Applied] WITH (NOLOCK)
		ON [Applicants].[Id] = [Applied].[ApplicantId]
		AND [Applied].[DateTimeCreated] = 
		(
			SELECT MIN([DateTimeCreated])
			FROM [dbo].[Applications]
			WHERE [Applicants].[Id] = [Applications].[ApplicantId]
		)
	LEFT OUTER JOIN [dbo].[AspNetUsers] AS [Reviewer] WITH (NOLOCK)
		ON [Reviewer].[Id] = [Applications].[Reviewer_Id]