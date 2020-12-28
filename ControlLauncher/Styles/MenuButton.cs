using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlLauncher.Styles {
	public class MenuButton : Button {
		public static readonly DependencyProperty InsetWidthProperty = DependencyProperty.RegisterAttached(
			"InsetWidth",
			typeof(double),
			typeof(MenuButton),
			new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

		public double InsetWidth {
			get => (double)GetValue(InsetWidthProperty);
			set => SetValue(InsetWidthProperty, value);
		}

		protected override void OnMouseEnter(MouseEventArgs e) {
			base.OnMouseEnter(e);
			this.Focus();
		}
	}
}