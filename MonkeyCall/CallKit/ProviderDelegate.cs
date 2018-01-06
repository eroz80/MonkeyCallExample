﻿using System;
using Foundation;
using CallKit;
using UIKit;
using MonkeyCall.Helpers;

namespace MonkeyCall
{
    public class ProviderDelegate : CXProviderDelegate
    {
        #region Computed Properties
        public ActiveCallManager CallManager { get; set; }
        public CXProviderConfiguration Configuration { get; set; }
        public CXProvider Provider { get; set; }
        #endregion

        #region Constructors
        public ProviderDelegate(ActiveCallManager callManager)
        {
            // Save connection to call manager
            CallManager = callManager;

            // Define handle types
            var handleTypes = new[] { (NSNumber)(int)CXHandleType.PhoneNumber };

            // Get Image Mask
            var maskImage = UIImage.FromFile("telephone_receiver.png");

            // Setup the initial configurations
            Configuration = new CXProviderConfiguration("MonkeyCall")
            {
                MaximumCallsPerCallGroup = 1,
                SupportedHandleTypes = new NSSet<NSNumber>(handleTypes),
                IconTemplateImageData = maskImage.AsPNG(),
                // IconMaskImageData = maskImage.AsPNG(),
                RingtoneSound = "musicloop01.wav"
            };

            // Create a new provider
            Provider = new CXProvider(Configuration);

            // Attach this delegate
            Provider.SetDelegate(this, null);

        }
        #endregion

        #region Override Methods
        public override void DidReset(CXProvider provider)
        {
            // Remove all calls
            CallManager.Calls.Clear();
        }

        public override void PerformStartCallAction(CXProvider provider, CXStartCallAction action)
        {
            // Create new call record
            var activeCall = new ActiveCall(action.CallUuid, action.CallHandle.Value, true);

            // Monitor state changes
            activeCall.StartingConnectionChanged += (call) => {
                if (call.IsConnecting)
                {
                    // Inform system that the call is starting
                    Provider.ReportConnectingOutgoingCall(call.UUID, Tools.ConvertDateTimeToNSDate(call.StartedConnectingOn));
                }
            };

            activeCall.ConnectedChanged += (call) => {
                if (call.IsConnected)
                {
                    // Inform system that the call has connected
                    provider.ReportConnectedOutgoingCall(call.UUID, Tools.ConvertDateTimeToNSDate(call.ConnectedOn));
                }
            };

            // Start call
            activeCall.StartCall((successful) => {
                // Was the call able to be started?
                if (successful)
                {
                    // Yes, inform the system
                    action.Fulfill();

                    // Add call to manager
                    CallManager.Calls.Add(activeCall);
                }
                else
                {
                    // No, inform system
                    action.Fail();
                }
            });
        }

        public override void PerformEndCallAction(CXProvider provider, CXEndCallAction action)
        {
            // Find requested call
            var call = CallManager.FindCall(action.CallUuid);

            // Found?
            if (call == null)
            {
                // No, inform system and exit
                action.Fail();
                return;
            }

            // Attempt to answer call
            call.EndCall((successful) => {
                // Was the call successfully answered?
                if (successful)
                {
                    // Remove call from manager's queue
                    CallManager.Calls.Remove(call);

                    // Yes, inform system
                    action.Fulfill();
                }
                else
                {
                    // No, inform system
                    action.Fail();
                }
            });
        }

        public override void PerformSetHeldCallAction(CXProvider provider, CXSetHeldCallAction action)
        {
            // Find requested call
            var call = CallManager.FindCall(action.CallUuid);

            // Found?
            if (call == null)
            {
                // No, inform system and exit
                action.Fail();
                return;
            }

            // Update hold status
            call.IsOnHold = action.OnHold;

            // Inform system of success
            action.Fulfill();
        }

        public override void TimedOutPerformingAction(CXProvider provider, CXAction action)
        {
            // Inform user that the action has timed out
        }

        public override void DidActivateAudioSession(CXProvider provider, AVFoundation.AVAudioSession audioSession)
        {
            // Start the calls audio session here
        }

        public override void DidDeactivateAudioSession(CXProvider provider, AVFoundation.AVAudioSession audioSession)
        {
            // End the calls audio session and restart any non-call
            // related audio
        }

        public override void PerformAnswerCallAction(CXProvider provider, CXAnswerCallAction action)
        {
            // Find requested call
            var call = CallManager.FindCall(action.CallUuid);

            // Found?
            if (call == null)
            {
                // No, inform system and exit
                action.Fail();
                return;
            }

            // Attempt to answer call
            call.AnswerCall((successful) => {
                // Was the call successfully answered?
                if (successful)
                {
                    // Yes, inform system
                    action.Fulfill();
                }
                else
                {
                    // No, inform system
                    action.Fail();
                }
            });
        }
        #endregion

        #region Public Methods
        public void ReportIncomingCall(NSUuid uuid, string handle)
        {
            // Create update to describe the incoming call and caller
            var update = new CXCallUpdate
            {
                RemoteHandle = new CXHandle(CXHandleType.Generic, handle)
            };

            // Report incoming call to system
            Provider.ReportNewIncomingCall(uuid, update, (error) => {
                // Was the call accepted
                if (error == null)
                {
                    // Yes, report to call manager
                    CallManager.Calls.Add(new ActiveCall(uuid, handle, false));
                }
                else
                {
                    // Report error to user here
                    if(error.Code == (int)CXErrorCodeIncomingCallError.CallUuidAlreadyExists) {
                        // Handle duplicate call ID
                    } else if (error.Code == (int)CXErrorCodeIncomingCallError.FilteredByBlockList) {
                        // Handle call from blocked user
                    } else if (error.Code == (int)CXErrorCodeIncomingCallError.FilteredByDoNotDisturb) {
                        // Handle call while in do-not-disturb mode
                    } else {
                        // Handle unknown error
                    }
                    Console.WriteLine("Error: {0}", error);
                }
            });
        }
        #endregion
    }
}
