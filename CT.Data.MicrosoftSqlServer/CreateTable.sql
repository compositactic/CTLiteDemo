CREATE OR ALTER PROCEDURE dbo.CreateTable 
    @tableName NVARCHAR(MAX),
	@parentTableName NVARCHAR(MAX) = '',
	@baseTableName NVARCHAR(MAX) = ''
AS
	DECLARE @sql NVARCHAR(MAX)

	SET @sql = 'IF NOT EXISTS (SELECT * FROM sys.tables WHERE Name = ''' + @tableName + ''')
		BEGIN
			CREATE TABLE "' + @tableName + '" ( [Id] INT NOT NULL PRIMARY KEY IDENTITY '
	IF @parentTableName = ''
		SET @sql = @sql + ')
			PRINT ''Created table: ' + @tableName + '''
		END'
	ELSE
		SET @sql = @sql + ', [' + @parentTableName + 'Id] INT NOT NULL,
			CONSTRAINT [FK_' + @tableName + '_To_' + @parentTableName + '] FOREIGN KEY ([' + @parentTableName + 'Id]) REFERENCES ['  + @parentTableName + ']([Id]) ON DELETE CASCADE)
			PRINT ''Created table: ' + @tableName + '''
		END'
	PRINT @sql
	EXEC sp_executesql @sql