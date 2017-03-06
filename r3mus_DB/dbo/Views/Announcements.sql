

CREATE VIEW [dbo].[Announcements]
AS
SELECT     CAT.Name AS Category, [TOP].Name AS Topic, 
	CAST(POST.DateCreated AS DATETIME2(0)) AS Date, POST.PostContent AS Post, [USER].UserName, SITEUSER.Avatar
FROM         r3mus_Forum.dbo.Category AS CAT WITH (NOLOCK) INNER JOIN
                      r3mus_Forum.dbo.Topic AS [TOP] WITH (NOLOCK) ON CAT.Id = [TOP].Category_Id INNER JOIN
                      r3mus_Forum.dbo.Topic_Tag AS LINK1 WITH (NOLOCK) ON [TOP].Id = LINK1.TopicTag_Id INNER JOIN
                      r3mus_Forum.dbo.TopicTag AS SLUG WITH (NOLOCK) ON SLUG.Id = LINK1.Topic_Id INNER JOIN
                      r3mus_Forum.dbo.Post AS POST WITH (NOLOCK) ON POST.Topic_Id = [TOP].Id INNER JOIN
                      r3mus_Forum.dbo.MembershipUser AS [USER] WITH (NOLOCK) ON [USER].Id = POST.MembershipUser_Id INNER JOIN
                      dbo.AspNetUsers AS SITEUSER WITH (NOLOCK) ON [USER].UserName COLLATE DATABASE_DEFAULT = SITEUSER.UserName COLLATE DATABASE_DEFAULT
WHERE     (POST.IsTopicStarter = 1) AND (SLUG.Tag = 'oyez') AND (POST.DateCreated > DATEADD(HH, - 12, GETDATE()))
GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 2, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'Announcements';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane2', @value = N' 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'Announcements';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "CAT"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 114
               Right = 195
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "TOP"
            Begin Extent = 
               Top = 6
               Left = 233
               Bottom = 114
               Right = 408
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "LINK1"
            Begin Extent = 
               Top = 6
               Left = 446
               Bottom = 99
               Right = 597
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SLUG"
            Begin Extent = 
               Top = 6
               Left = 635
               Bottom = 99
               Right = 786
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "POST"
            Begin Extent = 
               Top = 6
               Left = 824
               Bottom = 114
               Right = 999
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "USER"
            Begin Extent = 
               Top = 6
               Left = 1037
               Bottom = 114
               Right = 1266
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "SITEUSER"
            Begin Extent = 
               Top = 6
               Left = 1304
               Bottom = 114
               Right = 1500
            End
            DisplayFlags = 280
            TopColumn =', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'Announcements';

