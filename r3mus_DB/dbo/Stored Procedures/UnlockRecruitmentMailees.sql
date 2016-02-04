CREATE PROCEDURE [dbo].[UnlockRecruitmentMailees]
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
	SET [MailerId] = NULL
	WHERE [Name] IN
	(
		SELECT *
		FROM @table
	)

END