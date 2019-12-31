using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Formula.SimpleMembership
{
    public class DisplayRecoveryCodesViewModel
    {
        [Required]
        public IEnumerable<string> Codes { get; set; }

    }
}
