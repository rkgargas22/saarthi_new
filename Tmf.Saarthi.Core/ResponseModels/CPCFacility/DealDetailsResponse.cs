﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tmf.Saarthi.Core.ResponseModels.CPCFacility
{
    public class DealDetailsResponse
    {
        [JsonPropertyName("FleetID")]
        public long FleetID { get; set; }

        [JsonPropertyName("LoanAmount")]
        public decimal LoanAmount { get; set; } = 0;

        [JsonPropertyName("AIR")]
        public decimal AIR { get; set; } = 0;

        [JsonPropertyName("IRR")]
        public decimal IRR { get; set; } = 0;

        [JsonPropertyName("ProcessingFeeAmount")]
        public decimal ProcessingFeeAmount { get; set; } = 0;

        [JsonPropertyName("StampDutyAmount")]
        public decimal StampDutyAmount { get; set; } = 0;

        [JsonPropertyName("FacilityName")]
        public string FacilityName { get; set; } = string.Empty;

        [JsonPropertyName("FacilityDate")]
        public DateTime? FacilityDate { get; set; }

        [JsonPropertyName("FleetCount")]
        public int FleetCount { get; set; }
    }
}
