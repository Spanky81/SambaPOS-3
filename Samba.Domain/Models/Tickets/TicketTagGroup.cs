﻿using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    internal enum TagTypes
    {
        Alphanumeric,
        Numeric,
        Price
    }

    public class TicketTagGroup : Entity, IOrderable
    {
        public int Order { get; set; }

        public bool FreeTagging { get; set; }
        public bool SaveFreeTags { get; set; }
        public string ButtonColorWhenTagSelected { get; set; }
        public string ButtonColorWhenNoTagSelected { get; set; }
        public bool ForceValue { get; set; }
        public bool AskBeforeCreatingTicket { get; set; }
        public int DataType { get; set; }

        private readonly IList<TicketTag> _ticketTags;
        public virtual IList<TicketTag> TicketTags
        {
            get { return _ticketTags; }
        }

        private readonly IList<TicketTagMap> _ticketTagMaps;
        public virtual IList<TicketTagMap> TicketTagMaps
        {
            get { return _ticketTagMaps; }
        }

        public bool IsNumeric { get { return IsDecimal || IsInteger; } }
        public bool IsAlphanumeric { get { return DataType == 0; } }
        public bool IsInteger { get { return DataType == 1; } }
        public bool IsDecimal { get { return DataType == 2; } }

        public string UserString
        {
            get { return Name; }
        }

        public TicketTagGroup()
        {
            _ticketTags = new List<TicketTag>();
            _ticketTagMaps = new List<TicketTagMap>();
            ButtonColorWhenNoTagSelected = "Gainsboro";
            ButtonColorWhenTagSelected = "Gainsboro";
            SaveFreeTags = true;
        }

        public TicketTagMap AddTicketTagMap()
        {
            var map = new TicketTagMap();
            TicketTagMaps.Add(map);
            return map;
        }
    }
}
