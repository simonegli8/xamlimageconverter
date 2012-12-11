using System.Windows;

namespace NorthHorizon.Samples.SystemThemeChange
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnApplyButtonClicked(object sender, RoutedEventArgs e)
		{
			ThemeHelper.SetTheme(ThemeNameTextBox.Text, ThemeColorTextBox.Text);
		}

		private void OnResetButtonClicked(object sender, RoutedEventArgs e)
		{
			ThemeHelper.Reset();
		}
	}
}
