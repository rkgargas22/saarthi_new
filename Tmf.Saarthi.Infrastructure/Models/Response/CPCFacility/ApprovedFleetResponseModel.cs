﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tmf.Saarthi.Infrastructure.Models.Response.CPCFacility
{
    public class ApprovedFleetResponseModel
    {
        [JsonPropertyName("VehicleID")]
        public long VehicleID { get; set; }

        [JsonPropertyName("VehicleNo")]
        public string VehicleNo { get; set; } = string.Empty;

        [JsonPropertyName("Comment")]
        public string Comment { get; set; } = string.Empty;

        [JsonPropertyName("ApprovedBy")]
        public string ApprovedBy { get; set; } = string.Empty;
    }
}
