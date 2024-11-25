using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Controls;
using Lab2XML.Strategies;
using Lab2XML.Models;
using System.Net;
namespace Lab2XML
{
    public partial class MainPage : ContentPage
    {
        private string _loadedXmlContent;
        private IXmlParsingStrategy _currentParsingStrategy;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnFileSelectClicked(object sender, EventArgs e)
        {
            try
            {
                // Вибір файлу
                var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".xml" } },
                    { DevicePlatform.iOS, new[] { "public.xml" } },
                    { DevicePlatform.Android, new[] { "application/xml" } }
                });

                var options = new PickOptions
                {
                    PickerTitle = "Select an XML File",
                    FileTypes = customFileType
                };

                var result = await FilePicker.PickAsync(options);

                if (result != null)
                {
                    // Зчитування вмісту файлу
                    string xmlContent = await File.ReadAllTextAsync(result.FullPath);
                    _loadedXmlContent = xmlContent;

                    // Зміна статусу
                    FileStatusLabel.Text = $"Loaded File: {result.FileName}";

                    // Парсинг XML і заповнення списків
                    //ParseXmlAndFillPickers(xmlContent);

                    // Відображення повідомлення
                    await DisplayAlert("Success", $"File '{result.FileName}' was loaded successfully!", "OK");
                    OptionsStack.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Cancelled", "No file was selected.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private void ParseXmlAndFillPickers(string xmlContent)
        {
            try
            {
                if (_currentParsingStrategy == null)
                {
                    DisplayAlert("Error", "Please select a parsing method first.", "OK");
                    return;
                }

                _currentParsingStrategy.ParseXml(xmlContent, (faculties, departments, disciplines, names) =>
                {
                    UpdatePicker(FacultyPicker, faculties);
                    UpdatePicker(DepartmentPicker, departments);
                    UpdatePicker(DisciplinePicker, disciplines);
                    UpdatePicker(NamePicker, names);
                });
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error parsing XML: {ex.Message}", "OK");
            }
        }



        private void OnDomOptionChecked(object sender, EventArgs e)
        {
            _currentParsingStrategy = new DomParsingStrategy();

            // Якщо файл вже завантажено, викликати парсинг з новою стратегією
            if (!string.IsNullOrEmpty(_loadedXmlContent))
            {
                ParseXmlAndFillPickers(_loadedXmlContent);
            }
        }

        private void OnSaxOptionChecked(object sender, EventArgs e)
        {
            _currentParsingStrategy = new SaxParsingStrategy();

            // Якщо файл вже завантажено, викликати парсинг з новою стратегією
            if (!string.IsNullOrEmpty(_loadedXmlContent))
            {
                ParseXmlAndFillPickers(_loadedXmlContent);
            }
        }

        private void OnLinqOptionChecked(object sender, EventArgs e)
        {
            _currentParsingStrategy = new LinqToXmlParsingStrategy();

            // Якщо файл вже завантажено, викликати парсинг з новою стратегією
            if (!string.IsNullOrEmpty(_loadedXmlContent))
            {
                ParseXmlAndFillPickers(_loadedXmlContent);
            }
        }


        private void UpdatePicker(Picker picker, List<string> items)
        {
            // Очищення списку
            picker.ItemsSource = null;
            picker.ItemsSource = items;

            if (items.Any())
            {
                picker.SelectedIndex = 0; // Вибір першого елемента за замовчуванням
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_loadedXmlContent))
                {
                    DisplayAlert("Error", "Please load an XML file first.", "OK");
                    return;
                }

                string selectedFaculty = FacultyPicker.SelectedItem?.ToString();
                string selectedDepartment = DepartmentPicker.SelectedItem?.ToString();
                string selectedDiscipline = DisciplinePicker.SelectedItem?.ToString();
                string selectedName = NamePicker.SelectedItem?.ToString();

                var result = _currentParsingStrategy.Search(_loadedXmlContent, selectedFaculty, selectedDepartment, selectedDiscipline, selectedName);

                if (result.Any())
                {
                    ResultsScrollView.IsVisible = true;
                    ResultsContainer.Children.Clear();

                    foreach (var student in result)
                    {
                        // Створення стилізованого блоку для кожного студента
                        var studentFrame = new Frame
                        {
                            Content = new VerticalStackLayout
                            {
                                Children =
                                {
                                    new Label
                                    {
                                        Text = $"Ім'я: {student.Name}",
                                        FontSize = 16,
                                        TextColor = Colors.Black,
                                        FontAttributes = FontAttributes.Bold
                                    },
                                    new Label
                                    {
                                        Text = $"Факультет: {student.Faculty}",
                                        FontSize = 14,
                                        TextColor = Colors.Black
                                    },
                                    new Label
                                    {
                                        Text = $"Кафедра: {student.Department}",
                                        FontSize = 14,
                                        TextColor = Colors.Black
                                    },
                                    new Label
                                    {
                                        Text = "Оцінки:",
                                        FontSize = 14,
                                        TextColor = Colors.Black,
                                        FontAttributes = FontAttributes.Bold
                                    },
                                    new Label
                                    {
                                        Text = FormatDisciplines(student.Disciplines, student.Grades),
                                        FontSize = 14,
                                        TextColor = Colors.Black
                                    }
                                }
                            }
                        };

                        ResultsContainer.Children.Add(studentFrame);
                    }
                }
                else
                {
                    ResultsScrollView.IsVisible = false;
                    await DisplayAlert("No Results", "No matching records found.", "OK");
                }


            }
            catch (Exception ex)
            {
                DisplayAlert("Error", $"Error during search: {ex.Message}", "OK");
            }
        }

        private string FormatDisciplines(List<string> disciplines, List<string> grades)
        {
            // Об'єднуємо дисципліни з оцінками у формат "Дисципліна: Оцінка"
            var formatted = disciplines.Zip(grades, (d, g) => $"{d}: {g}");
            return string.Join("\n", formatted); // Рядки відображаються через новий рядок
        }


        private string GenerateHtml(List<StudentResult> students)
        {
            var html = @"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Filtered Students</title>
        <style>
            table {
                width: 100%;
                border-collapse: collapse;
            }
            th, td {
                border: 1px solid black;
                padding: 8px;
                text-align: left;
            }
            th {
                background-color: #f2f2f2;
            }
        </style>
    </head>
    <body>
        <h1>Filtered Students</h1>
        <table>
            <tr>
                <th>Ім'я</th>
                <th>Факультет</th>
                <th>Кафедра</th>
                <th>Дисципліни</th>
            </tr>";

            foreach (var student in students)
            {
                var disciplines = string.Join("<br>", student.Disciplines.Zip(student.Grades, (d, g) => $"{d}: {g}"));
                html += $@"
            <tr>
                <td>{student.Name}</td>
                <td>{student.Faculty}</td>
                <td>{student.Department}</td>
                <td>{disciplines}</td>
            </tr>";
            }

            html += @"
        </table>
    </body>
    </html>";

            return html;
        }


        private async Task SaveHtmlToFileAsync(string htmlContent, string fileName)
        {
            var filePath = Path.Combine("D:\\Ann\\k24\\oop\\Lab2\\", fileName);

            using (var writer = new StreamWriter(filePath))
            {
                await writer.WriteAsync(htmlContent);
            }

            await DisplayAlert("HTML Saved", $"HTML file saved to: {filePath}", "OK");
        }

        private async void OnTransformToHtmlClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_loadedXmlContent))
                {
                    await DisplayAlert("Error", "Please load an XML file first.", "OK");
                    return;
                }

                // Приклад: результат після пошуку
                var selectedFaculty = FacultyPicker.SelectedItem?.ToString();
                var selectedDepartment = DepartmentPicker.SelectedItem?.ToString();
                var selectedDiscipline = DisciplinePicker.SelectedItem?.ToString();
                var selectedName = NamePicker.SelectedItem?.ToString();

                var result = _currentParsingStrategy.Search(_loadedXmlContent, selectedFaculty, selectedDepartment, selectedDiscipline, selectedName);

                if (result.Any())
                {
                    var htmlContent = GenerateHtml(result);
                    await SaveHtmlToFileAsync(htmlContent, "FilteredStudents.html");
                }
                else
                {
                    await DisplayAlert("No Results", "No matching records found to convert to HTML.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }



        private void OnClearClicked(object sender, EventArgs e)
        {
            FacultyPicker.SelectedIndex = -1;
            DepartmentPicker.SelectedIndex = -1;
            DisciplinePicker.SelectedIndex = -1;
            NamePicker.SelectedIndex = -1;

            /*DomOption.IsChecked = false;
            SaxOption.IsChecked = false;
            LinqOption.IsChecked = false;*/

            ResultsScrollView.IsVisible = false;
            ResultsContainer.Children.Clear();

            //FileStatusLabel.Text = "No file loaded";
        }

        private async void OnInfoClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Information", "This is an XML Analyzer tool for parsing and transforming XML files.\nUse Load XML File to start. \nChoose parsing method\nUse filters\nGet your HTML file\nAnna Olkhovska K24","OK");
        }

        private async void OnExitClicked(object sender, EventArgs e)
        {
            var saveConfirmed = await DisplayAlert("Exit", "Do you want to save your file as HTML file?", "Yes", "No");
            if(saveConfirmed) 
            {
                OnTransformToHtmlClicked(sender, e);
                
            }
            var exitConfirmed = await DisplayAlert("Exit", "Do you really want to exit the application?", "Yes", "No");
            if (exitConfirmed)
            {
                Application.Current.Quit();
            }

        }
    }

}
