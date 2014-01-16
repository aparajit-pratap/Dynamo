using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Windows;
using System.Windows.Threading;


using Dynamo;
using Dynamo.Services;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;

namespace DynamoWrapper
{
    public class DynamoCSharpWrapper
    {
        protected static DynamoController Controller { get; set; }

        protected static DynamoViewModel Vm { get; set; }

        protected static DynamoView Ui { get; set; }

        protected static DynamoModel Model { get; set; }

        public static void StartUp()
        {
            
            Controller = DynamoController.MakeSandbox();
            //controller.Testing = true;

            //create the view   
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Model = Controller.DynamoModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();

            
            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
    }
}
