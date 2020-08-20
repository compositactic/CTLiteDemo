CREATE OR ALTER PROCEDURE dbo.CreateOrModifyColumn
	@tableName NVARCHAR(MAX),
	@columnName NVARCHAR(MAX),
	@columnType NVARCHAR(MAX)
AS
	DECLARE @sql NVARCHAR(MAX)

	SET @sql = 'IF COL_LENGTH(''' + @tableName + ''',''' + @columnName + ''') IS NULL
					BEGIN
						ALTER TABLE "' + @tableName + '" ADD "' + @columnName + '" ' + @columnType + '
						PRINT ''Added column: ' + @tableName + '.' + @columnName + ' ' + @columnType + '''
					END
				 ELSE 
					BEGIN
						ALTER TABLE "' + @tableName + '" ALTER COLUMN "' + @columnName + '" ' + @columnType + '
						PRINT ''Modified column: ' + @tableName + '.' + @columnName + ' ' + @columnType + '''
					END'
	PRINT @sql
	EXEC sp_executesql @sql