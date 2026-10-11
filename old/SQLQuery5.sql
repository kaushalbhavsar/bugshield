USE [BugSkanDB]
GO

DECLARE	@return_value Int

EXEC	@return_value = [dbo].[Rpt_AttackCountByDate]

SELECT	@return_value as 'Return Value'

GO
