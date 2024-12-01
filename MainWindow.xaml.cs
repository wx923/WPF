using System.Windows;
using WpfApp.ViewModels;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 