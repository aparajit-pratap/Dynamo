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
        public static DynamoController Controller { get; private set; }

        protected static DynamoViewModel Vm { get; set; }

        protected static DynamoView Ui { get; set; }

        protected static DynamoModel Model { get; set; }

        public static void Initialize()
        {
            Controller = DynamoController.MakeSandbox();
            Controller.DynamoViewModel.DynamicRunEnabled = true;

            InitializeUI();
        }

        public static void StartUI(bool enableDynamicRun)
        {
            //create the view   
            if(Controller != null)
                Controller.DynamoViewModel.DynamicRunEnabled = enableDynamicRun;
            
            if(Ui != null)
                Ui.Show();
        }

        public static void StartUp()
        {
            Controller = DynamoController.MakeSandbox();

            InitializeUI();

            //create the view   
            StartUI(false);

            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

        private static void InitializeUI()
        {
            Ui = new DynamoView(true);
            Ui.DataContext = Controller.DynamoViewModel;
            Vm = Controller.DynamoViewModel;
            Model = Controller.DynamoModel;
            Controller.UIDispatcher = Ui.Dispatcher;
        }

        public static void CreateGraphFromAst(AssociativeNode astNode, MethodMirror methodMirror)
        {
            Dynamo.DSEngine.AstToNode codeToNode = new Dynamo.DSEngine.AstToNode();
            codeToNode.CreateNodeFromAST(astNode, methodMirror);
        }

    }
}
