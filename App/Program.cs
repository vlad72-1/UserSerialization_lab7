/*
 *Лабораторная: 7.
 *Источник: https://hwmw.blogspot.com/p/labstimp2.html
 *
 *Язык: C Sharp (C#) v7.3.
 *Среда: Microsoft Visual Studio 2019 v16.9.1.
 *Платформа: .NET Framework v4.6.1.
 *API: Windows forms.
 *Изменение: 07.06.2021.
 *Защита: 08.06.2021.
 *
 *Вариант: БН.
 *Задание: пользовательская сериализация и десериализация. Разработайте .NET приложения обменивающиеся данными структур и классов
 *   используя пользовательскую бинарную, XML и JSON сериализацию.
 *
 *Примечание:
 *1. Принцип работы: запустить один экземпляр "App". Вначале выбрать "Путь для сохранения", затем нажать "Включить сервер" и выбрать
 *   файл для отправки.
 *2. Результат бинарной передачи просто выводится на экран без сохранения в файл.
 *3. Отличие базовой бинарной сериализации от пользовательской бинарной сериализации:
 *а) https://docs.microsoft.com/ru-ru/dotnet/standard/serialization/basic-serialization
 *б) https://docs.microsoft.com/ru-ru/dotnet/standard/serialization/custom-serialization
 */

using System;
using System.Windows.Forms;

namespace App
{

    static class Program
    {

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main ()
        {
            Application.EnableVisualStyles ();
            Application.SetCompatibleTextRenderingDefault (false);
            Application.Run (new Form1 ());
        }
    }
}
