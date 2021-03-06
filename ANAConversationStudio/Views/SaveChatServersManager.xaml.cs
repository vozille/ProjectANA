﻿using ANAConversationStudio.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ANAConversationStudio.Views
{
    public partial class SaveChatServersManager : Window
    {
        public SaveChatServersManager()
        {
            InitializeComponent();

            CollectionControl.ItemsSource = Utilities.Settings.SavedChatServerConnections.DeepCopy();
            CollectionControl.ItemsSourceType = typeof(ObservableCollection<ChatServerConnection>);
            CollectionControl.NewItemTypes = new List<Type>() { typeof(ChatServerConnection) };
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (HideConfirm) return;

            var op = MessageBox.Show("Save changes and exit?", "Hold on!", MessageBoxButton.YesNoCancel);
            if (op == MessageBoxResult.Yes)
            {
                Save();
                HideConfirm = true;
                if (MainWindow.Current != null)
                    MainWindow.Current.AskAlert = false;
                Application.Current.Shutdown();
            }
            else if (op == MessageBoxResult.No)
            {
                HideConfirm = true;
                if (MainWindow.Current != null)
                    MainWindow.Current.AskAlert = false;
                Application.Current.Shutdown();
            }
        }
        private bool HideConfirm = false;

        private bool Save()
        {
            var conns = CollectionControl.Items.Cast<ChatServerConnection>().ToList();

            if (conns.Any(x => !x.IsValid()))
            {
                MessageBox.Show("The following connections are invalid, please correct them: \r\n" + string.Join("\r\n", conns.Where(x => !x.IsValid())), "Oops!");
                return false;
            }
            Utilities.Settings.SavedChatServerConnections = conns;
            Utilities.Settings.Save(App.Cryptio);
            return true;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            HideConfirm = true;
            Application.Current.Shutdown();
        }

        private async void SaveAndConnectClick(object sender, RoutedEventArgs e)
        {
            var conn = (CollectionControl.SelectedItem as ChatServerConnection);
            if (conn != null)
            {
                try
                {
                    if (!conn.IsValid())
                    {
                        MessageBox.Show("The connection details are invalid. Please correct them and try again.", "Oops!");
                        return;
                    }
                    var done = await StudioContext.LoadFromChatServerConnectionAsync(conn);
                    if (!done) return;
                    if (Save())
                    {
                        HideConfirm = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    StudioContext.Current = null;
                    MessageBox.Show("Unable to connect to the chat server");
                }
            }
        }
    }
}

//var connected = MainWindow.Current.ViewModel.LoadNodes();
//                    if (connected)
//                    {
//                        MainWindow.Current.Status("Chat flow loaded");
