using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ControlLauncher.Styles
{
	public class MenuButton : Button
	{
		public static readonly DependencyProperty InsetWidthProperty = DependencyProperty.RegisterAttached(
			"InsetWidth", 
			typeof(double), 
			typeof(MenuButton), 
			new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

		public double InsetWidth {
			get => (double)GetValue(InsetWidthProperty);
			set => SetValue(InsetWidthProperty, value);
		}
	}
}
