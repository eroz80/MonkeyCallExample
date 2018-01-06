using System;
using CoreFoundation;
using Foundation;


namespace MonkeyCall
{
    public class ActiveCall
    {
        #region Private Variables
        private bool isConnecting;
        private bool isConnected;
        private bool isOnhold;
        #endregion

        #region Computed Propierties
        public NSUuid UUID { get; set; }
        public bool IsOutgoing { get; set; }
        public string Handle { get; set; }
        public DateTime StartedConnectingOn { get; set; }
        public DateTime ConnectedOn { get; set; }
        public DateTime EndedOn { get; set; }
        public bool IsConnecting
        {
            get { return isConnecting; }
            set
            {
                isConnecting = value;
                if (isConnecting) StartedConnectingOn = DateTime.Now;
                RaiseStartingConnectionChanged();
            }
        }
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                if (isConnected)
                {
                    ConnectedOn = DateTime.Now;
                }
                else
                {
                    EndedOn = DateTime.Now;
                }
                RaiseConnectedChanged();
            }
        }
        public bool IsOnHold
        {
            get { return isOnhold; }
            set
            {
                isOnhold = value;
            }
        }
        #endregion

        #region Constructors
        public ActiveCall()
        {
        }
        public ActiveCall(NSUuid uuid, string handle, bool outgoing)
        {
            // Initialize
            this.UUID = uuid;
            this.Handle = handle;
            this.IsOutgoing = outgoing;
        }
        #endregion

        #region Public Methods
        public void StartCall(ActiveCallbackDelegate completionHandler)
        {
            // Simulate the call starting successfully
            completionHandler(true);

            // Simulate making a starting and completing a connection
            DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 3000), () => {
                // Note that the call is starting
                IsConnecting = true;

                // Simulate pause before connecting
                DispatchQueue.MainQueue.DispatchAfter(new DispatchTime(DispatchTime.Now, 1500), () => {
                    // Note that the call has connected
                    IsConnecting = false;
                    IsConnected = true;
                });
            });
        }

        public void AnswerCall(ActiveCallbackDelegate completionHandler)
        {
            // Simulate the call being answered
            IsConnected = true;
            completionHandler(true);
        }

        public void EndCall(ActiveCallbackDelegate completionHandler)
        {
            // Simulate the call ending
            IsConnected = false;
            completionHandler(true);
        }
        #endregion

        #region Events
        public delegate void ActiveCallbackDelegate(bool successful);
        public delegate void ActiveCallStateChangedDelegate(ActiveCall call);

        public event ActiveCallStateChangedDelegate StartingConnectionChanged;
        internal void RaiseStartingConnectionChanged()
        {
            StartingConnectionChanged?.Invoke(this);
        }

        public event ActiveCallStateChangedDelegate ConnectedChanged;
        internal void RaiseConnectedChanged()
        {
            ConnectedChanged?.Invoke(this);
        }
        #endregion
    }
}
