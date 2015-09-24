using System;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.Data.Entity.Design.Extensibility;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Konamiman.VsExtensions.EntityModelExtraAttributes
{
    class ExtraAttributesProperty : EdmStringPropertyBase
    {
        public ExtraAttributesProperty(XElement parent, PropertyExtensionContext context)
            : base(parent, context)
        {
        }

        [DisplayName("Extra attributes")]
        [Description("Additional .NET attributes that will be added to the classes generated for the entity model, one per line.")]
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