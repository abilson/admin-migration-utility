﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AdminMigrationUtility.Helpers {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Javascript {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Javascript() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AdminMigrationUtility.Helpers.Javascript", typeof(Javascript).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;script type=&quot;text/javascript&quot;&gt;
        ///											$( document ).ready(function() {  
        ///												$(&quot;td:contains(&apos;Object Type:&apos;)&quot;).next().children(&quot;select&quot;).after(&quot;&lt;div id=&apos;AMUTemplateDownload&apos; style=&apos;display:inline&apos; /&gt;&quot;);
        ///												$(&quot;td:contains(&apos;Object Type:&apos;)&quot;).next().children(&quot;select&quot;).change(function() 
        ///													{
        ///														var selectedValue = $(this).find(&quot;option:selected&quot;).text();
        ///														if (selectedValue != &apos;Select...&apos;){
        ///															$(&quot;#AMUTemplateDownload&quot;).html(&quot;&lt;a class=&apos;inlin [rest of string was truncated]&quot;;.
        /// </summary>
        public static string ButtonTemplate {
            get {
                return ResourceManager.GetString("ButtonTemplate", resourceCulture);
            }
        }
    }
}
