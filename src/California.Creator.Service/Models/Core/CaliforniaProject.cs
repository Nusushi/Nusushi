using California.Creator.Service.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public class CaliforniaProject
    {
        public int CaliforniaProjectId { get; set; }
        [PreventDeveloperCodePropagation]
        public string CaliforniaStoreId { get; set; }
        [Required, PreventDeveloperCodePropagation]
        public CaliforniaStore CaliforniaStore { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; } = "UNSET_Project";

        public List<StyleValue> StyleValues { get; set; } = new List<StyleValue>();
        public List<StyleQuantum> StyleQuantums { get; set; } = new List<StyleQuantum>();
        public List<StyleAtom> StyleAtoms { get; set; } = new List<StyleAtom>();
        public List<StyleMolecule> StyleMolecules { get; set; } = new List<StyleMolecule>();

        public List<ResponsiveDevice> ResponsiveDevices { get; set; } = new List<ResponsiveDevice>();

        //[JsonIgnore] TODO uncomment, used as workaround to show layout molecule overview
        public List<LayoutBase> LayoutMolecules { get; set; } = new List<LayoutBase>();

        public List<LayoutStyleInteraction> LayoutStyleInteractions { get; set; } = new List<LayoutStyleInteraction>();

        public List<ContentAtom> ContentAtoms { get; set; } = new List<ContentAtom>();

        public List<PictureContent> PictureContents { get; set; } = new List<PictureContent>();

        public List<CaliforniaView> CaliforniaViews { get; set; } = new List<CaliforniaView>();
        public List<SharedProjectInfo> SharedProjectInfos { get; set; } = new List<SharedProjectInfo>();

        [Required]
        public int? ProjectDefaultsRevision { get; set; } = 0; // used for initial data

        [StringLength(CaliforniaServiceOptions.UserDefinedCssMaxLength)]
        public string UserDefinedCss { get; set; } = null;

        [NotMapped]
        public CaliforniaSelectionOverlay CurrentSelection { get; set; } = null;
    }
}
