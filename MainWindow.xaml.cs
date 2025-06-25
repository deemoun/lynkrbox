using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace LinkSaver
{
    public partial class MainWindow : Window
    {
        private List<LinkItem> links = new();
        private string? pendingUrlForDescription = null;

        public MainWindow()
        {
            InitializeComponent();
            links = LinkStorage.Load();
            LinkList.ItemsSource = links;
        }

        private bool IsValidUrl(string url)
        {
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string input = UrlBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!IsValidUrl(input))
            {
                MessageBox.Show("Only links starting with http:// or https:// are allowed.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            links.Add(new LinkItem { Url = input, Description = pendingUrlForDescription });
            pendingUrlForDescription = null;

            RefreshList();
            LinkStorage.Save(links);
            UrlBox.Clear();
        }

        private void Description_Click(object sender, RoutedEventArgs e)
        {
            string input = UrlBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Enter a URL first.");
                return;
            }

            var item = links.FirstOrDefault(l => l.Url == input);

            if (item != null)
            {
                // Уже добавленная ссылка — показать или изменить описание
                string existing = item.Description ?? "";
                string desc = Microsoft.VisualBasic.Interaction.InputBox("Description for this link:", "Edit Description", existing);

                if (!string.IsNullOrWhiteSpace(desc))
                {
                    item.Description = desc;
                    LinkStorage.Save(links);
                }
            }
            else
            {
                // Новый URL — сохранить описание, пока не нажата "+"
                string desc = Microsoft.VisualBasic.Interaction.InputBox("Description for this new link:", "New Description", "");
                if (!string.IsNullOrWhiteSpace(desc))
                    pendingUrlForDescription = desc;
            }
        }

        private void LinkList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LinkList.SelectedItem is LinkItem item)
                Process.Start(new ProcessStartInfo(item.Url) { UseShellExecute = true });
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is LinkItem item)
            {
                links.Remove(item);
                RefreshList();
                LinkStorage.Save(links);
            }
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                var json = File.ReadAllText(dialog.FileName);
                links = JsonSerializer.Deserialize<List<LinkItem>>(json) ?? new();
                RefreshList();
                LinkStorage.Save(links);
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files|*.json",
                FileName = "links.json"
            };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(links, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        private void RefreshList()
        {
            LinkList.ItemsSource = null;
            LinkList.ItemsSource = links;
        }
    }
}