


CREATE VIEW [dbo].[LatestNews]
AS
SELECT     TOP (1) CAT.Name AS Category, [TOP].Name AS Topic, POST.DateCreated AS Date, POST.PostContent AS Post, [USER].UserName,
	[SITEUSER].[Avatar]
FROM         [$(r3musForumDB)].dbo.Category AS CAT WITH (NOLOCK) INNER JOIN
                      [$(r3musForumDB)].dbo.Topic AS [TOP] WITH (NOLOCK) ON CAT.Id = [TOP].Category_Id INNER JOIN
                      [$(r3musForumDB)].dbo.Post AS POST WITH (NOLOCK) ON [TOP].Id = POST.Topic_Id INNER JOIN
                      [$(r3musForumDB)].dbo.MembershipUser AS [USER] WITH (NOLOCK) ON [USER].Id = POST.MembershipUser_Id
                      INNER JOIN [dbo].[AspNetUsers] AS [SITEUSER] WITH (NOLOCK)
						ON [USER].[UserName] COLLATE DATABASE_DEFAULT = [SITEUSER].[UserName] COLLATE DATABASE_DEFAULT
WHERE     (CAT.Slug = 'external-news') AND (POST.IsTopicStarter = 1)
ORDER BY Date DESC
UNION ALL
SELECT     TOP (1) CAT.Name AS Category, [TOP].Name AS Topic, POST.DateCreated AS Date, POST.PostContent AS Post, [USER].UserName,
	[SITEUSER].[Avatar]
FROM         [$(r3musForumDB)].dbo.Category AS CAT WITH (NOLOCK) INNER JOIN
                      [$(r3musForumDB)].dbo.Topic AS [TOP] WITH (NOLOCK) ON CAT.Id = [TOP].Category_Id INNER JOIN
                      [$(r3musForumDB)].dbo.Post AS POST WITH (NOLOCK) ON [TOP].Id = POST.Topic_Id INNER JOIN
                      [$(r3musForumDB)].dbo.MembershipUser AS [USER] WITH (NOLOCK) ON [USER].Id = POST.MembershipUser_Id
                      INNER JOIN [dbo].[AspNetUsers] AS [SITEUSER] WITH (NOLOCK)
						ON [USER].[UserName] COLLATE DATABASE_DEFAULT = [SITEUSER].[UserName] COLLATE DATABASE_DEFAULT
WHERE     (CAT.Slug = 'internal-news') AND (POST.IsTopicStarter = 1)
ORDER BY Date DESC
GO
