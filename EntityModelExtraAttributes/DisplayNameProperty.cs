using System;
using System.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using Microsoft.Data.Entity.Design.Extensibility;

namespace Konamiman.VsExtensions.EntityModelExtraAttributes
{
    class DisplayNameProperty : EdmStringPropertyBase
    {
        public DisplayNameProperty(XElement parent, PropertyExtensionContext context)
            : base(parent, context)
        {
        }

        [DisplayName("Display name")]
        [Description("Property name as displayed in user interface.")]
        [Category("Extra attributes")]
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