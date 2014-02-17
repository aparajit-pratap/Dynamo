using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Windows;
using System.Windows.Threading;


using Dynamo;
using Dynamo.Nodes;
using Dynamo.Services;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Selection;
using Dynamo.Utilities;
using Dynamo.ViewModels;

// DesignScript
using ProtoCore.Mirror;
using ProtoCore.AST.AssociativeAST;


namespace DynamoWrapper
{
    public class DynamoCSharpWrapper
    {
        protected static DynamoController Controller { get; set; }

        protected static DynamoViewModel Vm { get; set; }

        protected static DynamoView Ui { get; set; }

        protected static DynamoModel Model { get; set; }

        public static void Initialize()
        {
            Controller = DynamoController.MakeSandbox();
            Controller.DynamoViewModel.DynamicRunEnabled = true;
        }

        public static void StartUp()
        {
            Controller = DynamoController.MakeSandbox();

            //create the view   
            //Controller.DynamoViewModel.DynamicRunEnabled = false;
            Ui = new DynamoView();
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Model = Controller.DynamoModel;
            Controller.UIDispatcher = Ui.Dispatcher;
            Ui.Show();

            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        public static void CreateGraphFromAst(AssociativeNode astNode, MethodMirror methodMirror)
        {
            Dynamo.DSEngine.AstToNode codeToNode = new Dynamo.DSEngine.AstToNode();
            codeToNode.CreateNodeFromAST(astNode, methodMirror);
        }

        /// <summary>
        /// TODO: Remove - temporary function to test graph creation
        /// </summary>
        public static void CreateGraph()
        {
            var model = dynSettings.Controller.DynamoModel;

            model.CreateNode(400.0, 100.0, "Dynamo.Nodes.Addition");
            model.CreateNode(100.0, 100.0, "Number");
            model.CreateNode(100.0, 300.0, "Number");
            model.CreateNode(100.0, 300.0, "Dynamo.Nodes.Watch");

            var num1 = Controller.DynamoViewModel.Model.Nodes[1] as DoubleInput;
            num1.Value = "2";
            var num2 = Controller.DynamoViewModel.Model.Nodes[2] as DoubleInput;
            num2.Value = "2";

            var cd1 = new Dictionary<string, object>();
            cd1.Add("start", Controller.DynamoViewModel.Model.Nodes[1]);
            cd1.Add("end", Controller.DynamoViewModel.Model.Nodes[0]);
            cd1.Add("port_start", 0);
            cd1.Add("port_end", 0);

            model.CreateConnection(cd1);

            var cd2 = new Dictionary<string, object>();
            cd2.Add("start", Controller.DynamoViewModel.Model.Nodes[2]); //first number node
            cd2.Add("end", Controller.DynamoViewModel.Model.Nodes[0]); //+ node
            cd2.Add("port_start", 0); //first output
            cd2.Add("port_end", 1); //second input

            model.CreateConnection(cd2);

            var cd3 = new Dictionary<string, object>();
            cd3.Add("start", Controller.DynamoViewModel.Model.Nodes[0]); // add
            cd3.Add("end", Controller.DynamoViewModel.Model.Nodes[3]); // watch
            cd3.Add("port_start", 0); //first output
            cd3.Add("port_end", 0); //second input

            model.CreateConnection(cd3);

            dynSettings.Controller.RunExpression(null);

            Thread.Sleep(250);

            bool isWatch = Controller.DynamoViewModel.Model.Nodes[3] is Watch;
            
            var w = (Watch)Controller.DynamoViewModel.Model.Nodes[3];
            double val = 0.0;
           
        }
    }
}
