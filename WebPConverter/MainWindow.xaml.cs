using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebPConverter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Класс конвертации
        /// </summary>
        private Convert converter;

        /// <summary>
        /// Конструктор окна
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// Инициализатор окна
        /// </summary>
        private void Init()
        {
            InitDefaultScanPath();
            //Инициализируем используемые классы
            converter = new Convert();
            //Добавляем обработчик события завершения конвертации
            converter.ConvertComplete += Converter_ConvertComplete;
            //Добавляем обработчик события обновленяи процесса конвертации
            converter.ConvertProgress += Converter_ConvertProgress;
            //Добавляем обработчик события нажатия на кнопку запуска конвертации
            StartButton.Click += StartButton_Click;
            //Добавляем обработчик события нажатия на кнопку выбора пути конвертации
            ConvertPathBrowse.Click += ConvertPathBrowse_Click;
        }

        /// <summary>
        /// Проставляем дефолтный путь сохранения
        /// </summary>
        private void InitDefaultScanPath()
        {
            //Получаем путь к папке рабочего стола
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            //Прописываем его в текстовое поле
            ConvertPath.Text = $"{path}\\";
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку выбора пути конвертации
        /// </summary>
        private void ConvertPathBrowse_Click(object sender, RoutedEventArgs e)
        {
            //Инициализируем диалог работы с папками
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            //Если путь успешно установлен
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //Проставляем путь конвертации
                ConvertPath.Text = dialog.SelectedPath;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку запуска конвертации
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Блокируем форму
            this.IsEnabled = false;
            //Пытаемся запустить конвертацию, и если попытка была неудачной
            if (!converter.DecodeFiles(ConvertPath.Text, GetMoveStatus()))
            {
                //Разблокируем форму
                this.IsEnabled = true;
                //Выводим сообщение об ошибке
                MessageBox.Show("Попка для конвертации не найдена!", "Ошибка!");                
            }
        }

        /// <summary>
        /// Получаем статус чекбокса переноса файла
        /// </summary>
        /// <returns>TRue - нужно переносить</returns>
        private bool GetMoveStatus() =>
            //Если значение есть - возвращаем его
            //Если нет - false
            (MoveCheckBox.IsChecked.HasValue) ? MoveCheckBox.IsChecked.Value : false;

        /// <summary>
        /// Обработчик события обновленяи процесса конвертации
        /// </summary>
        /// <param name="convertedCount">Количество сконвертированных файлов</param>
        /// <param name="maxCount">Количество файлов для конвертации</param>
        private void Converter_ConvertProgress(int convertedCount, int maxCount)
        {
            //Вызываем это в UI потоке
            this.Dispatcher.Invoke(new Action(() => {
                //Обновляем текст прогресса
                ConvertProgressText.Text = $"{convertedCount} / {maxCount}";
                //Проставляем значения прогрессбару
                ConvertProgress.Maximum = maxCount;
                ConvertProgress.Value = convertedCount;
            }));
        }

        /// <summary>
        /// Обработчик события завершения конвертации
        /// </summary>
        private void Converter_ConvertComplete()
        {
            //Вызываем это в UI потоке
            this.Dispatcher.Invoke(new Action(() => {
                //Разблокируем форму
                this.IsEnabled = true;
                //Выводим сообщение
                MessageBox.Show("Конвертация завершена!");
                //Сбрасываем текст прогресса
                ConvertProgressText.Text = "";
            }));
        }
    }
}
