﻿IF (db_id(N'BlogDb2') IS NULL) 
	CREATE DATABASE BlogDb2
ELSE
	BEGIN
		ALTER DATABASE BlogDb2 SET single_user WITH ROLLBACK IMMEDIATE DROP DATABASE BlogDb2
		CREATE DATABASE BlogDb2
	END
