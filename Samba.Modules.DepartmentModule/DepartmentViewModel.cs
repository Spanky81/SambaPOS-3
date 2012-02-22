﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [Export,PartCreationPolicy(CreationPolicy.NonShared)]
    public class DepartmentViewModel : EntityViewModelBase<Department>
    {
        private readonly IMenuService _menuService;
        private readonly IPriceListService _priceListService;

        [ImportingConstructor]
        public DepartmentViewModel(IMenuService menuService,IPriceListService priceListService)
        {
            _menuService = menuService;
            _priceListService = priceListService;
            AddLocationScreenCommand = new CaptionCommand<string>(string.Format(Resources.Select_f, Resources.LocationScreen), OnAddLocationScreen);
            DeleteLocationScreenCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.LocationScreen), OnDeleteLocationScreen, CanDeleteLocationScreen);
        }

        public int ScreenMenuId { get { return Model.ScreenMenuId; } set { Model.ScreenMenuId = value; } }

        private IEnumerable<ScreenMenu> _screenMenus;
        public IEnumerable<ScreenMenu> ScreenMenus
        {
            get { return _screenMenus ?? (_screenMenus = _menuService.GetScreenMenus()); }
            set { _screenMenus = value; }
        }

        private ObservableCollection<AccountScreen> _locationScreens;
        public ObservableCollection<AccountScreen> LocationScreens
        {
            get { return _locationScreens ?? (_locationScreens = new ObservableCollection<AccountScreen>(Model.LocationScreens.OrderBy(x => x.Order))); }
        }

        private IEnumerable<TicketTemplate> _ticketTemplates;
        public IEnumerable<TicketTemplate> TicketTemplates
        {
            get { return _ticketTemplates ?? (_ticketTemplates = Workspace.All<TicketTemplate>()); }
        }
        public TicketTemplate TicketTemplate { get { return Model.TicketTemplate; } set { Model.TicketTemplate = value; } }

        public IEnumerable<string> PriceTags { get { return _priceListService.GetTags(); } }
        public string PriceTag { get { return Model.PriceTag; } set { Model.PriceTag = value; } }

        public int OpenTicketViewColumnCount { get { return Model.OpenTicketViewColumnCount; } set { Model.OpenTicketViewColumnCount = value; } }

        public bool IsFastFood
        {
            get { return Model.IsFastFood; }
            set { Model.IsFastFood = value; }
        }

        public bool IsAlaCarte
        {
            get { return Model.IsAlaCarte; }
            set { Model.IsAlaCarte = value; }
        }

        public bool IsTakeAway
        {
            get { return Model.IsTakeAway; }
            set { Model.IsTakeAway = value; }
        }

        public AccountScreen SelectedLocationScreen { get; set; }
        
        public ICaptionCommand AddLocationScreenCommand { get; set; }
        public ICaptionCommand DeleteLocationScreenCommand { get; set; }

        private bool CanDeleteLocationScreen(string arg)
        {
            return SelectedLocationScreen != null;
        }

        private void OnDeleteLocationScreen(string obj)
        {
            Model.LocationScreens.Remove(SelectedLocationScreen);
            LocationScreens.Remove(SelectedLocationScreen);
        }

        private void OnAddLocationScreen(string obj)
        {
            var selectedValues =
                  InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<AccountScreen>().ToList<IOrderable>(),
                  Model.LocationScreens.ToList<IOrderable>(), Resources.LocationScreens, string.Format(Resources.ChooseLocationScreensForTicketTemplate_f, Model.Name),
                  Resources.LocationScreen, Resources.LocationScreens);

            foreach (AccountScreen selectedValue in selectedValues)
            {
                if (!Model.LocationScreens.Contains(selectedValue))
                    Model.LocationScreens.Add(selectedValue);
            }

            _locationScreens = new ObservableCollection<AccountScreen>(Model.LocationScreens.OrderBy(x => x.Order));

            RaisePropertyChanged(() => LocationScreens);
        }

        public override Type GetViewType()
        {
            return typeof(DepartmentView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Department;
        }

        protected override AbstractValidator<Department> GetValidator()
        {
            return new DepartmentValidator();
        }
    }

    internal class DepartmentValidator : EntityValidator<Department>
    {
        public DepartmentValidator()
        {
            RuleFor(x => x.TicketTemplate).NotNull();
        }
    }
}
