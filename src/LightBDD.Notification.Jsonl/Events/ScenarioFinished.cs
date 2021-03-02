﻿using System;
using System.Text.Json.Serialization;
using LightBDD.Notification.Jsonl.Models;

namespace LightBDD.Notification.Jsonl.Events
{
    public sealed class ScenarioFinished : Event
    {
        [JsonPropertyName("i")]
        public Guid Id { get; set; }
        [JsonPropertyName("d")]
        public string StatusDetails { get; set; }
        [JsonPropertyName("s")]
        public ExecutionStatus Status { get; set; }
    }
}