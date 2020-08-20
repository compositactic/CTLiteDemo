CREATE OR ALTER PROCEDURE dbo.DropTable 
    @tableName NVARCHAR(MAX) 
AS
	DECLARE @sql NVARCHAR(MAX)

	SET @sql = 'IF EXISTS (SELECT * FROM sys.tables WHERE Name = ''' + @tableName + ''')
		BEGIN
			DROP TABLE "' + @tableName + '"
			PRINT ''Table dropped: ' + @tableName + '''
		END'

	PRINT @sql	
	EXEC sp_executesql @sql