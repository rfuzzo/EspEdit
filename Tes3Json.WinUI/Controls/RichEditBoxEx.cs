using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Tes3Json.WinUI.Controls
{
    // https://stackoverflow.com/a/26549205
    public class RichEditBoxEx : RichEditBox
    {
        public static readonly DependencyProperty RtfTextProperty =
            DependencyProperty.Register(
            "RtfText", typeof(string), typeof(RichEditBoxEx),
            new PropertyMetadata(default(string), RtfTextPropertyChanged));

        private bool _lockChangeExecution;

        public RichEditBoxEx()
        {
            TextChanged += RichEditBoxExtended_TextChanged;
        }

        public string RtfText
        {
            get => (string)GetValue(RtfTextProperty);
            set => SetValue(RtfTextProperty, value);
        }

        private void RichEditBoxExtended_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!_lockChangeExecution)
            {
                _lockChangeExecution = true;
                Document.GetText(TextGetOptions.None, out string text);
                if (string.IsNullOrWhiteSpace(text))
                {
                    RtfText = "";
                }
                else
                {
                    Document.GetText(TextGetOptions.FormatRtf, out text);
                    RtfText = text;
                }
                _lockChangeExecution = false;
            }
        }

        private static void RtfTextPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            RichEditBoxEx rtb = dependencyObject as RichEditBoxEx;
            if (rtb == null)
            {
                return;
            }

            if (!rtb._lockChangeExecution)
            {
                rtb._lockChangeExecution = true;
                rtb.Document.SetText(TextSetOptions.FormatRtf, rtb.RtfText);
                rtb._lockChangeExecution = false;
            }
        }
    }
}
