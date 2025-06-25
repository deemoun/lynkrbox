using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace LinkSaver
{
    public partial class MainWindow : Window
    {
        private List<LinkItem> links = new();

        public MainWindow()
        {
            InitializeComponent();
            links = LinkStorage.Load();
            LinkList.ItemsSource = links;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(UrlBox.Text))
            {
                links.Add(new LinkItem { Url = UrlBox.Text.Trim() });
                RefreshList();
                LinkStorage.Save(links);
                UrlBox.Clear();
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