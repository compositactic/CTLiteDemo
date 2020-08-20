CREATE OR ALTER PROCEDURE dbo.CreateIndex
	@tableName NVARCHAR(MAX),
	@columnName NVARCHAR(MAX)
AS
	DECLARE @sql NVARCHAR(MAX)
	DECLARE @indexName NVARCHAR(MAX)

	SET @indexName = 'IDX_' + @tableName + @columnName

	SET @sql = 'IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = ''' + @indexName + ''' AND object_id = OBJECT_ID(''' + @tableName + '''))
		BEGIN
			CREATE INDEX "' + @indexName + '" ON "' + @tableName + '" (' + @columnName + ')
			PRINT ''Created index: ' + @indexName + ' ON ' + @tableName + '.' + @columnName + '''
		END'

	PRINT @sql
	EXEC sp_executesql @sql