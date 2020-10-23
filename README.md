# VKFriendsGraph

Консольное приложение .NET Core 3

Параметры:

**--login --password** Логин/пароль. При отсутствии используется сохраненный токен
	
**--id** Id  пользователя, у которого измеряется множественность связей. По умолчанию пользователь с токеном
	
**--threshold** Порог, при котором возможно восстановить связь. По умолчанию 0.7, при значениях > 1 восстановления не происходит.
	
**--nocache** Не использовать кэш для построения графа

**--width** Ширина изображения. По умолчанию 4000
