using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Курсовая
{
    public partial class Form1 : Form
    {
        // Поля класса для хранения данных 
        private double[] x;
        private double[] y;
        private string currentFilePath = "";
        int a = 5;
        public Form1()
        {
            InitializeComponent();

            // Заполняем comboBox1 при запуске
            if (comboBox1 != null)
            {
                comboBox1.Items.AddRange(new object[] { "0.01", "0.02", "0.05", "0.10" });
                comboBox1.SelectedIndex = 2; // По умолчанию 0.05
            }
            if (comboBox2 != null)
            {
                comboBox2.Items.AddRange(new object[] { "0.10", "0.05", "0.02", "0.01" });
                comboBox2.SelectedIndex = 1; // По умолчанию 0.05
            }
        }


        //Метод загрузки данных из файла
        void LoadDataFromFile(string filePath, out double[] x, out double[] y)
        {
            string[] lines = File.ReadAllLines(filePath);

            //Пропускаем пустые строки
            var validLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            x = new double[validLines.Count];
            y = new double[validLines.Count];
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            for (int i = 0; i < validLines.Count; i++)
            {
                //Заменяем запятую на точку, если есть
                string line = validLines[i].Replace(',', '.');

                string[] values = line.Split(new char[] { ' ', '\t' },
                    StringSplitOptions.RemoveEmptyEntries);

                if (values.Length < 2)
                    throw new Exception($"В строке {i + 1} недостаточно данных");

                //Безопасное преобразование
                if (!double.TryParse(values[0],
                    System.Globalization.NumberStyles.Any,
                    culture, out double xVal))
                {
                    throw new Exception($"В строке {i + 1} значение '{values[0]}' не является числом");
                }

                if (!double.TryParse(values[1],
                    System.Globalization.NumberStyles.Any,
                    culture, out double yVal))
                {
                    throw new Exception($"В строке {i + 1} значение '{values[1]}' не является числом");
                }

                x[i] = xVal;
                y[i] = yVal;
            }
        }

        //Метод вычисления дисперсии 
        double CalculateVariance(double[] data)
        {
            if (data == null || data.Length == 0) return 0;
            double mean = data.Average();
            return data.Select(val => Math.Pow(val - mean, 2)).Average();
        }

        //Метод вычисления T
        double CalculateT(double[] x, double[] y, double Dx, double Dy)
        {
            int n1 = x.Length;
            int n2 = y.Length;
            double diffMeans = Math.Abs(x.Average() - y.Average());
            double denominator = Math.Sqrt(n1 * Dx + n2 * Dy);
            double factor = Math.Sqrt((double)(n1 * n2) / (n1 + n2) * (n1 + n2 - 2));
            return (diffMeans / denominator) * factor;
        }

       

        //Кноака загрузки файла
        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите файл с данными";
                openFileDialog.Filter = "Текстовые файлы|*.txt|Все файлы|*.*";
                openFileDialog.InitialDirectory = Application.StartupPath;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        currentFilePath = openFileDialog.FileName;
                        LoadDataFromFile(currentFilePath, out x, out y);
                        int n1 = x.Length;
                        int n2 = y.Length;
                        int k = n1 + n2 - 2;
                        textBox8.Text = k.ToString();
                        int U = 0;
                        double N = y.Length;
                        textBox9.Text = N.ToString();
                        //Расчёт количества инверсий
                        for (int i = 0; i < N; i++)
                        {
                            for (int j = 0; j < N; j++)
                            {
                                if (y[j] <= x[i])
                                {
                                    U++;
                                }
                            }
                        }
                        textBox1.Text = U.ToString();
                        //Расчёт математического ожидания
                        double M = (N * N) / 2.0;
                        textBox5.Text = M.ToString();
                        // Показываем загруженные данные
                        listBox4.Items.Clear();
                        listBox4.Items.Add($"Файл загружен: {currentFilePath}");
                        listBox4.Items.Add($"Загружено {x.Length} пар значений");
                        listBox4.Items.Add($"X: {string.Join(", ", x)}");
                        listBox4.Items.Add($"Y: {string.Join(", ", y)}");
                        MessageBox.Show($"Данные успешно загружены!\n\n" +
                            $"Массив X: {string.Join(", ", x)}\n" +
                            $"Массив Y: {string.Join(", ", y)}",
                            "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Кнопка оценки адекватности
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // ПРОВЕРЯЕМ: загружены ли данные в поля x и y
                if (x == null || y == null || x.Length == 0)
                {
                    MessageBox.Show("Нет загруженных данных!\n\nСначала нажмите 'Загрузить файл' и выберите файл с данными.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ПРОВЕРЯЕМ: введено ли критическое значение 
                if (string.IsNullOrWhiteSpace(textBox6.Text))
                {
                    MessageBox.Show("Введите критическое значение t_кр в поле!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Получаем уровень значимости
                double alpha = 0.05;
                if (comboBox1.SelectedItem != null)
                {
                    double.TryParse(comboBox1.SelectedItem.ToString(), out alpha);
                }
                // Получаем критическое значение
                double t_crit;
                if (!double.TryParse(textBox6.Text,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out t_crit))
                {
                    MessageBox.Show("Некорректное значение t_кр! Введите число (например: 2.306)",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Основные расчёты 
                int n1 = x.Length;
                int n2 = y.Length;
                int k = n1 + n2 - 2;

                double meanX = x.Average();
                double meanY = y.Average();
                double Dx = CalculateVariance(x);
                double Dy = CalculateVariance(y);
                double t_emp = CalculateT(x, y, Dx, Dy);

                // Выводим значение T эмп
                if (textBox4 != null)
                {
                    textBox4.Text = t_emp.ToString();
                }
                textBox8.Text = k.ToString();
                // Оценка адекватности модели
                string adequacyResult;

                if (t_emp >= t_crit)
                {
                    adequacyResult = "МОДЕЛЬ НЕ АДЕКВАТНА";

                }
                else
                {
                    adequacyResult = "МОДЕЛЬ АДЕКВАТНА";

                }
                // Вывод результата оценки
                listBox1.Items.Clear();
                listBox1.Items.Add($"{adequacyResult}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчёте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (textBox4 != null)
                    textBox4.Text = "Ошибка";

                listBox1.Items.Clear();
                listBox1.Items.Add($"ОШИБКА: {ex.Message}");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (x != null && y != null && x.Length > 0)
            {
                button1_Click(sender, e);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Visible = radioButton1.Checked;
            if (radioButton1.Checked)
            {
                panel1.Visible = false;
                panel3.Visible = false;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = radioButton2.Checked;
            if (radioButton2.Checked)
            {
                panel2.Visible = false;
                panel3.Visible = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            panel3.Visible = radioButton3.Checked;
            if (radioButton3.Checked)
            {
                panel1.Visible = false;
                panel2.Visible = false;
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (x != null && y != null && x.Length > 0)
            {
                button2_Click(sender, e);
            }
        }
        private void label1_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void label15_Click(object sender, EventArgs e) { }
        private void label17_Click(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }
        private void listBox4_SelectedIndexChanged(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void panel3_Paint(object sender, PaintEventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }

        //Кнопка оценки устойчивости
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка наличия данных
                if (x == null || y == null || x.Length == 0)
                {
                    MessageBox.Show("Нет загруженных данных!\n\nСначала нажмите 'Загрузить файл'.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Проверка ввода критического значения
                if (string.IsNullOrWhiteSpace(textBox7.Text))
                {
                    MessageBox.Show("Введите критическое значение Uкр в поле!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int U = 0;
                double N = y.Length;

                // Циклы для подсчёта инверсий
                for (int i = 0; i < N; i++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (y[j] <= x[i])
                        {
                            U++;
                        }
                    }
                }

                // Расчёт математического ожидания
                double M = (N * N) / 2.0;
                double diff = Math.Abs(U - M);         
                // Получение критического значения 
                double Ucrit = double.Parse(textBox7.Text,
                    System.Globalization.CultureInfo.InvariantCulture);
                // Сравнение и вывод результата
                string stabilityResult;
                if (diff >= Ucrit)
                {
                    stabilityResult = "МОДЕЛЬ НЕ УСТОЙЧИВА";
                }
                else
                {
                    stabilityResult = "МОДЕЛЬ УСТОЙЧИВА";
                }

                // Вывод результата
                if (listBox1 != null)
                {
                    listBox2.Items.Clear();
                    
                    listBox2.Items.Add(stabilityResult);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчёте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //Кнопка оценки чувствительности
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка наличия данных
                if (x == null || y == null || x.Length == 0 || y.Length == 0)
                {
                    MessageBox.Show("Нет загруженных данных!\n\nСначала нажмите 'Загрузить файл'.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Находим максимальное и минимальное значения X
                double Xmax = x.Max();
                double Xmin = x.Min();

                // Находим соответствующие значения Y при Xmax и Xmin
                int indexMax = Array.IndexOf(x, Xmax);
                int indexMin = Array.IndexOf(x, Xmin);

                double Y1 = y[indexMax]; // Y при Xmax
                double Y2 = y[indexMin]; // Y при Xmin

                //Вычисляем ΔX
                double deltaX = ((Xmax - Xmin) * 2.0) / (Xmax + Xmin);

                //Вычисляем ΔY
                double deltaY = (Math.Abs(Y1 - Y2) * 2.0) / (Y1 + Y2);

                //Вычисляем чувствительность 
                double S = deltaY / deltaX;
                double absS = Math.Abs(S);
                //Оценка чувствительности по масштабу
                string sensitivityLevel;

                if (absS > 1)
                {
                    sensitivityLevel = "ВЫСОКАЯ ЧУВСТВИТЕЛЬНОСТ";
                }
                else if (absS < 0.1)
                {
                    sensitivityLevel = "НИЗКАЯ ЧУВСТВИТЕЛЬНОСТЬ";
                }
                else
                {
                    sensitivityLevel = "СРЕДНЯЯ ЧУВСТВИТЕЛЬНОСТЬ";
                }

                // Вывод результата
                listBox3.Items.Clear();
                listBox3.Items.Add(sensitivityLevel);

                // Вывод значенй 
                textBox2.Text = deltaX.ToString("F2");
                textBox3.Text = deltaY.ToString("F2");
              

    
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчёте: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
 }