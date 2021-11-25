using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using Microsoft.VisualBasic;

namespace HS_Save_Loader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SavesPath = $@"{Environment.GetEnvironmentVariable("APPDATA")}\com.innersloth.henry.HenryFlash";

        public MainWindow()
        {
            InitializeComponent();

            ToggleButtons(0);

            RefreshSaves();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            if (MessageBox.Show("Are you sure you want to replace your current save with this one?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string[] Saves = Directory.GetDirectories(SavesPath);

                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Path.Combine(SavesPath, "Local Store"))))
                {
                    Console.WriteLine(path);
                    File.Delete(path);
                }
                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex])))
                {
                    File.Copy(path, Path.Combine(SavesPath, "Local Store", Path.GetFileName(path)), true);
                }

                MessageBox.Show(Path.GetFileName(Saves[SavesList.SelectedIndex]) + " has been set as the current save!");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MakeBackup();

            SaveButton.IsEnabled = false;
            Thread.Sleep(1000);
            SaveButton.IsEnabled = true;

            RefreshSaves();
        }

        private void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);

            Process.Start(Path.Combine(SavesPath, "Local Store", Saves[SavesList.SelectedIndex]));
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://rungameid/1089980");
        }

        private void SavesList_SelectionChanged(object sender, EventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            ToggleButtons(1);

            string[] Saves = Directory.GetDirectories(SavesPath);

            if (!File.Exists(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt")))
            {
                File.CreateText(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt"));
            }
            else
            {
                DescriptionText.Text = File.ReadAllText(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt"));
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);

            var Save = Interaction.InputBox("Save File Name", "Save Game", GetTime(), 100, 100);
            if (Save == "") return;

            if (Directory.Exists(Path.Combine(SavesPath, Save)))
            {
                if (MessageBox.Show("A save by this name already exists. Do you want to overwrite it?", "Warning!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }

                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Save)))
                {
                    File.Delete(path);
                }

                Directory.Delete(Path.Combine(SavesPath, Save));

                Directory.Move(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]), Path.Combine(SavesPath, Save));
            }
            else
            {
                Directory.Move(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]), Path.Combine(SavesPath, Save));
            }

            ToggleButtons(2);

            RefreshSaves();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);

            if (MessageBox.Show("Are you sure you want to PERMANENTLY delete this save?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            MessageBox.Show(Path.GetFileName(Saves[SavesList.SelectedIndex]) + " has been deleted!");

            Directory.Delete(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]), true);

            ToggleButtons(2);

            RefreshSaves();

        }

        private void DescriptionText_TextChanged(object sender, EventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);
            File.WriteAllText(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt"), DescriptionText.Text);
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Currently unavailable :(");
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Currently unavailable :(");
        }

        private string GetTime()
        {
            Regex DateRegex = new Regex(@"\b[1-9][1-2]?\/[1-3]?[1-9]\/\d*\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Regex TimeRegex = new Regex(@"\b[0-2]?[0-9](:[0-5][0-9]){2}\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var DateString = DateTime.Now.ToString();

            var Date = DateRegex.Matches(DateString)[0].Groups[0].ToString();
            var Time = TimeRegex.Matches(DateString)[0].Groups[0].ToString();
            var ampm = DateString.Remove(0, DateString.Length - 2);

            return Date.Replace("/", "-") + "_" + Time.Replace(":", "-") + "_" + ampm;
        }


        private void MakeBackup()
        {
            var Save = Interaction.InputBox("Save File Name", "Save Game", GetTime(), 100, 100);
            if (Save == "") return;

            if (Directory.Exists(Path.Combine(SavesPath, Save)))
            {
                if (MessageBox.Show("A save by this name already exists. Do you want to overwrite it?", "Warning!", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }

                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Save)))
                {
                    File.Delete(path);
                }
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(SavesPath, Save));
            }

            foreach(string path in Directory.GetFiles(Path.Combine(SavesPath, "Local Store")))
            {
                File.Copy(path, Path.Combine(SavesPath, Save, Path.GetFileName(path)));
            }

            File.WriteAllText(Path.Combine(SavesPath, Save, "desc.txt"), "Description...");

            ToggleButtons(2);
        }

        private string ConvertTimestamp(string Timestamp)
        {
            Regex TimestampRegex = new Regex(@"\b[1-9][1-2]?-[1-3]?[1-9]-\d*_[0-2]?[0-9](-[0-5][0-9]){2}_(AM|PM)\b",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (TimestampRegex.Matches(Timestamp).Count == 0) return Timestamp;

            return Timestamp.Split('_')[0].Replace("-", "/") + " " + Timestamp.Split('_')[1].Replace("-", ":") + " " + Timestamp.Split('_')[2];
        }

        private void RefreshSaves()
        {
            SavesList.Items.Clear();

            string[] Saves = Directory.GetDirectories(SavesPath);
            for (int i = 0; i < Saves.Length; i++)
            {
                SavesList.Items.Add(ConvertTimestamp(Path.GetFileName(Saves[i])));
            }

            ToggleButtons(2);
        }

        private void ToggleButtons(int Reason)
        {
            if (Reason == 0) // Nothing selected
            {
                LoadButton.IsEnabled = false;
                RevealButton.IsEnabled = false;
                RenameButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                ExportButton.IsEnabled = false;
                DescriptionText.IsEnabled = false;
            }

            if (Reason == 1) // Something selected
            {
                LoadButton.IsEnabled = true;
                RevealButton.IsEnabled = true;
                RenameButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                ExportButton.IsEnabled = true;
                DescriptionText.IsEnabled = true;
            }
        }
    }
}
