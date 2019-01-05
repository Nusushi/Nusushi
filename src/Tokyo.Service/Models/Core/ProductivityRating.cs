using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    public class ProductivityRating
    {
        public int ProductivityRatingId { get; set; }
        public int TimeNormId { get; set; }
        public TimeNorm TimeNorm { get; set; }
        [Required, StringLength(255)]
        public string Name { get; set; } = "UNSET_PRODUCTIVITY";
        [Column(TypeName = "float(53)")]
        public double ProductivityPercentage { get; set; } = 0.0;
    }
}
