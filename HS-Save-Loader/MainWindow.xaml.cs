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
        public string SavesPath = $@"{Environment.GetEnvironmentVariable("APPDATA")}\com.innersloth.henry.HenryFlash"; // Gets the path to the save folder

        public MainWindow()
        {
            InitializeComponent();

            ToggleButtons(0); // Disables buttons depending on stuff being selected (i.e. load, rename, delete)

            RefreshSaves(); // Gets all the saves and displays them
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return; // If nothing is selected, don't continue

            if (MessageBox.Show("Are you sure you want to replace your current save with this one?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string[] Saves = Directory.GetDirectories(SavesPath); // Get all saves in the save folder (because we have the index of the selected save, not the contents)

                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Path.Combine(SavesPath, "Local Store"))))
                {
                    File.Delete(path); // Deletes all items in the main save folder (the one used by the game)
                }
                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]))) // Gets item [index] of all the stuff in the save folder
                {
                    File.Copy(path, Path.Combine(SavesPath, "Local Store", Path.GetFileName(path)), true); // Copies the selected save to the main save folder (the one used by the game)
                }

                MessageBox.Show(Path.GetFileName(Saves[SavesList.SelectedIndex]) + " has been set as the current save!");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MakeBackup();

            SaveButton.IsEnabled = false; // Temporarily disables the save button so it looks like something's happening, rather than your computer doing it instantly
            Thread.Sleep(1000);
            SaveButton.IsEnabled = true;

            RefreshSaves();
        }

        private void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);

            Process.Start(Path.Combine(SavesPath, "Local Store", Saves[SavesList.SelectedIndex])); // Opens the selected save's folder
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshSaves();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("steam://rungameid/1089980"); // Starts the game (provided it's installed from Steam; I do not endorse piracy)
        }

        private void SavesList_SelectionChanged(object sender, EventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            ToggleButtons(1); // Enables buttons depending on stuff being selected (i.e. load, rename, delete) 

            string[] Saves = Directory.GetDirectories(SavesPath);

            if (!File.Exists(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt")))
            {
                DescriptionText.Text = "";
                File.WriteAllText(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt"), ""); // If the description file doesn't exist, create it (and leave the field blank because it would be anyways)
            }
            else
            {
                DescriptionText.Text = File.ReadAllText(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt")); // If the description file exists, display its contents
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);

            var Save = Interaction.InputBox("Save File Name", "Save Game", GetTime(), 100, 100); // Makes a prompt for the name of the new backup
            if (Save == "") return;

            if (Directory.Exists(Path.Combine(SavesPath, Save))) // If a backup by this name exists...
            {
                if (MessageBox.Show("A save by this name already exists. Do you want to overwrite it?", "Warning!", MessageBoxButton.YesNo) == MessageBoxResult.No) // Prompt whether to overwrite it
                {
                    return;
                }

                foreach (string path in Directory.GetFiles(Path.Combine(SavesPath, Save))) // If yes, loop through the folder and delete all files, then continue to what would've happened if the backup didn't exist
                {
                    File.Delete(path);
                }

                Directory.Delete(Path.Combine(SavesPath, Save)); // Delete the folder if it exists

                Directory.Move(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]), Path.Combine(SavesPath, Save)); // "Move" (rename) the selected backup to the new name
            }
            else
            {
                Directory.Move(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]), Path.Combine(SavesPath, Save)); // "Move" (rename) the selected backup to the new name
            }

            ToggleButtons(0); // Disables buttons depending on stuff being selected (i.e. load, rename, delete) 

            RefreshSaves();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);

            if (MessageBox.Show("Are you sure you want to PERMANENTLY delete this save?", "Are you sure?", MessageBoxButton.YesNo) == MessageBoxResult.No) // Confirm deletion of the backup
            {
                return;
            }

            MessageBox.Show(Path.GetFileName(Saves[SavesList.SelectedIndex]) + " has been deleted!");

            Directory.Delete(Path.Combine(SavesPath, Saves[SavesList.SelectedIndex]), true); // Delete the backup

            ToggleButtons(0); // Disables buttons depending on stuff being selected (i.e. load, rename, delete) 

            RefreshSaves();

        }

        private void DescriptionText_TextChanged(object sender, EventArgs e)
        {
            if (SavesList.SelectedIndex == -1) return;

            string[] Saves = Directory.GetDirectories(SavesPath);
            File.WriteAllText(Path.Combine(Saves[SavesList.SelectedIndex], "desc.txt"), DescriptionText.Text); // Write the updated description to file (backup directory > desc.txt)
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Currently unavailable :("); // Sadness
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Currently unavailable :("); // Depression
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

            ToggleButtons(0); // Disables buttons depending on stuff being selected (i.e. load, rename, delete) 
        }

        private string ConvertTimestamp(string Timestamp) // Boring code stuff I don't know how to explain. tl;dr, converts a computer timestamp into a human readable one
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
            for (int i = 0; i < Saves.Length; i++) // Loop through all the saves
            {
                if (!File.Exists(Path.Combine(Saves[i], "ver.txt"))) // Creates a version file if it doesn't already exist. This is for if/when I save the backups as archives for importing/exporting
                {
                    File.WriteAllText(Path.Combine(Saves[i], "ver.txt"), "2.0.0");
                }

                if (Path.GetFileName(Saves[i]) != "Local Store") // Hide the main save to avoid tampering
                {
                    SavesList.Items.Add(ConvertTimestamp(Path.GetFileName(Saves[i]))); // Displays the folder name on the list. If the folder name is the default [timestamp], it will be converted into a human readable string
                }
            }

            ToggleButtons(0); // Disables buttons depending on stuff being selected (i.e. load, rename, delete) 
        }

        private void ToggleButtons(int Reason) // Self explanatory (I hope)
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
