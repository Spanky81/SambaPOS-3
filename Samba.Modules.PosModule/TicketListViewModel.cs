﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketListViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public TicketListViewModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
            _tickets = new List<TicketButtonViewModel>();
            AddTicketCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Ticket).Replace(" ","\r"), OnAddTicket);
            MergeTicketsCommand = new CaptionCommand<string>(Resources.MergeTickets.Replace(" ", "\r"), OnMergeTickets, CanMergeTickets);
            CloseCommand = new CaptionCommand<string>(Resources.Close.Replace(" ", "\r"), OnCloseCommand);
        }

        private List<TicketButtonViewModel> _tickets;
        public List<TicketButtonViewModel> Tickets
        {
            get { return _tickets; }
        }

        public IEntity SelectedEntity { get; set; }
        public string ListName { get { return SelectedEntity != null ? SelectedEntity.Name : ""; } }
        public ICaptionCommand AddTicketCommand { get; set; }
        public ICaptionCommand MergeTicketsCommand { get; set; }
        public ICaptionCommand CloseCommand { get; set; }

        public string TotalRemainingAmountLabel { get { return _tickets != null ? Tickets.Sum(x => x.RemainingAmount).ToString(LocalSettings.DefaultCurrencyFormat) : ""; } }
        public int RowCount { get { return _tickets.Count() > 8 ? _tickets.Count() : 8; } }
        public int SelectedItemsCount { get { return _tickets.Count(x => x.IsSelected); } }

        private bool CanMergeTickets(string arg)
        {
            return SelectedItemsCount > 1;
        }

        private void OnCloseCommand(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnMergeTickets(string obj)
        {
            var result = _ticketService.MergeTickets(Tickets.Where(x => x.IsSelected).Select(x => x.TicketId));
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                InteractionService.UserIntraction.GiveFeedback(result.ErrorMessage);
                Tickets.ForEach(x=>x.IsSelected = false);
            }
            else
                ExtensionMethods.PublishIdEvent(result.TicketId, EventTopicNames.DisplayTicket);
        }

        private void OnAddTicket(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CreateTicket, true);
            var r = SelectedEntity as Resource;
            new EntityOperationRequest<Resource>(r, null).PublishEvent(EventTopicNames.ResourceSelected, true);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
        }

        public void UpdateListByResource(Resource resource)
        {
            if (resource != null)
            {
                SelectedEntity = resource;
                _tickets = _ticketService.GetOpenTickets(resource.Id).Select(x => new TicketButtonViewModel(x, resource)).ToList();
                Refresh();
            }
        }
        public void UpdateListByTicketTagGroup(TicketTagGroup tagGroup)
        {
            SelectedEntity = tagGroup;
            var tagValue = string.Format("\"TagName\":\"{0}\"", tagGroup.Name);
            _tickets = _ticketService.GetOpenTickets(x => !x.IsClosed && x.TicketTags.Contains(tagValue)).Select(x => new TicketButtonViewModel(x, null)).ToList();
            Refresh();
        }

        private void Refresh()
        {
            _tickets.ForEach(x => x.SelectionChanged = ItemSelectionChanged);
            RaisePropertyChanged(() => Tickets);
            RaisePropertyChanged(() => RowCount);
            RaisePropertyChanged(() => TotalRemainingAmountLabel);
            RaisePropertyChanged(() => ListName);
        }

        private void ItemSelectionChanged()
        {
            RaisePropertyChanged(() => SelectedItemsCount);
        }
    }
}
