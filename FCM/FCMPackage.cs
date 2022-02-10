using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FCM
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidFCMPackageString)]
    [ProvideCodeGenerator(typeof(SFG),SFG.Name , SFG.Discription, true, RegisterCodeBase = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]

    [ProvideUIContextRule(PackageGuids.guidFCMUIRuleString,
    name: "DGML File",
    expression: "Cs",
    termNames: new[] { "Cs" },
    termValues: new[] { "HierSingleSelectionName:.Cs$" })]

    public sealed class FCMPackage : AsyncPackage
    {
        public const string PackageGuidString = "981616b2-7e7e-4ed6-bf04-26b395a53bbb";
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await FCMCommand.InitializeAsync(this);
        }

    }
}
