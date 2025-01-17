﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An API wrapper that makes it easy to work with the status bar.
    /// </summary>
    public partial class Notifications
    {
        /// <summary>Provides access to the environment's status bar.</summary>
        public Task<IVsStatusbar> GetStatusBarAsync() => VS.GetRequiredServiceAsync<SVsStatusbar, IVsStatusbar>();

        /// <summary>Gets the current text from the status bar.</summary>
        public async Task<string?> GetStatusBarTextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusBarAsync();

                statusBar.GetText(out var pszText);
                return pszText;
            }
            catch (Exception ex)
            {
                VsShellUtilities.LogError(ex.Source, ex.ToString());
                return null;
            }
        }

        /// <summary>Sets the text in the status bar.</summary>
        public async Task SetStatusBarTextAsync(string text)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusBarAsync();

                statusBar.FreezeOutput(0);
                statusBar.SetText(text);
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        /// <summary>
        /// Shows the progress indicator in the status bar. 
        /// Set <paramref name="currentStep"/> and <paramref name="numberOfSteps"/> 
        /// to the same value to stop the progress.
        /// </summary>
        /// <param name="text">The text to display in the status bar.</param>
        /// <param name="currentStep">The current step number starting at 1.</param>
        /// <param name="numberOfSteps">The total number of steps to completion.</param>
        public async Task ShowStatusBarProgressAsync(string text, int currentStep, int numberOfSteps)
        {
            if (currentStep == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentStep), "currentStep must have a value of 1 or higher.");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsStatusbar? statusBar = await GetStatusBarAsync();

            statusBar.FreezeOutput(0);
            uint cookie = 0;

            // Start by resetting the status bar.
            if (currentStep == 1)
            {
                statusBar.Progress(ref cookie, 1, "", 0, 0);
            }

            // Then report progress.
            if (currentStep < numberOfSteps)
            {
                statusBar.Progress(ref cookie, 1, text, (uint)currentStep, (uint)numberOfSteps);
            }

            // And clear the status bar when done.
            else
            {
                statusBar.Progress(ref cookie, 1, "", 0, 0);
            }
        }

        /// <summary>Clears all text from the status bar.</summary>
        public async Task ClearStatusBarAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusBarAsync();

                statusBar.FreezeOutput(0);
                statusBar.Clear();
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        /// <summary>Starts the animation on the status bar.</summary>
        public async Task StartStatusBarAnimationAsync(StatusAnimation animation)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusBarAsync();
                object icon = (short)animation;

                statusBar.FreezeOutput(0);
                statusBar.Animation(1, ref icon);
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
        }

        /// <summary>Ends the animation on the status bar.</summary>
        public async Task EndStatusBarAnimationAsync(StatusAnimation animation)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                IVsStatusbar statusBar = await GetStatusBarAsync();
                object icon = (short)animation;

                statusBar.FreezeOutput(0);
                statusBar.Animation(0, ref icon);
                statusBar.FreezeOutput(1);
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }

        }
    }

    /// <summary>A list of built-in animation visuals for the status bar.</summary>
    public enum StatusAnimation
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        General = 0,
        Print = 1,
        Save = 2,
        Deploy = 3,
        Sync = 4,
        Build = 5,
        Find = 6
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}