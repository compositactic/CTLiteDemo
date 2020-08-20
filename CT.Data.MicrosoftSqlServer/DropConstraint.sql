CREATE OR ALTER PROCEDURE dbo.DropConstraint
	@tableName NVARCHAR(MAX),
	@constraintName NVARCHAR(MAX)
AS
	DECLARE @sql NVARCHAR(MAX)
	DECLARE @indexName NVARCHAR(MAX)

	SET @sql = 'IF OBJECT_ID(''' + @constraintName + ''') IS NOT NULL AND EXISTS (SELECT * FROM sys.tables WHERE name = ''' + @tableName + ''')
		BEGIN
			ALTER TABLE "' + @tableName + '" DROP CONSTRAINT "' + @constraintName + '" 
			PRINT ''Dropped constraint: ' + @constraintName + ',  ' + @tableName + ''' 
		END'

	PRINT @sql
	EXEC sp_executesql @sql