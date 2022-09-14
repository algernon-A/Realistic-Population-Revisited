// <copyright file="WhatsNewMessageListing.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using AlgernonCommons.Notifications;

    /// <summary>
    /// "What's new" update messages.
    /// </summary>
    internal class WhatsNewMessageListing
    {
        /// <summary>
        /// Gets the list of versions and associated update message lines (as translation keys).
        /// </summary>
        internal WhatsNewMessage[] Messages => new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                Version = new Version("2.1.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "RPR_21_0",
                },
            },
            new WhatsNewMessage
            {
                Version = new Version("2.0.1.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "RPR_201_0",
                },
            },
            new WhatsNewMessage
            {
                Version = new Version("2.0.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "RPR_200_0",
                    "RPR_200_1",
                    "RPR_200_2",
                    "RPR_200_3",
                    "RPR_200_4",
                    "RPR_200_5",
                    "RPR_200_6",
                    "RPR_200_7",
                    "RPR_200_8",
                    "RPR_200_9",
                },
            },
        };
    }
}