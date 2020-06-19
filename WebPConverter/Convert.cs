using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Imazen.WebP;


namespace WebPConverter
{
    /// <summary>
    /// Класс конвертации
    /// </summary>
    internal class Convert : IDisposable
    {
        /// <summary>
        /// Делегат события обновление процесса конвертации
        /// </summary>
        /// <param name="convertedCount">Количество сконвертированных файлов</param>
        /// <param name="maxCount">Количество файлов для перевода</param>
        public delegate void ConvertProgressEventHandler(int convertedCount, int maxCount);
        /// <summary>
        /// Событие обновление процесса конвертации
        /// </summary>
        public event ConvertProgressEventHandler ConvertProgress;

        /// <summary>
        /// Делегат события завершения конвертации
        /// </summary>
        public delegate void ConvertCompleteEventHandler();
        /// <summary>
        /// Событие завершения конвертации
        /// </summary>
        public event ConvertCompleteEventHandler ConvertComplete;


        /// <summary>
        /// Путь сохранения
        /// </summary>
        private string savePath;
        /// <summary>
        /// Путь переноса
        /// </summary>
        private string movePath;

        /// <summary>
        /// Флаг работы программы
        /// </summary>
        private bool isWork;

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public Convert()
        {
            //Проставляем дефолтные значения
            isWork = true;
            //Формируем путь сохранения
            string path = $@"{Environment.CurrentDirectory}\Save\";
            //Путь для сконвертированных файлов
            savePath = $@"{path}Converted\";
            //Путь для перенесённых файлов
            movePath = $@"{path}Moved\";
            //Создаём папку сохранения, если её ещё нету
            Directory.CreateDirectory(savePath);
            //Создаём папку перемещения, если её ещё нету
            Directory.CreateDirectory(movePath);
        }

        /// <summary>
        /// Декодируем файлы сохранений
        /// </summary>
        /// <param name="moveOriginal">Флаг переноса оригинальных файлов</param>
        /// <param name="path">Путь для поиска webp файлов</param>
        /// <returns>True - запуск успешен. False - папка не найдена.</returns>
        public bool DecodeFiles(string path, bool moveOriginal)
        {
            bool ex = false;
            //Если папка действительно существует
            if (Directory.Exists(path))
            {
                //Папка успешно получена
                ex = true;
                //Запускаем работу в отдельном потоке
                new Thread(() =>
                {
                    //Обнуляем количество сконвертированных файлов
                    int converted = 1;
                    //Инициализируем информацию о директории
                    DirectoryInfo di = new DirectoryInfo(path);
                    //Получаем список *.webp файлов в директории
                    List<FileInfo> FileList = di.GetFiles("*.webp").ToList();
                    //Проходимся по всем файлам директории
                    foreach (var file in FileList)
                    {
                        //Выполняем операции над файлом
                        FileWork(file, moveOriginal);
                        //Если работа была отменена
                        if (!isWork)
                            //Выходим из цикла
                            break;
                        //Вызываем ивент обновления
                        ConvertProgress?.Invoke(converted++, FileList.Count);
                    }

                    //Если работа всё ещё идёт
                    if (isWork)
                    {
                        //Открываем папку
                        Process.Start(savePath);
                        //Вызываем ивент завершения конвертации
                        ConvertComplete?.Invoke();
                    }
                }).Start();
            }
            return ex;
        }

        /// <summary>
        /// Работа над файлом
        /// </summary>
        /// <param name="moveOriginal">Флаг переноса оригинальных файлов</param>
        /// <param name="fi">Информация о переносимом файле</param>
        private void FileWork(FileInfo fi, bool moveOriginal)
        {
            //Декодируем файл
            Decode(fi);
            //Если оригинал нужно перенести
            if (moveOriginal)
                //Переносим
                Move(fi);
        }

        /// <summary>
        /// Переносим файл
        /// </summary>
        /// <param name="fi">Информация о переносимом файле</param>
        private void Move(FileInfo fi)
        {
            //Переносим файл в папку переноса
            fi.MoveTo($"{movePath}{fi.Name}");
        }


        /// <summary>
        /// Декодируем WebP изображение
        /// </summary>
        /// <param name="fi">Информация о декодируемом файле</param>
        private void Decode(FileInfo fi)
        {
            //Инициализируем декодер
            SimpleDecoder decoder = new SimpleDecoder();
            
            //Считываем байты файла
            byte[] bytes = File.ReadAllBytes(fi.FullName);
            //Декодируем изображение
            using (Bitmap image = decoder.DecodeFromBytes(bytes, bytes.Length))
                //Сохраняем файл
                image.Save($"{savePath}{GenerateFileName(fi)}");
        }

        /// <summary>
        /// Генерируем новое имя файла
        /// </summary>
        /// <param name="fi">Информация о файле</param>
        /// <returns>Новое имя файла</returns>
        private string GenerateFileName(FileInfo fi) =>
            //Заменяем родное расширение на .png
            fi.Name.Replace(fi.Extension, ".png");

        /// <summary>
        /// Метод очистки неуправляемых ресурсов
        /// </summary>
        public void Dispose()
        {
            //Завершаем работу
            isWork = false;
        }
    }
}
