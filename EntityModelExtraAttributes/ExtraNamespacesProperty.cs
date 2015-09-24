using System;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.Data.Entity.Design.Extensibility;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Konamiman.VsExtensions.EntityModelExtraAttributes
{
    class ExtraNamespacesProperty : EdmStringPropertyBase
    {
        public ExtraNamespacesProperty(XElement parent, PropertyExtensionContext context)
            : base(parent, context)
        {
        }

        [DisplayName("Extra namespaces")]
        [Description("Additional namespaces to be added to the generated classes code, one per line.")]
        [Category("Extra attributes")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [DefaultValue("")]
        public override string PropertyValue
        {
            get
            {
                return base.PropertyValue;
            }

            set
            {
                base.PropertyValue = value;
            }
        }
    }
}