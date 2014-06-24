using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dynamo.Nodes;

namespace Dynamo.UI
{
    class CodeCompletionHandler
    {
        TextBox textBox;
        Popup popup;
        int prefixLength;

        public CodeCompletionHandler(TextBox tb)
        {
            Popup p = new Popup();

            //var list = new ListView();
            var list = new ListBox();
            list.KeyDown += list_KeyDown;

            //list.MouseLeftButtonDown += list_MouseLeftButtonDown;
            //list.PreviewMouseLeftButtonDown += list_PreviewMouseLeftButtonDown;
            list.IsTextSearchEnabled = true;
            //p.LostFocus += p_LostFocus;
            list.FontSize = tb.FontSize * 0.8;            
            
            p.Child = list;
            p.StaysOpen = false;
            p.Placement = PlacementMode.Bottom;
            p.PlacementTarget = tb;
            textBox = tb;
            
            popup = p;
        }

        internal bool IsKeyboardFocusWithin
        {
            get { return popup.IsKeyboardFocusWithin; }
        }

        //void list_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //}

        //void list_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var item = (sender as ListBox).SelectedItem as string;
        //    if (item != null)
        //    {
        //        this.textBox.Text = this.textBox.Text.Insert(textBox.SelectionStart, item);
        //    }
        //    ((sender as ListBox).Parent as Popup).IsOpen = false;
        //}

        //void p_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    (sender as Popup).IsOpen = false;
        //}

       
        public void UpdateCompletionList(int prefixLength, Rect placementRect, List<string> items)
        {
            popup.PlacementRectangle = placementRect;
            ListBox list = popup.Child as ListBox;
            //list.Focus();
            list.Items.Clear();
            items.ForEach(x => list.Items.Add(x));
            list.SelectedIndex = 0;

            if (popup.IsOpen == false)
            {
                popup.IsOpen = true;
                // list.Focus();
            }

            this.prefixLength = prefixLength;
        }

        void list_KeyDown(object sender, KeyEventArgs e)
        {
            /*var item = (sender as ListBox).SelectedItem as string;
            if (item != null)
            {
                this.textBox.Text = this.textBox.Text.Insert(textBox.SelectionStart, item);
                //this.textBox.Text = this.textBox.Text.Insert(textBox.CaretIndex, item);
            }
            ((sender as ListBox).Parent as Popup).IsOpen = false;*/
            Popup popup = (sender as ListBox).Parent as Popup;
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    // Hide the Popup
                    popup.IsOpen = false;

                    ListBox lb = sender as ListBox;
                    if (lb == null)
                        return;

                    // Get the selected item value
                    string item = lb.SelectedItem.ToString();

                    // Save the Caret position
                    int i = textBox.CaretIndex - prefixLength;

                    // Add text to the text
                    if (prefixLength > 0)
                    {
                        textBox.Text = textBox.Text.Remove(i);
                    }
                    textBox.Text = textBox.Text.Insert(i, item);
                    
                    
                    // Move the caret to the end of the added text
                    textBox.CaretIndex = i + item.Length;

                    // Move focus back to the text box. 
                    // This will auto-hide the PopUp due to StaysOpen="false"
                    textBox.Focus();

                    CodeNodeTextBox cntextBox = textBox as CodeNodeTextBox;
                    if (cntextBox != null)
                        cntextBox.ReParseText();

                    break;

                case System.Windows.Input.Key.Escape:
                    // Hide the Popup
                    popup.IsOpen = false;
                    break;
            }            
        }

        internal void SetFocusOnList()
        {
            var list = this.popup.Child as ListBox;
            if (!list.IsKeyboardFocused)
                list.Focus();
        }
    }
}
