CREATE OR ALTER PROCEDURE dbo.DropIndex
	@tableName NVARCHAR(MAX),
	@columnName NVARCHAR(MAX)
AS
	DECLARE @sql NVARCHAR(MAX)
	DECLARE @indexName NVARCHAR(MAX)

	SET @indexName = 'IDX_' + @tableName + @columnName

	SET @sql = 'IF EXISTS(SELECT * FROM sys.indexes WHERE name = ''' + @indexName + ''' AND object_id = OBJECT_ID(''' + @tableName + '''))
		BEGIN
			DROP INDEX "' + @indexName + '" ON ' + @tableName + '
			PRINT ''Index Dropped: ' + @indexName + ', ' + @tableName + '''
		END'

	PRINT @sql
	EXEC sp_executesql @sql