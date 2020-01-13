using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ControlLauncher
{
	public class ControlButtonHelper : DependencyObject
	{
		public static readonly DependencyProperty InsetWidthProperty = DependencyProperty.RegisterAttached("InsetWidth", typeof(double), typeof(ControlButtonHelper), new PropertyMetadata(0d));

		public static void SetInsetWidth(DependencyObject target, double value)
		{
			target.SetValue(InsetWidthProperty, value);
		}

		public static double GetInsetWidth(DependencyObject target)
		{
			return (double)target.GetValue(InsetWidthProperty);
		}
	}
}