using System;
using System.Collections.Generic;

namespace ProjectPano.Model
{
    public class AddETCPopupFormModel
    {
        public List<vwCurves> ListCurves { get; set; } = new();
        public List<vwDiscETC> FilteredDiscETC { get; set; } = new();
        public List<ResourceDetailGroups> ListResourceGroups { get; set; } = new();
        public List<vwBudgetActuals_REVISED> vwBudgetActuals { get; set; } = new List<vwBudgetActuals_REVISED>();

        public DateTime RptWE { get; set; }

        // Optional - if you want to prefill a DiscETC record for editing or adding:
        public DiscETCDto DiscETC { get; set; }
    }
}
