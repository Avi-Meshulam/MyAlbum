using MyAlbum.BL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MyAlbum
{
    public class VariableSizedWrapGridView : GridView
    {
        public string PlaceholderText { get; set; }
        public int MinColSpan { get; set; }
        public int MinRowSpan { get; set; }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            int colSpan = MinColSpan > 0 ? MinColSpan : 1;
            int rowSpan = MinRowSpan > 0 ? MinRowSpan : 1;

            PropertyInfo variableDisplayPropertyInfo =
                (item as IVariableDisplayProperty).VariableDisplayPropertyInfo;

            Type propType = variableDisplayPropertyInfo.PropertyType;
            object propValue = variableDisplayPropertyInfo.GetValue(item);

            if (propType == typeof(string))
            {
                string text = propValue?.ToString();
                if (string.IsNullOrEmpty(text))
                    text = PlaceholderText;

                if (!string.IsNullOrEmpty(text))
                {
                    var textBlock = new TextBlock { Text = text/*, TextWrapping = TextWrapping.Wrap*/ };
                    textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    textBlock.Arrange(new Rect(0, 0, textBlock.DesiredSize.Width, textBlock.DesiredSize.Height));

                    colSpan = MinColSpan + (int)textBlock.ActualWidth;
                    rowSpan = MinRowSpan + (int)textBlock.ActualHeight;
                }
            }

            try
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, colSpan);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, rowSpan);
            }
            catch
            {
                element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            finally
            {
                //element.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
                //element.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
                base.PrepareContainerForItemOverride(element, item);
            }
        }
    }
}
