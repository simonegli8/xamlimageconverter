using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.IO;
using System.Windows.Markup;
using System.Diagnostics;
using Tomers.WPF.Themes.Skins;

namespace Tomers.WPF.Themes
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{		
		public MainWindow()
		{
			InitializeComponent();
			InitializeSkins();

			DataContext = _skins;
		}

		private void InitializeSkins()
		{
			_skins.Add(new DirectAssemblySkin("Simple", @"Skins\Tomers.WPF.Themes.SimpleSkin.dll"));
			_skins.Add(new AppDomainAssemblySkin("Simple", @"Skins\Tomers.WPF.Themes.SimpleSkin.dll"));
			_skins.Add(new LooseXamlSkin("Classic", new Uri(@"Skins\Classic.xaml", UriKind.Relative)));
			_skins.Add(new AppDomainAssemblySkin("Classic", new AssemblyName("PresentationFramework.Classic, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")));
			_skins.Add(new ReferencedAssemblySkin("Luna Normal", new Uri("PresentationFramework.Luna;V3.0.0.0;31bf3856ad364e35;component/themes/luna.normalcolor.xaml", UriKind.Relative)));
			_skins.Add(new AppDomainAssemblySkin("Aero", new AssemblyName("PresentationFramework.Aero, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")));
			_skins.Add(new AppDomainAssemblySkin("Luna Metalic", new AssemblyName("PresentationFramework.Luna, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), "themes/luna.metallic.xaml"));
			_skins.Add(new AppDomainAssemblySkin("Royale", new AssemblyName("PresentationFramework.Royale, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")));
		}

		public Skin CurrentSkin
		{
			get { return (Skin)GetValue(CurrentSkinProperty); }
			set { SetValue(CurrentSkinProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentSkin.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentSkinProperty =
			DependencyProperty.Register(
				"CurrentSkin",
				typeof(Skin),
				typeof(MainWindow),
				new UIPropertyMetadata(Skin.Null, OnCurrentSkinChanged, OnCoerceSkinValue));
		
		private static void OnCurrentSkinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			try
			{
				Skin oldSkin = e.OldValue as Skin;
				oldSkin.Unload();
				Skin newSkin = e.NewValue as Skin;

				int ticks = Environment.TickCount;

				newSkin.Load();

				ticks = Environment.TickCount - ticks;
				MessageBox.Show(string.Format("Time to load {0} skin of type {1}: {2}ms",
					newSkin.Name, newSkin.GetType().Name, ticks));
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Change error: " + ex.Message);
				throw;
			}
		}

		private static object OnCoerceSkinValue(DependencyObject d, object baseValue)
		{
			try
			{
				if (baseValue == null)
				{
					return Skin.Null;
				}
				return baseValue;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Coerce error: " + ex.Message);
				throw;
			}
		}		

		private void ButtonDirectAssembly_Click(object sender, RoutedEventArgs e)
		{
			Skin simpleSkin = new ReferencedAssemblySkin("Simple Skin", new Uri(@"Skins\Tomers.WPF.Themes.SimpleSkin.dll", UriKind.Relative));
			simpleSkin.Load();
		}

		private void ButtonAppDomainAssembly_Click(object sender, RoutedEventArgs e)
		{
			Skin simpleSkin = new AppDomainAssemblySkin("Simple Skin", @"Skins\Tomers.WPF.Themes.SimpleSkin.dll");
			simpleSkin.Load();
		}

		private readonly List<Skin> _skins = new List<Skin>();
	}
}
