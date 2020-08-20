CREATE OR ALTER PROCEDURE dbo.CreateForeignKeyConstraint
	@tableName NVARCHAR(MAX),
	@parentTableName NVARCHAR(MAX),
	@keyColumnName NVARCHAR(MAX),
	@parentKeyColumnName NVARCHAR(MAX),
	@constraintName NVARCHAR(MAX) = null

AS
	DECLARE @sql NVARCHAR(MAX)
	DECLARE @indexName NVARCHAR(MAX)

	IF @constraintName IS NULL
		SET @constraintName = 'FK_' + @tableName + @keyColumnName + '_To_' + @parentTableName + @parentKeyColumnName


	SET @sql = 'IF OBJECT_ID(''' + @constraintName + ''') IS NULL AND EXISTS (SELECT * FROM sys.tables WHERE name = ''' + @tableName + ''')
		BEGIN
			ALTER TABLE "' + @tableName + '" 
			ADD CONSTRAINT [' + @constraintName + '] FOREIGN KEY ([' + @keyColumnName + ']) REFERENCES [' + @parentTableName + ']([' + @parentKeyColumnName + '])
			PRINT ''Foreign key constraint added: ' + @tableName + '.' + @constraintName + '''
		END'

	PRINT @sql
	EXEC sp_executesql @sql
