

//using Dynamo.Controls;
//using Dynamo.ViewModels;
//using Dynamo.Wpf.ViewModels.Watch3D;
using DynamoSandbox;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DynamoSandboxWrapper
{
    public class DynamoWrapper
    {
        public static void LoadDynamo(string asmLocation)
        {
            //var model = Dynamo.Applications.StartupUtils.MakeModel(false);

            //var viewModel = DynamoViewModel.Start(
            //    new DynamoViewModel.StartConfiguration()
            //    {
            //        CommandFilePath = string.Empty,
            //        DynamoModel = model,
            //        Watch3DViewModel = HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(new Watch3DViewModelStartupParams(model), model.Logger),
            //        ShowLogin = true
            //    });

            //var view = new DynamoView(viewModel);
            //view.Show();

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            //Include Dynamo Core path in System Path variable for helix to load properly.
            UpdateSystemPathForProcess();

            var args = new string[] { };
            var setup = new DynamoCoreSetup(args);
            setup.StartUI(asmLocation);

        }

        public static string DynamoCorePath
        {
            get
            {
                return @"C:\Users\pratapa.ADS\Documents\GitHub\Dynamo\bin\AnyCPU\Debug";
            }
        }

        public static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyPath = string.Empty;
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";

            try
            {
                assemblyPath = Path.Combine(DynamoCorePath, assemblyName);
                if (File.Exists(assemblyPath))
                    return Assembly.LoadFrom(assemblyPath);

                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                assemblyPath = Path.Combine(assemblyDirectory, assemblyName);
                return (File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("The location of the assembly, {0} could not be resolved for loading.", assemblyPath), ex);
            }
        }

        private static void UpdateSystemPathForProcess()
        {
            var path =
                    Environment.GetEnvironmentVariable(
                        "Path",
                        EnvironmentVariableTarget.Process) + ";" + DynamoCorePath;
            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);
        }
    }
}
