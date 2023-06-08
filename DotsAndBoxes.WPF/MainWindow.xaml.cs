using ControlzEx.Theming;
using DotsAndBoxes.WPF.Dialogs;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DotsAndBoxes.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel VM;
        private StartupDialog m_Startup;
        public MainWindow()
        {
            InitializeComponent();
            Closing += __MainWindowClosing;
            VM = new MainWindowViewModel(GameCanvas);
            DataContext = VM;
            m_Startup = new StartupDialog(VM);
            m_Startup.StartRequested += __StartRequested;
            m_Startup.ShowWindow();
            Hide();
        }

        private void __StartRequested(object? sender, EventArgs e)
        {
            Show();
        }

        private void __MainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            m_Startup.ShowWindow();
        }
    }
}
