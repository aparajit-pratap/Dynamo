using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dynamo.UI
{
    class CodeCompletionHandler
    {
        TextBox textBox;

        public CodeCompletionHandler(TextBox tb)
        {
            Popup p = new Popup();
            var list = new ListBox();
            list.KeyDown += list_KeyDown;
            list.Items.Add("Hey");
            p.Child = list;

            p.Placement = PlacementMode.Custom;
            p.PlacementTarget = tb;
            textBox = tb;
            
            
            p.CustomPopupPlacementCallback = delegate(Size popupSize, Size targetSize, Point offset)
            {
                return new[] { new CustomPopupPlacement() };
            };
            
            p.IsOpen = true;

        }

        void list_KeyDown(object sender, KeyEventArgs e)
        {
            var item = (sender as ListBox).SelectedItem as string;
            if (item != null)
            {
                this.textBox.Text.Insert(textBox.CaretIndex, item);
            }
            ((sender as ListBox).Parent as Popup).IsOpen = false;
        }

    }
}
