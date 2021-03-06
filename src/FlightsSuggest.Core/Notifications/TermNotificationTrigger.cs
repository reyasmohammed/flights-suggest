﻿using System;
using FlightsSuggest.Core.Timelines;

namespace FlightsSuggest.Core.Notifications
{
    public class TermNotificationTrigger : INotificationTrigger
    {
        private readonly string term;

        public TermNotificationTrigger(string term)
        {
            this.term = term ?? throw new ArgumentNullException(nameof(term));
        }

        public bool ShouldNotify(FlightNews flightNews)
        {
            return flightNews.NormalizedText.Contains(term.ToLower());
        }

        public string Serialize()
        {
            return term;
        }
    }
}