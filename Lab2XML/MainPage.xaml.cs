using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Maui.Controls;
using Lab2XML.Strategies;
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
                    ParseXmlAndFillPickers(xmlContent);

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

        private void OnSearchClicked(object sender, EventArgs e)
        {
            // Логіка для пошуку
        }

        private void OnTransformToHtmlClicked(object sender, EventArgs e)
        {
            // Логіка для трансформації в HTML
        }
        private void OnClearClicked(object sender, EventArgs e)
        {
            // Логіка для очищення
            FacultyPicker.SelectedIndex = -1;
            DepartmentPicker.SelectedIndex = -1;
            DisciplinePicker.SelectedIndex = -1;
            NamePicker.SelectedIndex = -1;
            DomOption.IsChecked = false;
            SaxOption.IsChecked = false;
            LinqOption.IsChecked = false;
        }
        

    }

}
