#pragma checksum "..\..\YTSignInDialog.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "ECF6EBE612596CBFE425E72A0CBA1D3BAA4B72F409CDB4BC52671D07D5BBB6FC"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using RedditVideoGenerator;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Wpf.Ui;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Navigation;
using Wpf.Ui.Converters;
using Wpf.Ui.Markup;
using Wpf.Ui.ValidationRules;


namespace RedditVideoGenerator {
    
    
    /// <summary>
    /// YTSignInDialog
    /// </summary>
    public partial class YTSignInDialog : Wpf.Ui.Controls.UiWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 59 "..\..\YTSignInDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image YTImage;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\YTSignInDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid FooterGrid;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\YTSignInDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid FooterButtonsGrid;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\YTSignInDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Button SignInBtn;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\YTSignInDialog.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Button CancelBtn;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/RedditVideoGenerator;component/ytsignindialog.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\YTSignInDialog.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 15 "..\..\YTSignInDialog.xaml"
            ((RedditVideoGenerator.YTSignInDialog)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.YTSignInDialog_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.YTImage = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            this.FooterGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.FooterButtonsGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 5:
            this.SignInBtn = ((Wpf.Ui.Controls.Button)(target));
            
            #line 92 "..\..\YTSignInDialog.xaml"
            this.SignInBtn.Click += new System.Windows.RoutedEventHandler(this.SignInBtn_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.CancelBtn = ((Wpf.Ui.Controls.Button)(target));
            
            #line 101 "..\..\YTSignInDialog.xaml"
            this.CancelBtn.Click += new System.Windows.RoutedEventHandler(this.CancelBtn_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

