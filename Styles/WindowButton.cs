using System.Windows;
using System.Windows.Controls;

namespace ControlLauncher.Styles;

public class WindowButton : Button
{
	public enum ButtonTypes
	{
		Undefined = -1,
		Minimize = 0,
		Maximize = 1,
		Close = 2,
		Info = 3,
	}

	public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.RegisterAttached(
		"ButtonType",
		typeof(ButtonTypes),
		typeof(WindowButton),
		new FrameworkPropertyMetadata(ButtonTypes.Undefined, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsRender));

	public ButtonTypes ButtonType
	{
		get => (ButtonTypes)this.GetValue(ButtonTypeProperty);
		set => this.SetValue(ButtonTypeProperty, value);
	}
}
