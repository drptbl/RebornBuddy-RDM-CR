using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using ff14bot.Helpers;

namespace Kupo.Settings
{
    internal class RedMageSettings : KupoSettings
    {
        public RedMageSettings(string filename = "RedMage-KupoSettings") : base(filename)
        {
        }

        [Setting]
        [DefaultValue(true)]
        public bool UseAoe { get; set; }

        [Setting]
        [DefaultValue(3.0f)]
        public float MinMonstersToAoe { get; set; }

        [DefaultValue(true)]
        public bool UseCorpsACorps { get; set; }

        [DefaultValue(true)]
        public bool UseDisplacement { get; set; }

        [Setting]
        [DefaultValue(true)]
        public bool UseSwiftcast { get; set; }

        [Setting]
        [DefaultValue(true)]
        public bool UseAcceleration { get; set; }

        [Setting]
        [DefaultValue(true)]
        public bool UseEmbolden { get; set; }

        [Setting]
        [DefaultValue(true)]
        public bool UseLucidDreaming { get; set; }

        [Setting]
        [DefaultValue(70.0f)]
        public float LucidDreamingManaPercent { get; set; }

        [Setting]
        [DefaultValue(true)]
        public bool UseHeal { get; set; }

        [Setting]
        [DefaultValue(40.0f)]
        public float HealAtHealthPercent { get; set; }

    }



}
