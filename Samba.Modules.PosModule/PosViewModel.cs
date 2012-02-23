﻿using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class PosViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;
        private readonly IRegionManager _regionManager;
        private readonly AccountTicketsViewModel _accountTicketsViewModel;
        private readonly MenuItemSelectorViewModel _menuItemSelectorViewModel;
        private readonly TicketExplorerViewModel _ticketExplorerViewModel;
        private readonly MenuItemSelectorView _menuItemSelectorView;

        [ImportingConstructor]
        public PosViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IUserService userService, TicketExplorerViewModel ticketExplorerViewModel,
            MenuItemSelectorViewModel menuItemSelectorViewModel, MenuItemSelectorView menuItemSelectorView,AccountTicketsViewModel accountTicketsViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _applicationState = applicationState;
            _regionManager = regionManager;
            _menuItemSelectorView = menuItemSelectorView;
            _accountTicketsViewModel = accountTicketsViewModel;

            _menuItemSelectorViewModel = menuItemSelectorViewModel;
            _ticketExplorerViewModel = ticketExplorerViewModel;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Account>>().Subscribe(OnAccountEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoginEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectedOrdersChanged)
                    {
                        if (x.Value.SelectedOrders.Count() != 1)
                            DisplayMenuScreen();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                 x =>
                 {
                     switch (x.Topic)
                     {
                         case EventTopicNames.ActivatePosView:
                             DisplayTickets();
                             DisplayMenuScreen();
                             break;
                         case EventTopicNames.ActivateTicket:
                             DisplaySingleTicket();
                             break;
                     }
                 });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateOpenTickets)
                    {
                        DisplayOpenTickets();
                    }
                });
        }

        private void OnAccountEvent(EventParameters<Account> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayAccountTickets)
            {
                DisplayAccountTickets(obj.Value);
            }
        }

        public CaptionCommand<Ticket> DisplayPaymentScreenCommand { get; set; }

        private void OnUserLoginEvent(EventParameters<User> obj)
        {
            if (obj.Topic == EventTopicNames.UserLoggedOut)
            {
                CloseTicket();
            }
        }

        private void CloseTicket()
        {
            if (_applicationState.CurrentTicket != null)
                _ticketService.CloseTicket(_applicationState.CurrentTicket);
            //todo fix
            //_ticketListViewModel.SelectedDepartment = null;
        }

        private void OnWorkPeriodEvent(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicketExplorer)
            {
                DisplayTicketExplorerScreen();
            }
        }

        private void OnTicketEvent(EventParameters<Ticket> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.PaymentSubmitted:
                    DisplayMenuScreen();
                    break;
            }
        }

        public void DisplayTickets()
        {
            if (_applicationState.CurrentTicket != null)
            {
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
                return;
            }

            _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);

            if (_applicationState.CurrentDepartment.IsAlaCarte && _applicationState.CurrentDepartment.LocationScreens.Count > 0)
            {
                _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.SelectAccountState);
            }
            else if (_applicationState.CurrentDepartment.IsTakeAway)
            {
                _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.SelectAccount);
            }
            else if (_applicationState.CurrentDepartment.IsFastFood)
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
        }

        private void DisplaySingleTicket()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketListView", UriKind.Relative));
        }

        public void DisplayOpenTickets()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("OpenTicketsView", UriKind.Relative));
        }

        public void DisplayAccountTickets(Account value)
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("AccountTicketsView", UriKind.Relative));
            _accountTicketsViewModel.SelectedAccount = value;
        }

        public void DisplayMenuScreen()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosSubRegion, new Uri("MenuItemSelectorView", UriKind.Relative));
        }

        public void DisplayTicketExplorerScreen()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosSubRegion, new Uri("TicketExplorerView", UriKind.Relative));

            _ticketExplorerViewModel.StartDate = _applicationState.CurrentWorkPeriod.StartDate.Date;
            if (!_userService.IsUserPermittedFor(PermissionNames.DisplayOldTickets))
            {
                _ticketExplorerViewModel.StartDate = _applicationState.CurrentWorkPeriod.StartDate;
            }
            _ticketExplorerViewModel.EndDate = DateTime.Now;
            _ticketExplorerViewModel.Refresh();
        }

        public bool HandleTextInput(string text)
        {
            return _regionManager.Regions[RegionNames.PosSubRegion].ActiveViews.Contains(_menuItemSelectorView)
                && _menuItemSelectorViewModel.HandleTextInput(text);
        }
    }
}
