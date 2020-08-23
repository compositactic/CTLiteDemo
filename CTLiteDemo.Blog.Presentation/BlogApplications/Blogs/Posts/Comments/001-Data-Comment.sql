INSERT INTO Comment (PostId, Text) VALUES ((SELECT ID FROM Post WHERE Title = 'Anim id est laborum'), 'feugiat euismod lacinia at')
INSERT INTO Comment (PostId, Text) VALUES ((SELECT ID FROM Post WHERE Title = 'Anim id est laborum'), 'quam elementum')
