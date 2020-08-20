CREATE OR ALTER PROCEDURE dbo.DropColumn
	@tableName NVARCHAR(MAX),
	@columnName NVARCHAR(MAX)
AS
	DECLARE @sql NVARCHAR(MAX)

	SET @sql = 'IF COL_LENGTH(''' + @tableName + ''',''' + @columnName + ''') IS NOT NULL
					BEGIN
						ALTER TABLE "' + @tableName + '" DROP COLUMN "' + @columnName + '"
						PRINT ''Dropped column: ''' + @columnName + ', ' + @tableName + '
					END'
	PRINT @sql
	EXEC sp_executesql @sql