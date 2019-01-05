using AngleSharp.Css;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class StyleMolecule
    {
        public int StyleMoleculeId { get; set; }
        [PreventDeveloperCodePropagation]
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public CaliforniaProject CaliforniaProject { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required, StringLength(255)]
        public string NameShort { get; set; }

        [StringLength(255)]
        public string HtmlTag { get; set; } = null;

        public int? ClonedFromStyleId { get; set; }
        public StyleMolecule ClonedFromStyle { get; set; }

        public List<StyleMolecule> CloneOfStyles { get; set; } = new List<StyleMolecule>(); // TODO rename ClonedStyles
        
        public int StyleForLayoutId { get; set; }
        public LayoutBase StyleForLayout { get; set; }
        
        public List<StyleMoleculeAtomMapping> MappedStyleAtoms { get; set; } = new List<StyleMoleculeAtomMapping>(); // TODO could be sorted into per device / per pseudo style dictionaries for client

        [NotMapped] // TODO calculation can be cached
        public bool IsPositionFixed // TODO document workaround TODO works only on responsive device == None
        {
            get  // TODO too expensive on every request
            {
                var value = MappedStyleAtoms.Any(a => /* TODO a.ResponsiveDevice.Name == "None" &&*/ a.StyleAtom.StyleAtomType == StyleAtomType.Box && a.StyleAtom.AppliedValues.Any(v => v.CssProperty == PropertyNames.Position && v.CssValue == "fixed")); // TODO magic string
                return value;
            }
        }

        [NotMapped] // TODO calculation can be cached
        public string TopCssValuePx // TODO document workaround TODO works only on responsive device == None
        {
            get  // TODO too expensive on every request
            {
                string value = null;
                if (IsPositionFixed)
                {
                    foreach (var mappedStyleAtom in MappedStyleAtoms.Where(a => /* TODO a.ResponsiveDevice.Name == "None" &&*/ a.StyleAtom.StyleAtomType == StyleAtomType.Box)) // TODO magic string
                    {
                        var styleValue = mappedStyleAtom.StyleAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == PropertyNames.Top);
                        if (styleValue != null)
                        {
                            value = styleValue.CssValue.EndsWith("px") ? styleValue.CssValue.Substring(0, styleValue.CssValue.Length - 2) : styleValue.CssValue;
                            break;
                        }
                    }
                }
                return value;
            }
        }

        [NotMapped] // TODO calculation can be cached
        public string LeftCssValuePx // TODO document workaround TODO works only on responsive device == None
        {
            get  // TODO too expensive on every request
            {
                string value = null;
                if (IsPositionFixed)
                {
                    foreach (var mappedStyleAtom in MappedStyleAtoms.Where(a => /* TODO a.ResponsiveDevice.Name == "None" &&*/ a.StyleAtom.StyleAtomType == StyleAtomType.Box)) // TODO magic string
                    {
                        var styleValue = mappedStyleAtom.StyleAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == PropertyNames.Left);
                        if (styleValue != null)
                        {
                            value = styleValue.CssValue.EndsWith("px") ? styleValue.CssValue.Substring(0, styleValue.CssValue.Length - 2) : styleValue.CssValue;
                            break;
                        }
                    }
                }
                return value;
            }
        }
    }
}
