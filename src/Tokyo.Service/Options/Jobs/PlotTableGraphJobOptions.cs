using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Options.Jobs
{
    public class PlotTableGraphJobOptions : TrackerUserJobOptions
    {
        public enum GraphType { Timeline, Cake, Calendar }
        public GraphType OutputGraphType { get; set; } = GraphType.Timeline;

        // TODO interaction: https://stackoverflow.com/questions/13662525/how-to-get-pixel-coordinates-for-matplotlib-generated-scatterplot
    }
}
