CREATE OR ALTER PROCEDURE dbo.CreateCheckConstraint
	@tableName NVARCHAR(MAX),
	@constraintName NVARCHAR(MAX),
	@constraintExpression NVARCHAR(MAX)
AS
	DECLARE @sql NVARCHAR(MAX)
	DECLARE @indexName NVARCHAR(MAX)

	SET @sql = 'IF OBJECT_ID(''' + @constraintName + ''') IS NULL AND EXISTS (SELECT * FROM sys.tables WHERE name = ''' + @tableName + ''')
		BEGIN
			ALTER TABLE "' + @tableName + '" ADD CONSTRAINT "' + @constraintName + '" CHECK (' + @constraintExpression + ') 
			PRINT ''Check constraint added: ' + @tableName + '.' + @constraintName + '''
		END'

	PRINT @sql
	EXEC sp_executesql @sql