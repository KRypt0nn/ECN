# ECN
**ECN** - Enfesto Community Network, платформа для реализации системы локальной авторизации пользователей. Представлен в виде **C#** библиотеки

Основной проект *(сама библиотека)* находится в папке **ECN Library**. Там же по пути **ECN Library/bin/Release** *(или **ECN Library/bin/Debug**)* вы можете найти собранный вариант библиотеки

## Работа с библиотекой
Весь функционал библиотеки расположен в пространстве имён **ECN**. В настоящий момент в библиотеке имеется **2** объекта:

1. **ECNUser** - основной объект, который предоставляет доступ к авторизационной информации
2. **ECNListener** - объект, предоставляющий функционал по мониторингу носителей информации

### ECNUser

Метод | Аргументы | Вывод | Описание
------|-----------|-------|---------
***[конструктор]*** | | | Создание объекта информации
| | string **dataPath**
**SetData** | string **data**, [bool **verify** = **false**], [string **dataPath** = **""**] | | Установка информации в локальное хранилище

Пример использования:

```cs
using ECN;
using System;

ECNUser User = new ECNUser ();
Console.WriteLine ("Actual user ID: " + User.ID);
```

Пример регистрации пользователя:

```cs
using ECN;
using System;

Console.WriteLine ("Actual drives:");

foreach (DriveInfo Drive in DriveInfo.GetDrives ())
    Console.WriteLine ("- [" + Drive.RootDirectory.FullName + "]");

Console.Write ("\nEnter drive: ");
string DriveName = Console.ReadLine ();

Console.Write ("Enter data: ");
string Data = Console.ReadLine ();

ECNUser User = new ECNUser ();
User.SetData (Data, true, DriveName);
```

### ECNListener

Метод | Аргументы | Вывод | Описание
------|-----------|-------|---------
***[конструктор]*** | Action< ECNUser > **handler** | | Создание объекта мониторинга носителей информации
**SetUserLeaveHandler** | Action< ECNUser > **handler** | | Установка анонимной функции-обработчика информации о отключении авторизационных насителей информации
**Listen** | [bool **infinity** = **false**] | | Начать мониторинг носителей информации
**ThreadListen** | [bool **infinity** = **false**] | | Начать мониторинг носителей информации в потоке

Пример использования *(консоль с логом авторизации)*:

```cs
using ECN;
using System;

ECNListener Listener = new ECNListener ((user) => {
    Console.WriteLine ("Entered user '" + user.Name + "' with ID '" + user.ID + "' (" + (user.Verified ? "Verified" : "Not verified") + "). Data: " + user.Data);
});

Listener.SetUserLeaveHandler ((user) => {
    Console.WriteLine ("Leaved user '" + user.Name + "' with ID '" + user.ID + "' (" + (user.Verified ? "Verified" : "Not verified") + "). Data: " + user.Data);
});

Listener.ThreadListen (true);
```

Вот и всё. Приятного использования! :3

Автор: [Подвирный Никита](https://vk.com/technomindlp). Специально для [Enfesto Studio Group](http://vk.com/hphp_convertation)