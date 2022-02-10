using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace FCM
{
    internal sealed class FCMCommand
    {
        private const int _commandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("55165515-0f3b-4427-b4bc-5e023897d0cd");
        private static EnvDTE.DTE _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _dte = await package.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            Assumes.Present(_dte);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as IMenuCommandService;
            Assumes.Present(commandService);
            var cmdId = new CommandID(CommandSet, _commandId);

            var cmd = new OleMenuCommand(OnExecute, cmdId)
            {
                // This will defer visibility control to the VisibilityConstraints section in the .vsct file
                Supported = false
            };

            commandService.AddCommand(cmd);
        }

        private static void OnExecute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProjectItem item = _dte.SelectedItems.Item(1).ProjectItem;

            if (item != null)
            {
                item.Properties.Item("CustomTool").Value = SFG.Name;
            }
        }
    }
}
