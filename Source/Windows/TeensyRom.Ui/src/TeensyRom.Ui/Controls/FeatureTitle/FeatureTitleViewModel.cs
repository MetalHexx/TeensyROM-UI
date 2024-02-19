using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Controls.FeatureTitle
{
    public class FeatureTitleViewModel(string _title) : ReactiveObject
    {
        [Reactive] public string Title { get; set; } = _title;
    }
}
