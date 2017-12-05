using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MyAlbum.ContentDialogs
{
    public sealed partial class CustomContentDialog : ContentDialog
    {
        public new event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> PrimaryButtonClick;
        public new event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> SecondaryButtonClick;
        public event TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs> CancelButtonClick;

        public CustomContentDialogResult Result { get; private set; }

        public CustomContentDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        #region Content
        public new static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content",
                                        typeof(string),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(string.Empty));

        public new string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value ?? string.Empty); }
        }
        #endregion // Content

        #region Primary Button Text
        public new static readonly DependencyProperty PrimaryButtonTextProperty =
            DependencyProperty.Register("PrimaryButtonText",
                                        typeof(string),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public new string PrimaryButtonText
        {
            get
            {
                string text = (string)GetValue(PrimaryButtonTextProperty);
                return string.IsNullOrWhiteSpace(text) ? "Close" : text;
            }
            set { SetValue(PrimaryButtonTextProperty, value ?? string.Empty); }
        }
        #endregion // Primary Button Text

        #region Primary Button Command
        public new static readonly DependencyProperty PrimaryButtonCommandProperty =
            DependencyProperty.Register("PrimaryButtonCommand",
                                        typeof(ICommand),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public new ICommand PrimaryButtonCommand
        {
            get { return (ICommand)GetValue(PrimaryButtonCommandProperty); }
            set { SetValue(PrimaryButtonCommandProperty, value); }
        }
        #endregion // Primary Button Command

        #region Primary Button Command Parameter
        public new static readonly DependencyProperty PrimaryButtonCommandParameterProperty =
            DependencyProperty.Register("PrimaryButtonCommandParameter",
                                        typeof(object),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public new object PrimaryButtonCommandParameter
        {
            get { return GetValue(PrimaryButtonCommandParameterProperty); }
            set { SetValue(PrimaryButtonCommandParameterProperty, value); }
        }
        #endregion // Primary Button Command Parameter

        #region Secondary Button Text
        public new static readonly DependencyProperty SecondaryButtonTextProperty =
            DependencyProperty.Register("SecondaryButtonText",
                                        typeof(string),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(string.Empty));

        public new string SecondaryButtonText
        {
            get { return (string)GetValue(SecondaryButtonTextProperty); }
            set { SetValue(SecondaryButtonTextProperty, value ?? string.Empty); }
        }
        #endregion // Secondary Button Text

        #region Secondary Button Command
        public new static readonly DependencyProperty SecondaryButtonCommandProperty =
            DependencyProperty.Register("SecondaryButtonCommand",
                                        typeof(ICommand),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public new ICommand SecondaryButtonCommand
        {
            get { return (ICommand)GetValue(SecondaryButtonCommandProperty); }
            set { SetValue(SecondaryButtonCommandProperty, value); }
        }
        #endregion // Secondary Button Command

        #region Secondary Button Command Parameter
        public new static readonly DependencyProperty SecondaryButtonCommandParameterProperty =
            DependencyProperty.Register("SecondaryButtonCommandParameter",
                                        typeof(object),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public new object SecondaryButtonCommandParameter
        {
            get { return GetValue(SecondaryButtonCommandParameterProperty); }
            set { SetValue(SecondaryButtonCommandParameterProperty, value); }
        }
        #endregion // Secondary Button Command Parameter

        #region Cancel Button Text
        public static readonly DependencyProperty CancelButtonTextProperty =
            DependencyProperty.Register("CancelButtonText",
                                        typeof(string),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(string.Empty));

        public string CancelButtonText
        {
            get { return (string)GetValue(CancelButtonTextProperty); }
            set { SetValue(CancelButtonTextProperty, value ?? string.Empty); }
        }
        #endregion // Cancel Button Text

        #region Cancel Button Command
        public static readonly DependencyProperty CancelButtonCommandProperty =
            DependencyProperty.Register("CancelButtonCommand",
                                        typeof(ICommand),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public ICommand CancelButtonCommand
        {
            get { return (ICommand)GetValue(CancelButtonCommandProperty); }
            set { SetValue(CancelButtonCommandProperty, value); }
        }
        #endregion // Cancel Button Command

        #region Cancel Button Command Parameter
        public static readonly DependencyProperty CancelButtonCommandParameterProperty =
            DependencyProperty.Register("CancelButtonCommandParameter",
                                        typeof(object),
                                        typeof(CustomContentDialog),
                                        new PropertyMetadata(null));

        public object CancelButtonCommandParameter
        {
            get { return GetValue(CancelButtonCommandParameterProperty); }
            set { SetValue(CancelButtonCommandParameterProperty, value); }
        }
        #endregion // Cancel Button Command Parameter

        #region Events Handlers
        private void CustomContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            SetButtonsGrid();
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            PrimaryButtonCommand?.Execute(PrimaryButtonCommandParameter);
            PrimaryButtonClick?.DynamicInvoke();
            Result = CustomContentDialogResult.Primary;
            Hide();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            SecondaryButtonCommand?.Execute(SecondaryButtonCommandParameter);
            SecondaryButtonClick?.DynamicInvoke();
            Result = CustomContentDialogResult.Secondary;
            Hide();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButtonCommand?.Execute(CancelButtonCommandParameter);
            CancelButtonClick?.DynamicInvoke();
            Result = CustomContentDialogResult.Cancel;
            Hide();
        }
        #endregion // Events Handlers

        #region Helper Methods
        private void SetButtonsGrid()
        {
            if (string.IsNullOrEmpty(SecondaryButtonText))
            {
                ButtonsGrid.ColumnDefinitions.RemoveAt(1);
                ButtonsGrid.ColumnDefinitions.RemoveAt(1);
                SecondaryButton.Visibility = Visibility.Collapsed;
            }

            if (string.IsNullOrEmpty(CancelButtonText))
            {
                int columnsCount = ButtonsGrid.ColumnDefinitions.Count();
                ButtonsGrid.ColumnDefinitions.RemoveAt(columnsCount - 1);
                ButtonsGrid.ColumnDefinitions.RemoveAt(columnsCount - 2);
                CancelButton.Visibility = Visibility.Collapsed;
            }
        }
        #endregion // Helper Methods
    }
}
