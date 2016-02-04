




CREATE VIEW [dbo].[Wardecs]
AS
SELECT     --TOP (1) 
	CAT.Name AS Category, [TOP].Name AS Topic, POST.DateCreated AS Date, POST.PostContent AS Post, [USER].UserName,
	[SITEUSER].[Avatar]
FROM         [$(r3mus_ForumDB)].dbo.Category AS CAT WITH (NOLOCK) INNER JOIN
                      [$(r3mus_ForumDB)].dbo.Topic AS [TOP] WITH (NOLOCK) ON CAT.Id = [TOP].Category_Id INNER JOIN
                      [$(r3mus_ForumDB)].dbo.Post AS POST WITH (NOLOCK) ON [TOP].Id = POST.Topic_Id INNER JOIN
                      [$(r3mus_ForumDB)].dbo.MembershipUser AS [USER] WITH (NOLOCK) ON [USER].Id = POST.MembershipUser_Id
                      INNER JOIN [dbo].[AspNetUsers] AS [SITEUSER] WITH (NOLOCK)
						ON [USER].[UserName] COLLATE DATABASE_DEFAULT = [SITEUSER].[UserName] COLLATE DATABASE_DEFAULT
WHERE     (CAT.Slug = 'wardec-info') AND (POST.IsTopicStarter = 1) AND (CAT.IsLocked = 0)
--ORDER BY Date DESC