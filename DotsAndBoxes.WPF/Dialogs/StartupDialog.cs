using DotsAndBoxes.WPF.Views;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Security.RightsManagement;
using System.Windows;

namespace DotsAndBoxes.WPF.Dialogs
{
    public class StartupDialog : MetroWindow
    {
        private StartupDialogView m_View;
        private MainWindowViewModel m_ViewModel;
        public event EventHandler StartRequested;
        public StartupDialog(MainWindowViewModel VM)
        {
            Closing += __OnClosing; ;
            m_ViewModel = VM;
            Width = 100;
            Height = 200;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            m_View = new StartupDialogView();
            m_View.DataContext = m_ViewModel;
            m_View.StartGameRequested += __StartGameRequested;
            Content = m_View;
        }

        private void __OnClosing(object? sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void __StartGameRequested(object? sender, EventArgs e)
        {
            if (sender != null && sender is StartupDialogView StartupView && __ValidateCanStartGame(StartupView))
            {
                m_ViewModel.DrawGameField();
                StartRequested?.Invoke(this, e);
                Hide();
            }
            else
                MessageBox.Show("Cant start new game, need a minimum of two players", "Cant start a game",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool __ValidateCanStartGame(StartupDialogView StartupView)
        {
            var CheckedPlayers = 0;
            if (m_ViewModel.IsRedPlayerChecked)
                CheckedPlayers++;
            if (m_ViewModel.IsGreenPlayerChecked)
                CheckedPlayers++;
            if (m_ViewModel.IsYellowPlayerChecked)
                CheckedPlayers++;
            if (m_ViewModel.IsBluePlayerChecked)
                CheckedPlayers++;
            if (CheckedPlayers >= 2)
            {
                return true;
            }
            return false;
        }

        public void ShowWindow()
        {
            Show();
        }
    }
}
