﻿using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PosModule
{
    public class OrderStateButton
    {
        public OrderStateButton(OrderStateGroup orderStateGroup)
        {
            Model = orderStateGroup;
            Name = Model.ButtonHeader;
        }

        public OrderStateGroup Model { get; set; }
        public string Name { get; set; }
    }
}
