﻿using System.Windows.Input;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;

namespace Samba.Modules.ResourceModule
{
    public class ResourceScreenItemViewModel : ObservableObject
    {
        private readonly bool _isTicketSelected;
        private readonly bool _userPermittedToMerge;
        private readonly ICommand _actionCommand;

        public ResourceScreenItemViewModel(ResourceScreenItem model, ResourceScreen screen,
            ICommand actionCommand, bool isTicketSelected, bool userPermittedToMerge, ResourceState state)
        {
            _actionCommand = actionCommand;
            _screen = screen;
            _isTicketSelected = isTicketSelected;
            _userPermittedToMerge = userPermittedToMerge;
            ResourceState = state;
            Model = model;
        }

        public ResourceState ResourceState { get; set; }

        private readonly ResourceScreen _screen;

        private ResourceScreenItem _model;
        public ResourceScreenItem Model
        {
            get { return _model; }
            set
            {
                _model = value;
                UpdateButtonColor();
            }
        }

        public string Name { get { return Model.Name; } }

        private string _buttonColor;
        public string ButtonColor
        {
            get { return _buttonColor; }
            set
            {
                if (_buttonColor != value)
                {
                    _buttonColor = value;
                    RaisePropertyChanged(() => ButtonColor);
                }
            }
        }

        public double ButtonHeight { get { return _screen.ButtonHeight > 0 ? _screen.ButtonHeight : double.NaN; } }

        public ICommand Command
        {
            get { return _actionCommand; }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; RaisePropertyChanged(() => IsEnabled); }
        }

        public void UpdateButtonColor()
        {
            IsEnabled = Model.ResourceId != 0 || !_isTicketSelected;
            if (_isTicketSelected && !_userPermittedToMerge) IsEnabled = false;
            if (ResourceState != null) ButtonColor = ResourceState.Color;
        }
    }
}
