﻿using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTag : Value, IOrderable
    {
        public OrderTag()
        {
            MaxQuantity = 1;
        }

        public string Name { get; set; }
        public int Order { get; set; }
        public int OrderTagGroupId { get; set; }
        public string UserString { get { return Name; } }
        public decimal Price { get; set; }
        public int MenuItemId { get; set; }
        public int MaxQuantity { get; set; }
    }
}
