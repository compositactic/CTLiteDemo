-- CTLite.Data.MicrosoftSqlServer - Made in the USA - Indianapolis, IN  - Copyright (c) 2020 Matt J. Crouch

-- Permission is hereby granted, free of charge, to any person obtaining a copy of this software
-- and associated documentation files (the "Software"), to deal in the Software without restriction, 
-- including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
-- and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
-- subject to the following conditions:

-- The above copyright notice and this permission notice shall be included in all copies
-- or substantial portions of the Software.

-- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
-- LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN 
-- NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
-- WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
-- SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

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
