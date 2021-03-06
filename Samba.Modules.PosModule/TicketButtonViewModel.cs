﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    public class TicketButtonViewModel : ObservableObject
    {
        private readonly OpenTicketData _openTicketData;
        private readonly int _baseResourceId;

        public DelegateCommand<string> OpenTicketCommand { get; set; }
        public DelegateCommand<string> SelectTicketCommand { get; set; }

        public TicketButtonViewModel(OpenTicketData openTicketData, Resource baseResource)
        {
            _openTicketData = openTicketData;
            _baseResourceId = baseResource != null ? baseResource.Id : 0;
            OpenTicketCommand = new DelegateCommand<string>(OnOpenTicketCommand);
            SelectTicketCommand = new DelegateCommand<string>(OnSelectTicket);
        }

        public bool IsSelected { get; set; }
        public string TicketNumber { get { return _openTicketData.TicketNumber; } }
        public decimal RemainingAmount { get { return _openTicketData.RemainingAmount; } }
        public string RemainingAmountLabel { get { return RemainingAmount.ToString(LocalSettings.DefaultCurrencyFormat); } }
        public IEnumerable<string> ResourceNames { get { return _openTicketData.TicketResources.Where(x => x.ResourceId != _baseResourceId).Select(x => x.ResourceName); } }
        public IEnumerable<TicketTagValue> TicketTags { get { return _openTicketData.TicketTagValues; } }

        public Action SelectionChanged { get; set; }

        public string SelectionBackground { get { return IsSelected ? SystemColors.HighlightBrush.Color.ToString() : "Silver"; } }
        public string SelectionForeground { get { return IsSelected ? SystemColors.HighlightTextBrush.Color.ToString() : "Black"; } }

        public int TicketId { get { return _openTicketData.Id; } }

        private void OnSelectTicket(string obj)
        {
            IsSelected = !IsSelected;
            RaisePropertyChanged(() => SelectionBackground);
            RaisePropertyChanged(() => SelectionForeground);
            if (SelectionChanged != null) SelectionChanged.Invoke();
        }

        private void OnOpenTicketCommand(string obj)
        {
            ExtensionMethods.PublishIdEvent(_openTicketData.Id, EventTopicNames.DisplayTicket);
        }

    }
}
