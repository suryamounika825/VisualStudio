﻿using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using GitHub.ViewModels;
using Octokit;
using ReactiveUI;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Helpers;
using GitHub.Extensions;
using GitHub.ViewModels.Dialog;

namespace GitHub.Authentication
{
    [Export(typeof(ITwoFactorChallengeHandler))]
    [Export(typeof(IDelegatingTwoFactorChallengeHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TwoFactorChallengeHandler : ReactiveObject, IDelegatingTwoFactorChallengeHandler
    {
        ILogin2FaViewModel twoFactorDialog;
        public IViewModel CurrentViewModel
        {
            get { return twoFactorDialog; }
            private set { this.RaiseAndSetIfChanged(ref twoFactorDialog, (ILogin2FaViewModel)value); }
        }

        public void SetViewModel(IViewModel vm)
        {
            CurrentViewModel = vm;
        }

        public async Task<TwoFactorChallengeResult> HandleTwoFactorException(TwoFactorAuthorizationException exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            await ThreadingHelper.SwitchToMainThreadAsync();

            var userError = new TwoFactorRequiredUserError(exception);
            var result = await twoFactorDialog.Show(userError);

            if (result != null)
            {
                return result;
            }
            else
            {
                throw exception;
            }
        }

        public async Task ChallengeFailed(Exception exception)
        {
            await ThreadingHelper.SwitchToMainThreadAsync();
            twoFactorDialog.Cancel();
        }
    }
}