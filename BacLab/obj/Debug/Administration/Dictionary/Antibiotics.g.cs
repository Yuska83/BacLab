﻿#pragma checksum "..\..\..\..\Administration\Dictionary\Antibiotics.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "48B29A50EA7AD36807D75CAF130E9C44"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using BacLab;
using BacLab.Administration;
using MaterialDesignThemes.MahApps;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Transitions;
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


namespace BacLab.Administration.Dictionary {
    
    
    /// <summary>
    /// Antibiotics
    /// </summary>
    public partial class Antibiotics : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 33 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock x_nameDictionary;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid x_antibioticsGrid;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock x_btn_delete;
        
        #line default
        #line hidden
        
        
        #line 71 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGridTemplateColumn x_save_button;
        
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
            System.Uri resourceLocater = new System.Uri("/BacLab;component/administration/dictionary/antibiotics.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
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
            this.x_nameDictionary = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.x_antibioticsGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 40 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
            this.x_antibioticsGrid.AddingNewItem += new System.EventHandler<System.Windows.Controls.AddingNewItemEventArgs>(this.x_antibioticsGrid_AddingNewItem);
            
            #line default
            #line hidden
            return;
            case 3:
            this.x_btn_delete = ((System.Windows.Controls.TextBlock)(target));
            
            #line 48 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
            this.x_btn_delete.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.x_btn_delete_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.x_save_button = ((System.Windows.Controls.DataGridTemplateColumn)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 5:
            
            #line 75 "..\..\..\..\Administration\Dictionary\Antibiotics.xaml"
            ((System.Windows.Controls.Primitives.ToggleButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.x_save_button_Checked);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

